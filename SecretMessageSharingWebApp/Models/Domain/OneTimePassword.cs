namespace SecretMessageSharingWebApp.Models.Domain
{
	public record OneTimePassword(string Code, long CreatedTimestamp)
	{
		public int AvailableValidationAttempts { get; set; }
	}
}
