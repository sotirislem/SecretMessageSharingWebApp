using System.Text.Json.Serialization;

namespace SecretMessageSharingWebApp.Models.Common;

public sealed record OtpSettings
{
	[JsonPropertyName("required")]
	public bool Required { get; init; }

	[JsonPropertyName("recipientsEmail")]
	public string RecipientsEmail { get; init; }
}