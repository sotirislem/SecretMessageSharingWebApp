namespace SecretMessageSharingWebApp.Models;

public sealed record HttpContextClientInfo
{
	public DateTime RequestDateTime { get; init; }

	public string? ClientIP { get; init; }

	public string? ClientInfo { get; init; }

	// The "Client-Id" header sent by the browser. All tabs of the same browser share it.
	public string? ClientId { get; init; }

	// Coordinates are only present when the recipient consented to share their device location.
	// They are used for one reverse-geocoding lookup and are never persisted.
	public double? ClientLatitude { get; init; }

	public double? ClientLongitude { get; init; }

	public string? ClientTimeZone { get; init; }

	public int? ClientUtcOffsetMinutes { get; init; }

	public string? ClientLanguage { get; init; }
}
