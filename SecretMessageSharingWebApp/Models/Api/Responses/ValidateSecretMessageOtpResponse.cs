using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Api.Requests;

public sealed class ValidateSecretMessageOtpResponse
{
	public bool IsValid { get; init; }

	public string? Token { get; init; }

	public bool CanRetry { get; init; }

	public bool HasExpired { get; init; }
}
