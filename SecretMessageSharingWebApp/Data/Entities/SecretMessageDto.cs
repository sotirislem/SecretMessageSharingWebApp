using System.ComponentModel.DataAnnotations;

namespace SecretMessageSharingWebApp.Data.Entities
{
	public class SecretMessageDto
	{
		[Key]
		public string Id { get; private init; }

		public bool DeleteOnRetrieve { get; init; }

		public DateTime CreatedDateTime { get; init; }

		public string JsonData { get; init; }

		public string? CreatorIP { get; init; }

		public string? CreatorClientInfo { get; init; }

		public OtpSettings? Otp { get; init; }


		public SecretMessageDto()
		{
			Id = Guid.NewGuid().ToString("N");
		}
	}

	public class OtpSettings
	{
		public bool Required { get; private set; } = true;

		public string RecipientsEmail { get; init; }
	}
}
