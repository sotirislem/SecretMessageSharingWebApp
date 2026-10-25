using System.ComponentModel.DataAnnotations;

namespace SecretMessageSharingWebApp.ConfigurationSettings;

public sealed record RecipientLocationConfigurationSettings
{
	public bool Enabled { get; init; } = true;

	[Required]
	public string ReverseGeocodeEndpoint { get; init; } = "https://api.bigdatacloud.net/data/reverse-geocode-client";

	[Required]
	public string IpGeolocationEndpoint { get; init; } = "https://ipwho.is";

	// The lookup must never make a one-time message retrieval noticeably slower.
	[Range(0.5, 10)]
	public double TimeoutSeconds { get; init; } = 2.5;

	[Range(1, 1440)]
	public int IpCacheMinutes { get; init; } = 1440;

	// Opening the app from the developer's machine reports "localhost", which has no
	// public IP. Development can ask the provider to resolve the machine's own egress
	// address instead, so the approximate-location path is testable locally.
	public bool ResolveLocalRequestsThroughPublicIp { get; init; }
}
