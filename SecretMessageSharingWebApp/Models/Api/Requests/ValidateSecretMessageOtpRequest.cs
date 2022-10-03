using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Api.Requests;

public sealed class ValidateSecretMessageOtpRequest
{
	public string OtpCode { get; init; }
}
