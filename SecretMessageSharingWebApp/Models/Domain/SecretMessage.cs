using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Domain
{
	public class SecretMessage
	{
		public string Id { get; init; }

		public DateTime CreatedDateTime { get; set; }

		public SecretMessageData Data { get; set; }

		public string? CreatorIP { get; set; }

		public string? CreatorClientInfo { get; set; }

		public OtpSettings Otp { get; init; }
	}

	public class OtpSettings
	{
		public bool Required { get; init; }

		public string RecipientsEmail { get; init; }
	}
}
