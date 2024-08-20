namespace SecretMessageSharingWebApp.Models.Api.Requests;

public sealed record ValidateSecretMessageOtpRequest
{
	public string OtpCode { get; init; }
}
