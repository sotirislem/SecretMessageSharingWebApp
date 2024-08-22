using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Domain;

public sealed record SecretMessage
{
	public string Id { get; init; }

	public DateTime CreatedDateTime { get; init; }

	public SecretMessageData Data { get; init; }

	public OtpSettings Otp { get; init; }

	public string EncryptionKeySha256 { get; init; }

	public string CreatorClientId { get; init; }

	public string? CreatorIP { get; init; }

	public string? CreatorClientInfo { get; init; }
}