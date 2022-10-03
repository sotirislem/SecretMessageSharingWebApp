using System.Text.Json;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Mappings;

public static class DomainToDtoMapper
{
	public static SecretMessageDto ToSecretMessageDto(this SecretMessage secretMessage)
	{
		return new SecretMessageDto
		{
			DeleteOnRetrieve = true,
			CreatedDateTime = secretMessage.CreatedDateTime,
			CreatorIP = secretMessage.CreatorIP,
			CreatorClientInfo = secretMessage.CreatorClientInfo,
			JsonData = JsonSerializer.Serialize(secretMessage.Data),
			Otp = secretMessage.Otp.Required ? new Data.Entities.OtpSettings()
			{
				RecipientsEmail = secretMessage.Otp.RecipientsEmail
			} : null
		};
	}

	public static GetLogDto ToGetLogDto(this GetLog getLog)
	{
		return new GetLogDto(getLog.SecretMessageId, getLog.RequestDateTime)
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
}
