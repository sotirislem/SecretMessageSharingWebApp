using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Api.Responses;

public sealed record GetSecretMessageResponse
{
	public SecretMessageData Data { get; init; }

	public bool DeliveryNotificationSent { get; init; }
}