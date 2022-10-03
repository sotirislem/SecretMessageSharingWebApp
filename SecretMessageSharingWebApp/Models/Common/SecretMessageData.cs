namespace SecretMessageSharingWebApp.Models.Common;

public sealed record SecretMessageData(string IV, string Salt, string CT);
