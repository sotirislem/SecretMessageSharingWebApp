using System.ComponentModel.DataAnnotations;

namespace SecretMessageSharingWebApp.Configuration;

public sealed class JwtConfigurationSettings
{
	[Required]
	public string SigningKey { get; init; }
}
