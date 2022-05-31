using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Api.Responses
{
	public class GetSecretMessageResponse
	{
		public SecretMessageData Data { get; init; }

		public bool DeliveryNotificationSent { get; init; }
	}
}
