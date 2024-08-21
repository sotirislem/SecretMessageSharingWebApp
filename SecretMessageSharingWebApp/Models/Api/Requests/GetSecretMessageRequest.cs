using FastEndpoints;

namespace SecretMessageSharingWebApp.Models.Api.Requests;

public sealed record GetSecretMessageRequest
{
	[QueryParam, BindFrom("keyHash")]
	public string EncryptionKeySha256 { get; set; }
}