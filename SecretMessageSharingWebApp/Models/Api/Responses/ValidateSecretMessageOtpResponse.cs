namespace SecretMessageSharingWebApp.Models.Api.Requests;

public sealed record ValidateSecretMessageOtpResponse
{
	public bool IsValid { get; init; }

	public bool HasExpired { get; init; }

	public string? AuthToken { get; init; }
}
