using System.Text.Json;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Mappings;

public static class DomainToEntityDtoMapper
{
	public static SecretMessageEntity ToEntity(this SecretMessage secretMessage) => new()
	{
		CreatedDateTime = secretMessage.CreatedDateTime,
		JsonData = JsonSerializer.Serialize(secretMessage.Data),
		CreatorClientId = secretMessage.CreatorClientId,
		CreatorIP = secretMessage.CreatorIP,
		CreatorClientInfo = secretMessage.CreatorClientInfo,
		Otp = secretMessage.Otp.Required
			? new OtpSettings() { RecipientsEmail = secretMessage.Otp.RecipientsEmail }
			: null,
		EncryptionKeySha256 = secretMessage.EncryptionKeySha256
	};

	public static GetLogEntity ToEntity(this GetLog getLog) => new(getLog.SecretMessageId, getLog.RequestDateTime)
	{
		RequestDateTime = getLog.RequestDateTime,
		RequestCreatorIP = getLog.RequestCreatorIP,
		RequestClientInfo = getLog.RequestClientInfo,
		SecretMessageId = getLog.SecretMessageId,
		SecretMessageCreatedDateTime = getLog.SecretMessageCreatedDateTime,
		SecretMessageCreatorIP = getLog.SecretMessageCreatorIP,
		SecretMessageCreatorClientInfo = getLog.SecretMessageCreatorClientInfo
	};
}
