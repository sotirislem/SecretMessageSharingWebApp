using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Hubs;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Services;

public sealed class SecretMessageDeliveryNotificationHubService(
		ILogger<SecretMessageDeliveryNotificationHubService> logger,
		IMemoryCacheService memoryCacheService,
		IHttpContextAccessor httpContextAccessor,
		IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> secretMessageDeliveryNotificationHub)
	: ISecretMessageDeliveryNotificationHubService
{
	private readonly ConcurrentDictionary<string, string> _activeClients = new();

	public void AddConnection(string clientId, string connectionId)
	{
		_activeClients.TryAdd(clientId, connectionId);
	}

	public void RemoveConnection(string clientId)
	{
		_activeClients.TryRemove(clientId, out _);
	}

	public async Task<bool> SendNotification(string clientId, SecretMessageDeliveryNotification secretMessageDeliveryNotification)
	{
		secretMessageDeliveryNotification.IsSelfNotification = IsSelfNotification(clientId);

		var notificationSent = await TrySend(clientId, secretMessageDeliveryNotification);
		if (!notificationSent)
		{
			SaveNotificationToMemoryCacheQueueForFutureDelivery(clientId, secretMessageDeliveryNotification);
		}

		return notificationSent;
	}

	public async Task SendAnyPendingNotificationFromMemoryCacheQueue(string clientId)
	{
		var memoryResult = memoryCacheService.GetValue<Queue<SecretMessageDeliveryNotification>>(clientId, Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue);
		if (!memoryResult.exists)
		{
			return;
		}

		var queue = memoryResult.value!;
		while (queue.TryDequeue(out var notification))
		{
			await TrySend(clientId, notification);
		}
	}

	private async Task<bool> TrySend(string clientId, SecretMessageDeliveryNotification secretMessageDeliveryNotification)
	{
		if (!_activeClients.TryGetValue(clientId, out string? connectionId))
		{
			return false;
		}

		await secretMessageDeliveryNotificationHub.Clients
				.Client(connectionId)
				.SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);

		logger.LogInformation("SecretMessageDeliveryNotificationHubService => Sent delivery notification to client: {clientId}", clientId);

		return true;
	}

	private void SaveNotificationToMemoryCacheQueueForFutureDelivery(string clientId, SecretMessageDeliveryNotification secretMessageDeliveryNotification)
	{
		var memoryResult = memoryCacheService.GetValue<Queue<SecretMessageDeliveryNotification>>(clientId, Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue);

		Queue<SecretMessageDeliveryNotification> secretMessageDeliveryNotificationQueue = memoryResult.exists
			? memoryResult.value!
			: new();

		secretMessageDeliveryNotificationQueue.Enqueue(secretMessageDeliveryNotification);

		memoryCacheService.SetValue(clientId, secretMessageDeliveryNotificationQueue, Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue);
	}

	private bool IsSelfNotification(string clientIdToSendNotification)
	{
		var currentClientId = httpContextAccessor.HttpContext!.GetRequestHeaderValue("Client-Id");

		return clientIdToSendNotification == currentClientId;
	}
}
