using SecretMessageSharingWebApp.Models.Api.Responses;

namespace SecretMessageSharingWebApp.Services.Interfaces
{
	public interface ISecretMessageDeliveryNotificationHubService
	{
		void AddConnection(string connectionId);

		void RemoveConnection(string connectionId);

		Task<bool> SendNotification(SecretMessageDeliveryNotification secretMessageDeliveryNotification);
	}
}
