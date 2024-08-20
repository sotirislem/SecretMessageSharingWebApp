namespace SecretMessageSharingWebApp.Models;

public sealed record HttpContextClientInfo
{
	public DateTime RequestDateTime { get; init; }

	public string? ClientIP { get; init; }

	public string? ClientInfo { get; init; }
}
