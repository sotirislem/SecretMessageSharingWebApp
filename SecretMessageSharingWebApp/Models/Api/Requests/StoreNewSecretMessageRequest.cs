using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Api.Requests;

public sealed record StoreNewSecretMessageRequest
{
	public SecretMessageData SecretMessageData { get; init; }

	public OtpSettings Otp { get; init; }

	public string EncryptionKeySha256 { get; init; }
}