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

	[JsonPropertyName("recipientCity")]
	public string? RecipientCity { get; init; }

	[JsonPropertyName("recipientRegion")]
	public string? RecipientRegion { get; init; }

	[JsonPropertyName("recipientCountry")]
	public string? RecipientCountry { get; init; }

	[JsonPropertyName("recipientCountryCode")]
	public string? RecipientCountryCode { get; init; }

	[JsonPropertyName("recipientLocationSource")]
	public string? RecipientLocationSource { get; init; }

	[JsonPropertyName("recipientIsp")]
	public string? RecipientIsp { get; init; }

	[JsonPropertyName("recipientTimezone")]
	public string? RecipientTimezone { get; init; }

	[JsonPropertyName("recipientUtcOffsetMinutes")]
	public int? RecipientUtcOffsetMinutes { get; init; }

	[JsonPropertyName("recipientLanguage")]
	public string? RecipientLanguage { get; init; }

	[JsonPropertyName("otpVerified")]
	public bool OtpVerified { get; init; }
}
