using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using SecretMessageSharingWebApp.Hubs;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Services;

public sealed class SecretMessageDeliveryNotificationHubService(
		ILogger<SecretMessageDeliveryNotificationHubService> logger,
		IMemoryCacheService memoryCacheService,
		IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> secretMessageDeliveryNotificationHub)
	: ISecretMessageDeliveryNotificationHubService
{
	private sealed record ClientConnection(string ClientId, string TabId);

	// connectionId -> client and tab it belongs to.
	private readonly Dictionary<string, ClientConnection> _connections = new(StringComparer.Ordinal);

	// clientId -> connectionIds and clientId|tabId -> connectionIds. A client can hold several
	// connections (multiple tabs, or a reconnect that overlaps with the previous connection's
	// disconnect), so every connection is tracked individually.
	private readonly Dictionary<string, HashSet<string>> _connectionsByClient = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, HashSet<string>> _connectionsByTab = new(StringComparer.OrdinalIgnoreCase);

	private readonly object _connectionsLock = new();

	// Serializes every read-modify-write of the pending notification queue. Two notifications
	// arriving at the same time must land in the same queue instead of overwriting each other.
	private readonly object _pendingQueueLock = new();

	public void AddConnection(string clientId, string tabId, string connectionId)
	{
		lock (_connectionsLock)
		{
			// A reconnect can register a connection id that is still present (for example when a
			// disconnect is replayed late). Remove the previous indexes first so they cannot leak.
			if (_connections.Remove(connectionId, out var previousConnection))
			{
				RemoveFromIndex(_connectionsByClient, previousConnection.ClientId, connectionId);
				RemoveFromIndex(_connectionsByTab, GetTabKey(previousConnection.ClientId, previousConnection.TabId), connectionId);
			}

			_connections[connectionId] = new ClientConnection(clientId, tabId);
			AddToIndex(_connectionsByClient, clientId, connectionId);
			AddToIndex(_connectionsByTab, GetTabKey(clientId, tabId), connectionId);
		}
	}

	public void RemoveConnection(string connectionId)
	{
		lock (_connectionsLock)
		{
			if (!_connections.Remove(connectionId, out var connection))
			{
				return;
			}

			// Only drop the index entries once they have no connections left. Removing them by
			// client id alone would unregister a newer connection that is still alive.
			RemoveFromIndex(_connectionsByClient, connection.ClientId, connectionId);
			RemoveFromIndex(_connectionsByTab, GetTabKey(connection.ClientId, connection.TabId), connectionId);
		}
	}

	public async Task<bool> SendNotification(string clientId, string? targetTabId, SecretMessageDeliveryNotification secretMessageDeliveryNotification)
	{
		try
		{
			if (await TrySend(clientId, targetTabId, secretMessageDeliveryNotification))
			{
				return true;
			}

			SaveNotificationToMemoryCacheQueueForFutureDelivery(clientId, targetTabId, secretMessageDeliveryNotification);

			return false;
		}
		catch (Exception exception)
		{
			// The message is deleted before the notification is sent, so a notification failure
			// must never surface to the reader as an error. The delivery notification is
			// best-effort; the reader still receives the message.
			logger.LogError(exception, "SecretMessageDeliveryNotificationHubService => Failed to send or queue delivery notification for client: {clientId}, tab: {tabId}", clientId, targetTabId ?? "-");

			return false;
		}
	}

	public async Task SendAnyPendingNotificationFromMemoryCacheQueue(string clientId, string tabId, string connectionId)
	{
		while (true)
		{
			var pendingNotification = TakeNextPendingNotification(clientId, tabId);

			if (pendingNotification is null)
			{
				return;
			}

			// Replay only to the connection that just reconnected. The other tabs of the client
			// were already connected when the notification was queued (otherwise it would have
			// been sent immediately), so they must not receive a duplicate.
			if (await TrySendToConnection(connectionId, pendingNotification.Notification))
			{
				continue;
			}

			// The connection went away again (or the send failed), so the notification has to
			// wait for the next reconnect.
			RequeuePendingNotification(clientId, pendingNotification);
			return;
		}
	}

	private async Task<bool> TrySend(string clientId, string? targetTabId, SecretMessageDeliveryNotification secretMessageDeliveryNotification)
	{
		var connectionIds = GetConnectionIds(clientId, targetTabId);
		if (connectionIds is null)
		{
			return false;
		}

		try
		{
			await secretMessageDeliveryNotificationHub.Clients
				.Clients(connectionIds)
				.SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);

			logger.LogInformation("SecretMessageDeliveryNotificationHubService => Sent delivery notification to client: {clientId}, tab: {tabId}", clientId, targetTabId ?? "-");

			return true;
		}
		catch (Exception exception)
		{
			logger.LogError(exception, "SecretMessageDeliveryNotificationHubService => Could not send delivery notification to client: {clientId}, tab: {tabId}", clientId, targetTabId ?? "-");

			return false;
		}
	}

	private async Task<bool> TrySendToConnection(string connectionId, SecretMessageDeliveryNotification secretMessageDeliveryNotification)
	{
		try
		{
			await secretMessageDeliveryNotificationHub.Clients
				.Client(connectionId)
				.SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);

			logger.LogInformation("SecretMessageDeliveryNotificationHubService => Replayed pending delivery notification to connection: {connectionId}", connectionId);

			return true;
		}
		catch (Exception exception)
		{
			logger.LogError(exception, "SecretMessageDeliveryNotificationHubService => Could not replay pending delivery notification to connection: {connectionId}", connectionId);

			return false;
		}
	}

	private string[]? GetConnectionIds(string clientId, string? targetTabId)
	{
		lock (_connectionsLock)
		{
			var connections = targetTabId is null
				? GetConnections(_connectionsByClient, clientId)
				: GetConnections(_connectionsByTab, GetTabKey(clientId, targetTabId));

			return connections?.ToArray();
		}
	}

	private void SaveNotificationToMemoryCacheQueueForFutureDelivery(string clientId, string? targetTabId, SecretMessageDeliveryNotification secretMessageDeliveryNotification)
	{
		lock (_pendingQueueLock)
		{
			var memoryResult = memoryCacheService.GetValue<ConcurrentQueue<PendingSecretMessageDeliveryNotification>>(
				clientId, Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue, remove: false);

			var queue = memoryResult.exists
				? memoryResult.value!
				: new ConcurrentQueue<PendingSecretMessageDeliveryNotification>();

			queue.Enqueue(new PendingSecretMessageDeliveryNotification
			{
				Notification = secretMessageDeliveryNotification,
				TargetTabId = targetTabId
			});

			// Refresh the expiration so the queue lives for the configured time after the most
			// recent notification, not after its first one. The cache entry is never removed by
			// the drain, so a notification enqueued during a drain cannot be lost.
			memoryCacheService.SetValue(clientId, queue, Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue);
		}
	}

	private void RequeuePendingNotification(string clientId, PendingSecretMessageDeliveryNotification pendingNotification)
	{
		lock (_pendingQueueLock)
		{
			var memoryResult = memoryCacheService.GetValue<ConcurrentQueue<PendingSecretMessageDeliveryNotification>>(
				clientId, Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue, remove: false);

			if (memoryResult.exists)
			{
				// Do not refresh the expiration: this notification is old, replaying it must not
				// extend its lifetime.
				memoryResult.value!.Enqueue(pendingNotification);
				return;
			}

			// The queue expired while it was being replayed. Recreate it so the notification is
			// not lost; it gets a fresh expiration because it is now the only entry.
			var queue = new ConcurrentQueue<PendingSecretMessageDeliveryNotification>();
			queue.Enqueue(pendingNotification);
			memoryCacheService.SetValue(clientId, queue, Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue);
		}
	}

	private PendingSecretMessageDeliveryNotification? TakeNextPendingNotification(string clientId, string tabId)
	{
		lock (_pendingQueueLock)
		{
			var memoryResult = memoryCacheService.GetValue<ConcurrentQueue<PendingSecretMessageDeliveryNotification>>(
				clientId, Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue, remove: false);

			if (!memoryResult.exists)
			{
				return null;
			}

			var queue = memoryResult.value!;
			PendingSecretMessageDeliveryNotification? nextNotification = null;
			var unavailableNotifications = new List<PendingSecretMessageDeliveryNotification>();

			while (queue.TryDequeue(out var pendingNotification))
			{
				if (nextNotification is null && IsDeliverableToTab(pendingNotification, tabId))
				{
					nextNotification = pendingNotification;
					continue;
				}

				// The notification belongs to another tab of the same client, or there are more
				// matching ones after the first. Keep them for their own reconnect.
				unavailableNotifications.Add(pendingNotification);
			}

			foreach (var pendingNotification in unavailableNotifications)
			{
				queue.Enqueue(pendingNotification);
			}

			return nextNotification;
		}
	}

	private static bool IsDeliverableToTab(PendingSecretMessageDeliveryNotification pendingNotification, string tabId)
	{
		return pendingNotification.TargetTabId is null
			|| string.Equals(pendingNotification.TargetTabId, tabId, StringComparison.OrdinalIgnoreCase);
	}

	private static HashSet<string>? GetConnections(Dictionary<string, HashSet<string>> connections, string key)
	{
		return connections.TryGetValue(key, out var connectionIds) && connectionIds.Count > 0
			? connectionIds
			: null;
	}

	private static string GetTabKey(string clientId, string tabId) => clientId + '|' + tabId;

	private static void AddToIndex(Dictionary<string, HashSet<string>> index, string key, string connectionId)
	{
		if (!index.TryGetValue(key, out var connectionIds))
		{
			connectionIds = new HashSet<string>(StringComparer.Ordinal);
			index.Add(key, connectionIds);
		}

		connectionIds.Add(connectionId);
	}

	private static void RemoveFromIndex(Dictionary<string, HashSet<string>> index, string key, string connectionId)
	{
		if (!index.TryGetValue(key, out var connectionIds))
		{
			return;
		}

		connectionIds.Remove(connectionId);

		if (connectionIds.Count == 0)
		{
			index.Remove(key);
		}
	}
}
