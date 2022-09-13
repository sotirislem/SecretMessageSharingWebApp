using System.ComponentModel.DataAnnotations;

namespace SecretMessageSharingWebApp.Configuration
{
	public sealed class CosmosDbConfigurationSettings
	{
		[Required]
		public string Endpoint { get; init; }

		[Required]
		public string ApiKey { get; init; }

		[Required]
		public string DbName { get; init; }
	}
}
