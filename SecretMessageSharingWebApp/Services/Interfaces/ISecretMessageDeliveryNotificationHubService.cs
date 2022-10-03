using SecretMessageSharingWebApp.Models.Api.Responses;

namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface ISecretMessageDeliveryNotificationHubService
{
	void AddConnection(string connectionId, string clientId);

	void RemoveConnection(string clientId);

	Task SendAnyPendingSecretMessageDeliveryNotificationFromMemoryCacheQueue(string clientId);

	Task<bool> SendNotification(SecretMessageDeliveryNotification secretMessageDeliveryNotification);
}
