using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Domain;

public sealed record SecretMessage
{
	public string Id { get; init; }

	public DateTime CreatedDateTime { get; set; }

	public SecretMessageData Data { get; set; }

	public OtpSettings Otp { get; init; }

	public string EncryptionKeySha256 { get; init; }

	public string? CreatorIP { get; set; }

	public string? CreatorClientInfo { get; set; }
}