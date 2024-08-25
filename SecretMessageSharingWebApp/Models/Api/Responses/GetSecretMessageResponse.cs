using System.Text.Json.Serialization;
using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Api.Responses;

public sealed record GetSecretMessageResponse
{
	[JsonPropertyName("data")]
	public SecretMessageData Data { get; init; }

	[JsonPropertyName("deliveryNotificationSent")]
	public bool DeliveryNotificationSent { get; init; }
}