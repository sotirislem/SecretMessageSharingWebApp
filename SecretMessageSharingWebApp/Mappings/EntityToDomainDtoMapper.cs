using System.Text.Json;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Mappings;

public static class EntityToDomainDtoMapper
{
	public static SecretMessage ToDomain(this SecretMessageEntity entity) => new()
	{
		Id = entity.Id,
		CreatedDateTime = entity.CreatedDateTime,
		Data = JsonSerializer.Deserialize<SecretMessageData>(entity.JsonData)!,
		Otp = entity.Otp.ToDomain(),
		EncryptionKeySha256 = entity.EncryptionKeySha256,
		CreatorClientId = entity.CreatorClientId,
		CreatorIP = entity.CreatorIP,
		CreatorClientInfo = entity.CreatorClientInfo,
	};

	public static Models.Common.OtpSettings ToDomain(this Data.Entities.OtpSettings? entity) => new()
	{
		Required = entity?.Required ?? false,
		RecipientsEmail = entity?.RecipientsEmail ?? string.Empty
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

	public static RecentlyStoredSecretMessage ToRecentlyStoredSecretMessage(this SecretMessageEntity entity) => new()
	{
		Id = entity.Id,
		CreatedDateTime = entity.CreatedDateTime
	};

	public static RecentlyStoredSecretMessage ToRecentlyStoredSecretMessage(this GetLogEntity entity) => new()
	{
		Id = entity.SecretMessageId,
		CreatedDateTime = entity.SecretMessageCreatedDateTime!.Value,
		DeliveryDetails = new DeliveryDetails
		{
			DeliveredAt = entity.RequestDateTime,
			RecipientIP = MaskIpAddress(entity.RequestCreatorIP),
			RecipientClientInfo = entity.RequestClientInfo
		}
	};

	private static string? MaskIpAddress(string? ipAddress)
	{
		if (ipAddress?.Contains('.') is false)
		{
			return ipAddress;
		}

		return string.Concat(ipAddress.AsSpan(0, ipAddress!.LastIndexOf('.')), ".xxx");
	}
}
