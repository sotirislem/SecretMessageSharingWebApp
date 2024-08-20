namespace SecretMessageSharingWebApp.Models.Common;

public sealed record DeliveryDetails
{
	public DateTime DeliveredAt { get; init; }

	public string? RecipientIP { get; init; }

	public string? RecipientClientInfo { get; init; }
}
