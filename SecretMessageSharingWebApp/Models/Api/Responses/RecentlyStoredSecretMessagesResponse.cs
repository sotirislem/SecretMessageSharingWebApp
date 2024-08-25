using System.Text.Json.Serialization;
using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Api.Responses;

public sealed record RecentlyStoredSecretMessagesResponse
{
	[JsonPropertyName("recentlyStoredSecretMessages")]
	public List<RecentlyStoredSecretMessage> RecentlyStoredSecretMessages { get; init; } = [];
}