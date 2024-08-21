using System.Text.Json.Serialization;

namespace SecretMessageSharingWebApp.Models.Api.Responses;

public sealed record SecretMessageDeliveryNotification
{
	[JsonPropertyName("messageId")]
	public string MessageId { get; init; }

	[JsonPropertyName("messageCreatedOn")]
	public DateTime MessageCreatedOn { get; init; }

	[JsonPropertyName("messageDeliveredOn")]
	public DateTime MessageDeliveredOn { get; init; }

	[JsonPropertyName("recipientIP")]
	public string? RecipientIP { get; init; }

	[JsonPropertyName("recipientClientInfo")]
	public string? RecipientClientInfo { get; init; }
}
