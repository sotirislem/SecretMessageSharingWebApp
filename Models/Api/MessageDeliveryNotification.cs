namespace SecretMessageSharingWebApp.Models.Api
{
	public class MessageDeliveryNotification
	{
		public DateTime MessageCreatedOn { get; set; }

		public DateTime MessageDeliveredOn { get; set; }

		public string RecipientIp { get; set; }

		public string RecipientClientInfo { get; set; }

	}
}
