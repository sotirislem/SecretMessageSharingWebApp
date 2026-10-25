using SecretMessageSharingWebApp.Models.Api.Responses;

namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface ISecretMessageDeliveryNotificationHubService
{
	void AddConnection(string clientId, string tabId, string connectionId);

	void RemoveConnection(string connectionId);

	Task<bool> SendNotification(string clientId, string? targetTabId, SecretMessageDeliveryNotification secretMessageDeliveryNotification);

	Task SendAnyPendingNotificationFromMemoryCacheQueue(string clientId, string tabId, string connectionId);
}
