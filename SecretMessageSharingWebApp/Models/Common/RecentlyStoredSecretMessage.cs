using System.Text.Json.Serialization;

namespace SecretMessageSharingWebApp.Models.Common;

public sealed record RecentlyStoredSecretMessage
{
	[JsonPropertyName("id")]
	public string Id { get; init; }

	[JsonPropertyName("createdDateTime")]
	public DateTime CreatedDateTime { get; init; }

	[JsonPropertyName("deliveryDetails")]
	public DeliveryDetails? DeliveryDetails { get; init; }
}