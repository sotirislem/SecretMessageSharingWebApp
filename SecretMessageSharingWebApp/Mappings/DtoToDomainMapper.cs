using System.Text.Json;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Mappings;

public static class DtoToDomainMapper
{
	public static SecretMessage ToSecretMessage(this SecretMessageDto secretMessageDto)
	{
		return new SecretMessage
		{
			Id = secretMessageDto.Id,
			CreatedDateTime = secretMessageDto.CreatedDateTime,
			CreatorClientInfo = secretMessageDto.CreatorClientInfo,
			CreatorIP = secretMessageDto.CreatorIP,
			Data = JsonSerializer.Deserialize<SecretMessageData>(secretMessageDto.JsonData)!,
			Otp = secretMessageDto.Otp.ToOtpSettings()
		};
	}

	public static Models.Domain.OtpSettings ToOtpSettings(this Data.Entities.OtpSettings? otpSettingsDto)
	{
		return new Models.Domain.OtpSettings
		{
			Required = otpSettingsDto?.Required ?? false,
			RecipientsEmail = otpSettingsDto?.RecipientsEmail ?? string.Empty
		};
	}

	public static GetLog ToGetLog(this GetLogDto getLogDto)
	{
		return new GetLog
		{
			Id = getLogDto.Id,
			RequestDateTime = getLogDto.RequestDateTime,
			RequestCreatorIP = getLogDto.RequestCreatorIP,
			RequestClientInfo = getLogDto.RequestClientInfo,
			SecretMessageId = getLogDto.SecretMessageId,
			SecretMessageCreatedDateTime = getLogDto.SecretMessageCreatedDateTime,
			SecretMessageCreatorIP = getLogDto.SecretMessageCreatorIP,
			SecretMessageCreatorClientInfo = getLogDto.SecretMessageCreatorClientInfo
		};
	}

	public static RecentlyStoredSecretMessage ToRecentlyStoredSecretMessage(this SecretMessageDto secretMessageDto)
	{
		return new RecentlyStoredSecretMessage
		{
			Id = secretMessageDto.Id,
			CreatedDateTime = secretMessageDto.CreatedDateTime
		};
	}

	public static RecentlyStoredSecretMessage ToRecentlyStoredSecretMessage(this GetLogDto getLogDto)
	{
		return new RecentlyStoredSecretMessage
		{
			Id = getLogDto.SecretMessageId,
			CreatedDateTime = getLogDto.SecretMessageCreatedDateTime!.Value,
			DeliveryDetails = new DeliveryDetails
			{
				DeliveredAt = getLogDto.RequestDateTime,
				RecipientIP = getLogDto.RequestCreatorIP,
				RecipientClientInfo = getLogDto.RequestClientInfo
			}
		};
	}
}
