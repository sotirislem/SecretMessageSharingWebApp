using SecretMessageSharingWebApp.Models.Api.Responses;

namespace SecretMessageSharingWebApp.Models.Common;

/// <summary>
/// A delivery notification waiting for the creator's browser to reconnect.
/// </summary>
/// <remarks>
/// A self-read is only meant for the tab that created the message, so the pending entry has to
/// remember which tab it belongs to. A null <see cref="TargetTabId"/> means "every tab of the
/// client" and is used for notifications about messages opened by somebody else.
/// </remarks>
public sealed record PendingSecretMessageDeliveryNotification
{
	public SecretMessageDeliveryNotification Notification { get; init; }

	public string? TargetTabId { get; init; }
}
