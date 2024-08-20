using SecretMessageSharingWebApp.Models.Api.Responses;

namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface ISecretMessageDeliveryNotificationHubService
{
	void AddConnection(string clientId, string connectionId);

	void RemoveConnection(string clientId);

	Task<bool> SendNotification(SecretMessageDeliveryNotification secretMessageDeliveryNotification);

	Task SendAnyPendingSecretMessageDeliveryNotificationFromMemoryCacheQueue(string clientId);
}
