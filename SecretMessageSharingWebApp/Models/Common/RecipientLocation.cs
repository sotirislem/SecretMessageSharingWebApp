namespace SecretMessageSharingWebApp.Models.Common;

/// <summary>
/// City-level location of the recipient. Coordinates are never stored: they are only
/// sent to the reverse geocoder to resolve a city, and the result is what gets persisted.
/// </summary>
public sealed record RecipientLocation
{
	public string? City { get; init; }

	public string? Region { get; init; }

	public string? Country { get; init; }

	public string? CountryCode { get; init; }

	public string? Isp { get; init; }

	public string Source { get; init; } = Constants.LocationSources.Ip;
}
