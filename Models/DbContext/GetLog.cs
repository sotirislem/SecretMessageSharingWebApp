namespace SecretMessageSharingWebApp.Models.DbContext
{
	public class GetLog
	{
		public int Id { get; set; }

		public DateTime RequestDateTime { get; set; }

		public string? RequestCreatorIP { get; set; }

		public string? RequestClientInfo { get; set; }

		public string SecretMessageId { get; set; }

		public bool SecretMessageExisted { get; set; }

		public DateTime? SecretMessageCreatedDateTime { get; set; }

		public string? SecretMessageCreatorIP { get; set; }

		public string? SecretMessageCreatorClientInfo { get; set; }
	}
}
