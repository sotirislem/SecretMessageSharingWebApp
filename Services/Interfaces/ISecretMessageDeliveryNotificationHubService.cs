using SecretMessageSharingWebApp.Models.Api.Responses;

namespace SecretMessageSharingWebApp.Services.Interfaces
{
	public interface ISecretMessageDeliveryNotificationHubService
	{
		Task<bool> Send(SecretMessageDeliveryNotification secretMessageDeliveryNotification);
	}
}
