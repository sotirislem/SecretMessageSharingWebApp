using Microsoft.AspNetCore.SignalR;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Hubs;

public interface ISecretMessageDeliveryNotificationHub
{
	[HubMethodName("message-delivery-notification")]
	Task SendSecretMessageDeliveryNotification(SecretMessageDeliveryNotification secretMessageDeliveryNotification);
}

public sealed class SecretMessageDeliveryNotificationHub(
	ILogger<SecretMessageDeliveryNotificationHub> logger,
	ISecretMessageDeliveryNotificationHubService secretMessageDeliveryNotificationHubService) : Hub<ISecretMessageDeliveryNotificationHub>
{
	public const string Url = "signalr/secret-message-delivery-notification-hub";

	public override async Task OnConnectedAsync()
	{
		var clientId = GetQueryValue("client_id");
		if (Guid.TryParse(clientId, out var parsedClientId) is false)
		{
			logger.LogWarning("SecretMessageDeliveryNotificationHub => New connection aborted, ID: {connectionId}, ClientId: {clientId}", Context.ConnectionId, clientId);

			Context.Abort();
			return;
		}

		clientId = parsedClientId.ToString("D");

		if (Guid.TryParse(GetQueryValue("tab_id"), out var parsedTabId) is false)
		{
			logger.LogWarning("SecretMessageDeliveryNotificationHub => New connection aborted, ID: {connectionId}, TabId: {tabId}", Context.ConnectionId, GetQueryValue("tab_id"));

			Context.Abort();
			return;
		}

		var tabId = parsedTabId.ToString("D");

		secretMessageDeliveryNotificationHubService.AddConnection(clientId, tabId, Context.ConnectionId);

		logger.LogInformation("SecretMessageDeliveryNotificationHub => New connection established, ID: {connectionId}, ClientId: {clientId}, TabId: {tabId}", Context.ConnectionId, clientId, tabId);

		try
		{
			// Awaited and guarded: pending notifications have to be replayed before the connect
			// call completes, and a replay failure must not break the connection itself.
			await secretMessageDeliveryNotificationHubService.SendAnyPendingNotificationFromMemoryCacheQueue(clientId, tabId, Context.ConnectionId);
		}
		catch (Exception exception)
		{
			logger.LogError(exception, "SecretMessageDeliveryNotificationHub => Failed to replay pending notifications, ID: {connectionId}, ClientId: {clientId}", Context.ConnectionId, clientId);
		}

		await base.OnConnectedAsync();
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		// The connection id identifies which connection went away. Using only the client id
		// could unregister a newer connection that is still alive.
		secretMessageDeliveryNotificationHubService.RemoveConnection(Context.ConnectionId);

		logger.LogInformation("SecretMessageDeliveryNotificationHub => Connection terminated, ID: {connectionId}, ClientId: {clientId}", Context.ConnectionId, GetQueryValue("client_id"));

		if (exception is not null)
		{
			logger.LogError(exception, "SecretMessageDeliveryNotificationHub => Exception: {exceptionMessage}", exception.GetAllErrorMessages());
		}

		await base.OnDisconnectedAsync(exception);
	}

	private string? GetQueryValue(string key)
	{
		return Context.GetHttpContext()?.Request.Query[key].FirstOrDefault();
	}
}
