using System.ComponentModel.DataAnnotations;

namespace SecretMessageSharingWebApp.Configuration;

public sealed record JwtConfigurationSettings
{
	[Required]
	[MinLength(length: 32, ErrorMessage = "Signing key is too short, use at least 256 bits (32 bytes).")]
	public string SigningKey { get; init; }
}
