namespace SecretMessageSharingWebApp.Data.Entities
{
	public class GetLogDto
	{
		public int Id { get; init; }

		public DateTime RequestDateTime { get; init; }

		public string? RequestCreatorIP { get; init; }

		public string? RequestClientInfo { get; init; }

		public string SecretMessageId { get; init; }

		public bool SecretMessageExisted { get; init; }

		public DateTime? SecretMessageCreatedDateTime { get; init; }

		public string? SecretMessageCreatorIP { get; init; }

		public string? SecretMessageCreatorClientInfo { get; init; }
	}
}
