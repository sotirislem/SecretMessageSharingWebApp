﻿namespace SecretMessageSharingWebApp.Models.Api.Responses;

public sealed record SecretMessageDeliveryNotification
{
	public string MessageId { get; init; }

	public DateTime MessageCreatedOn { get; init; }

	public DateTime MessageDeliveredOn { get; init; }

	public string? RecipientIP { get; init; }

	public string? RecipientClientInfo { get; init; }
}
