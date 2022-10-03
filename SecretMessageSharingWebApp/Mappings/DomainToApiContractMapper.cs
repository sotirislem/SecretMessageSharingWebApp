using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Domain;
using System;

namespace SecretMessageSharingWebApp.Mappings;

public static class DomainToApiContractMapper
{
	public static SecretMessageDeliveryNotification ToSecretMessageDeliveryNotification(this GetLog getLog)
	{
		return new SecretMessageDeliveryNotification
		{
			MessageId = getLog.SecretMessageId,
			MessageCreatedOn = getLog.SecretMessageCreatedDateTime!.Value,
			MessageDeliveredOn = getLog.RequestDateTime,
			RecipientIP = MaskIpAddress(getLog.RequestCreatorIP),
			RecipientClientInfo = getLog.RequestClientInfo
		};
	}

	public static Models.Api.Responses.RecentlyStoredSecretMessage ToApiRecentlyStoredSecretMessage(this Models.Domain.RecentlyStoredSecretMessage recentlyStoredSecretMessage)
	{
		return new Models.Api.Responses.RecentlyStoredSecretMessage
		{
			Id = recentlyStoredSecretMessage.Id,
			CreatedDateTime = recentlyStoredSecretMessage.CreatedDateTime,
			DeliveryDetails = (recentlyStoredSecretMessage.DeliveryDetails is null) ? null : new Models.Api.Responses.DeliveryDetails
			{
				DeliveredAt = recentlyStoredSecretMessage.DeliveryDetails.DeliveredAt,
				RecipientIP = MaskIpAddress(recentlyStoredSecretMessage.DeliveryDetails.RecipientIP),
				RecipientClientInfo = recentlyStoredSecretMessage.DeliveryDetails.RecipientClientInfo
			}
		};
	}

	private static string? MaskIpAddress(string? ipAddress)
	{
		if (ipAddress is null)
		{
			return null;
		}

		return string.Concat(ipAddress.AsSpan(0, ipAddress.LastIndexOf('.')), ".xxx");
	}
}
