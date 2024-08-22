using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Mappings;

public static class DomainDtoToApiContractMapper
{
	public static SecretMessageDeliveryNotification ToSecretMessageDeliveryNotification(this GetLog getLog) => new()
	{
		MessageId = getLog.SecretMessageId,
		MessageCreatedOn = getLog.SecretMessageCreatedDateTime!.Value,
		MessageDeliveredOn = getLog.RequestDateTime,
		RecipientIP = MaskIpAddress(getLog.RequestCreatorIP),
		RecipientClientInfo = getLog.RequestClientInfo
	};

	private static string? MaskIpAddress(string? ipAddress)
	{
		if (string.IsNullOrWhiteSpace(ipAddress))
		{
			return null;
		}

		if (ipAddress.IndexOf('.') < 0)
		{
			return ipAddress;
		}

		return string.Concat(ipAddress.AsSpan(0, ipAddress.LastIndexOf('.')), ".xxx");
	}
}
