namespace SecretMessageSharingWebApp.Models.Domain;

public sealed record OneTimePassword(string Code, long CreatedTimestamp)
{
	public int AvailableValidationAttempts { get; set; }
}
