using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Api.Requests
{
	public class StoreNewSecretMessageRequest
	{
		public SecretMessageData SecretMessageData { get; init; }

		public OtpSettings Otp { get; init; }
	}

	public class OtpSettings
	{
		public bool Required { get; init; }

		public string RecipientsEmail { get; init; }
	}
}
