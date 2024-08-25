using System.Text.Json.Serialization;
using FastEndpoints;
using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Api.Requests;

public sealed record StoreNewSecretMessageRequest
{
	[FromHeader("Client-Id")]
	public string ClientId { get; init; }

	[JsonPropertyName("secretMessageData")]
	public SecretMessageData SecretMessageData { get; init; }

	[JsonPropertyName("otp")]
	public OtpSettings Otp { get; init; }

	[JsonPropertyName("encryptionKeySha256")]
	public string EncryptionKeySha256 { get; init; }
}