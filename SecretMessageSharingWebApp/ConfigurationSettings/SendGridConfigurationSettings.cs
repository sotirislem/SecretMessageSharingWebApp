using System.ComponentModel.DataAnnotations;

namespace SecretMessageSharingWebApp.Configuration;

public sealed class SendGridConfigurationSettings
{
	//[Required]
	public string ApiKey { get; init; }

	//[Required]
	public string AuthSender { get; init; }
}
