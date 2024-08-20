namespace SecretMessageSharingWebApp.Models.Domain;

public sealed record GetLog
{
	public string Id { get; init; }

	public DateTime RequestDateTime { get; set; }

	public string? RequestCreatorIP { get; set; }

	public string? RequestClientInfo { get; set; }

	public string SecretMessageId { get; set; }

	public DateTime? SecretMessageCreatedDateTime { get; set; }

	public string? SecretMessageCreatorIP { get; set; }

	public string? SecretMessageCreatorClientInfo { get; set; }
}
