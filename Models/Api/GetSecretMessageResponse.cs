namespace SecretMessageSharingWebApp.Models.Api
{
	public class GetSecretMessageResponse
	{
		public SecretMessageData SecretMessageData { get; set; }

		public bool DeliveryNotificationSent { get; set; }
	}
}
