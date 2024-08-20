namespace SecretMessageSharingWebApp.Models.Common;

public sealed record OtpSettings
{
	public bool Required { get; init; }

	public string RecipientsEmail { get; init; }
}