using System.Text.Json.Serialization;

namespace SecretMessageSharingWebApp.Models.Api.Requests;

public sealed record ValidateSecretMessageOtpRequest
{
	[JsonPropertyName("otpCode")]
	public string OtpCode { get; init; }
}
