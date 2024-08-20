namespace SecretMessageSharingWebApp.Models.Common;

public sealed record SecretMessageData
{
	public string IV { get; init; }

	public string Salt { get; init; }

	public string CT { get; init; }
}