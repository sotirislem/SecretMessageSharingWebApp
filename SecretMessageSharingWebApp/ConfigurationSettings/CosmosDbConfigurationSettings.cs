using System.ComponentModel.DataAnnotations;

namespace SecretMessageSharingWebApp.ConfigurationSettings;

public sealed record CosmosDbConfigurationSettings
{
	[Required]
	public string Endpoint { get; init; }

	[Required]
	public string ApiKey { get; init; }

	[Required]
	public string DbName { get; init; }
}
