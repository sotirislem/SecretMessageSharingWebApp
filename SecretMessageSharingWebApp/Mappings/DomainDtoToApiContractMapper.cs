using SecretMessageSharingWebApp.Extensions;
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
		RecipientIP = getLog.RequestCreatorIP.MaskIpAddress(),
		RecipientClientInfo = getLog.RequestClientInfo,
		RecipientCity = getLog.RecipientCity,
		RecipientRegion = getLog.RecipientRegion,
		RecipientCountry = getLog.RecipientCountry,
		RecipientCountryCode = getLog.RecipientCountryCode,
		RecipientLocationSource = getLog.RecipientLocationSource,
		RecipientIsp = getLog.RecipientIsp,
		RecipientTimezone = getLog.RecipientTimezone,
		RecipientUtcOffsetMinutes = getLog.RecipientUtcOffsetMinutes,
		RecipientLanguage = getLog.RecipientLanguage,
		OtpVerified = getLog.OtpVerified
	};
}
