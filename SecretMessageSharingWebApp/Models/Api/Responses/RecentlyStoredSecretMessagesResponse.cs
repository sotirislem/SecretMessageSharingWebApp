using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Api.Responses;

public sealed record RecentlyStoredSecretMessagesResponse
{
	public List<RecentlyStoredSecretMessage> RecentlyStoredSecretMessages { get; init; } = [];
}