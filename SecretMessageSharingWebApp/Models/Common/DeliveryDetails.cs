using System.Text.Json.Serialization;

namespace SecretMessageSharingWebApp.Models.Common;

public sealed record DeliveryDetails
{

	[JsonPropertyName("deliveredAt")]
	public DateTime DeliveredAt { get; init; }

	[JsonPropertyName("recipientIP")]
	public string? RecipientIP { get; init; }

	[JsonPropertyName("recipientClientInfo")]
	public string? RecipientClientInfo { get; init; }
}
