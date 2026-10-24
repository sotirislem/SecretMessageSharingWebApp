using FastEndpoints;

namespace SecretMessageSharingWebApp.Models.Api.Requests;

public sealed record GetSecretMessageRequest
{
	// The hash is sent only via header so it does not end up in URLs, logs, or analytics.
	// IsRequired = true prevents FastEndpoints from also binding this property from
	// query/route/form values, keeping the retrieval API header-only.
	[FromHeader("X-Key-Hash", IsRequired = true)]
	public string EncryptionKeySha256 { get; set; } = string.Empty;
}
