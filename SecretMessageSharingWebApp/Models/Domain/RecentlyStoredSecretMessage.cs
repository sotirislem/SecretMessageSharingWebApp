namespace SecretMessageSharingWebApp.Models.Domain
{
	public class RecentlyStoredSecretMessage
	{
		public string Id { get; init; }

		public DateTime CreatedDateTime { get; set; }

		public DeliveryDetails? DeliveryDetails { get; set; }
	}

	public class DeliveryDetails
	{
		public DateTime DeliveredAt { get; set; }

		public string? RecipientIP { get; set; }

		public string? RecipientClientInfo { get; set; }
	}
}
