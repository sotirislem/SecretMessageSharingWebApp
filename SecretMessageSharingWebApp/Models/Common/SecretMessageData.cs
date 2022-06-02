namespace SecretMessageSharingWebApp.Models.Common
{
	public record SecretMessageData(string IV, string Salt, string CT);
}
