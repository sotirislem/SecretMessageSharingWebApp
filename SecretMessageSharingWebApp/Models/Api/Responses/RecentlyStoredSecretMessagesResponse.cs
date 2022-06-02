namespace SecretMessageSharingWebApp.Models.Api.Responses
{
	public class RecentlyStoredSecretMessagesResponse
	{
		public List<RecentlyStoredSecretMessage> RecentlyStoredSecretMessages { get; set; } = new();
	}

	public class RecentlyStoredSecretMessage
	{
		public string Id { get; init; }

		public DateTime CreatedDateTime { get; init; }

		public DeliveryDetails? DeliveryDetails { get; init; }
	}

	public class DeliveryDetails
	{
		public DateTime DeliveredAt { get; init; }

		public string? RecipientIP { get; init; }

		public string? RecipientClientInfo { get; init; }
	}
}
