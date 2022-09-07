using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Api.Requests
{
	public class ValidateSecretMessageOtpRequest
	{
		public string OtpCode { get; init; }
	}
}
