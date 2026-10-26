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

	// The browser tab that created the message. Used to deliver a self-read notification to
	// that tab only, because all tabs of a browser share the same client id.
	public string? CreatorTabId { get; init; }

	public string? CreatorIP { get; init; }

	public string? CreatorClientInfo { get; init; }

	// Optional address that receives the delivery details by email once the message is opened.
	public string? NotificationEmail { get; init; }
}