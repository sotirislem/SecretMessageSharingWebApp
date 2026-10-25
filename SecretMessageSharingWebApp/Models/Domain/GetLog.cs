namespace SecretMessageSharingWebApp.Models.Domain;

public sealed record GetLog
{
	public string Id { get; init; }

	public DateTime RequestDateTime { get; init; }

	public string? RequestCreatorIP { get; init; }

	public string? RequestClientInfo { get; init; }

	public string? RecipientCity { get; init; }

	public string? RecipientRegion { get; init; }

	public string? RecipientCountry { get; init; }

	public string? RecipientCountryCode { get; init; }

	public string? RecipientLocationSource { get; init; }

	public string? RecipientIsp { get; init; }

	public string? RecipientTimezone { get; init; }

	public int? RecipientUtcOffsetMinutes { get; init; }

	public string? RecipientLanguage { get; init; }

	public bool OtpVerified { get; init; }

	public string SecretMessageId { get; init; }

	public DateTime? SecretMessageCreatedDateTime { get; init; }

	public string? SecretMessageCreatorIP { get; init; }

	public string? SecretMessageCreatorClientInfo { get; init; }
}
