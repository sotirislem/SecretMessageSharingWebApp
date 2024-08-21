using System.Text.Json.Serialization;

namespace SecretMessageSharingWebApp.Models.Api.Responses;

public sealed record VerifySecretMessageResponse
{
	[JsonPropertyName("id")]
	public string Id { get; init; }

	[JsonPropertyName("exists")]
	public bool Exists { get; init; }

	[JsonPropertyName("requiresOtp")]
	public bool? RequiresOtp { get; init; }
}
