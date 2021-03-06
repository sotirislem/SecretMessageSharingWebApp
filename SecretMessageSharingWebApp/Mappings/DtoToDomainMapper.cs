using Newtonsoft.Json;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Mappings
{
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
				Data = JsonConvert.DeserializeObject<SecretMessageData>(secretMessageDto.JsonData)!
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
				SecretMessageExisted = getLogDto.SecretMessageExisted,
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
				DeliveryDetails = new DeliveryDetails {
					DeliveredAt = getLogDto.RequestDateTime,
					RecipientIP = getLogDto.RequestCreatorIP,
					RecipientClientInfo = getLogDto.RequestClientInfo
				}
			};
		}
	}
}
