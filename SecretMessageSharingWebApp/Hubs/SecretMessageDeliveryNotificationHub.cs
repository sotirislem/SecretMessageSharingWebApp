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

	public override Task OnConnectedAsync()
	{
		var clientId = GetClientId();
		if (string.IsNullOrEmpty(clientId))
		{
			Context.Abort();
			return Task.CompletedTask;
		}

		secretMessageDeliveryNotificationHubService.AddConnection(clientId, Context.ConnectionId);
		secretMessageDeliveryNotificationHubService.SendAnyPendingSecretMessageDeliveryNotificationFromMemoryCacheQueue(clientId);

		logger.LogInformation("SecretMessageDeliveryNotificationHub => New connection, ID: {connectionId}, Client: {clientId}", Context.ConnectionId, clientId);

		return base.OnConnectedAsync();
	}

	public override Task OnDisconnectedAsync(Exception? exception)
	{
		var clientId = GetClientId();
		if (!string.IsNullOrEmpty(clientId))
		{
			secretMessageDeliveryNotificationHubService.RemoveConnection(clientId);
			logger.LogInformation("SecretMessageDeliveryNotificationHub => Connection terminated, ID: {connectionId}, Client: {clientId}", Context.ConnectionId, clientId);
		}

		if (exception is not null)
		{
			logger.LogError(exception, "SecretMessageDeliveryNotificationHub => Exception: {exceptionMessage}", exception.GetAllErrorMessages());
		}

		return base.OnDisconnectedAsync(exception);
	}

	private string? GetClientId()
	{
		return Context.GetHttpContext()?.Request.Query
			.Where(x => x.Key == "client_id")
			.Select(x => x.Value.ToString())
			.FirstOrDefault();
	}
}
