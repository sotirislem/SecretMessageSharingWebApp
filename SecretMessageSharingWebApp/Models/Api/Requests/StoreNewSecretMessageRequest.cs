using System.Text.Json.Serialization;
using FastEndpoints;
using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Api.Requests;

public sealed record StoreNewSecretMessageRequest
{
	[FromHeader("Client-Id")]
	public string ClientId { get; init; }

	// Identifies the browser tab that creates the message. It survives a refresh through
	// sessionStorage, which lets a self-read notification be delivered to that tab only.
	[FromHeader("Tab-Id")]
	public string? TabId { get; init; }

	[JsonPropertyName("secretMessageData")]
	public SecretMessageData? SecretMessageData { get; init; }

	[JsonPropertyName("otp")]
	public OtpSettings? Otp { get; init; }

	[JsonPropertyName("encryptionKeySha256")]
	public string EncryptionKeySha256 { get; init; }

	// Optional address that receives the delivery details by email once the message is opened.
	[JsonPropertyName("notificationEmail")]
	public string? NotificationEmail { get; init; }
}