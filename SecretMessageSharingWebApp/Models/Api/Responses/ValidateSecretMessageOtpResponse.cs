using System.Text.Json.Serialization;

namespace SecretMessageSharingWebApp.Models.Api.Requests;

public sealed record ValidateSecretMessageOtpResponse
{
	[JsonPropertyName("isValid")]
	public bool IsValid { get; init; }

	[JsonPropertyName("canRetry")]
	public bool CanRetry { get; init; }

	[JsonPropertyName("hasExpired")]
	public bool HasExpired { get; init; }

	[JsonPropertyName("authToken")]
	public string? AuthToken { get; init; }
}
