using System.Text.Json;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Mappings;

public static class EntityToDomainDtoMapper
{
	public static SecretMessage ToDomain(this SecretMessageEntity secretMessageEntity) => new()
	{
		Id = secretMessageEntity.Id,
		CreatedDateTime = secretMessageEntity.CreatedDateTime,
		CreatorClientInfo = secretMessageEntity.CreatorClientInfo,
		CreatorIP = secretMessageEntity.CreatorIP,
		Data = JsonSerializer.Deserialize<SecretMessageData>(secretMessageEntity.JsonData)!,
		Otp = secretMessageEntity.Otp.ToDomain(),
		EncryptionKeySha256 = secretMessageEntity.EncryptionKeySha256
	};

	public static Models.Common.OtpSettings ToDomain(this Data.Entities.OtpSettings? otpSettingsEntity) => new()
	{
		Required = otpSettingsEntity?.Required ?? false,
		RecipientsEmail = otpSettingsEntity?.RecipientsEmail ?? string.Empty
	};

	public static GetLog ToDomain(this GetLogEntity getLogDto) => new()
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

	public static RecentlyStoredSecretMessage ToRecentlyStoredSecretMessage(this SecretMessageEntity secretMessageEntity) => new()
	{
		Id = secretMessageEntity.Id,
		CreatedDateTime = secretMessageEntity.CreatedDateTime
	};

	public static RecentlyStoredSecretMessage ToRecentlyStoredSecretMessage(this GetLogEntity getLogEntity) => new()
	{
		Id = getLogEntity.SecretMessageId,
		CreatedDateTime = getLogEntity.SecretMessageCreatedDateTime!.Value,
		DeliveryDetails = new DeliveryDetails
		{
			DeliveredAt = getLogEntity.RequestDateTime,
			RecipientIP = getLogEntity.RequestCreatorIP,
			RecipientClientInfo = getLogEntity.RequestClientInfo
		}
	};
}
