namespace SecretMessageSharingWebApp.Models;

public sealed record HttpContextClientInfo
{
	public DateTime RequestDateTime { get; init; }

	public string? ClientIP { get; init; }

	public string? ClientInfo { get; init; }

	// The "Client-Id" header sent by the browser. All tabs of the same browser share it.
	public string? ClientId { get; init; }
}
