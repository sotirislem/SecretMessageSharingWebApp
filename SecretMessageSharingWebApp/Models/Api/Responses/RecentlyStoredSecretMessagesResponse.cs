namespace SecretMessageSharingWebApp.Models.Api.Responses;

public sealed class RecentlyStoredSecretMessagesResponse
{
	public List<RecentlyStoredSecretMessage> RecentlyStoredSecretMessages { get; set; } = new();
}

public sealed class RecentlyStoredSecretMessage
{
	public string Id { get; init; }

	public DateTime CreatedDateTime { get; init; }

	public DeliveryDetails? DeliveryDetails { get; init; }
}

public sealed class DeliveryDetails
{
	public DateTime DeliveredAt { get; init; }

	public string? RecipientIP { get; init; }

	public string? RecipientClientInfo { get; init; }
}
