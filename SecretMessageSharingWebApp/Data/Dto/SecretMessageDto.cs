namespace SecretMessageSharingWebApp.Data.Dto
{
	public class SecretMessageDto
	{
		public string Id { get; init; }

		public bool DeleteOnRetrieve { get; init; }

		public DateTime CreatedDateTime { get; init; }

		public string JsonData { get; init; }

		public string? CreatorIP { get; init; }

		public string? CreatorClientInfo { get; init; }
	}
}
