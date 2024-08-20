namespace SecretMessageSharingWebApp.Models.Common;

public sealed record RecentlyStoredSecretMessage
{
	public string Id { get; init; }

	public DateTime CreatedDateTime { get; init; }

	public DeliveryDetails? DeliveryDetails { get; init; }
}