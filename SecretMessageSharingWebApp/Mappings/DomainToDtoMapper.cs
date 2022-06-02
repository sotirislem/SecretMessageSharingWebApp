using Newtonsoft.Json;
using SecretMessageSharingWebApp.Data.Dto;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Mappings
{
	public static class DomainToDtoMapper
	{
		public static SecretMessageDto ToSecretMessageDto(this SecretMessage secretMessage)
		{
			return new SecretMessageDto
			{
				Id = Guid.NewGuid().ToString("N"),
				DeleteOnRetrieve = true,
				CreatedDateTime = secretMessage.CreatedDateTime,
				CreatorIP = secretMessage.CreatorIP,
				CreatorClientInfo = secretMessage.CreatorClientInfo,
				JsonData = JsonConvert.SerializeObject(secretMessage.Data)
			};
		}

		public static GetLogDto ToGetLogDto(this GetLog getLog)
		{
			return new GetLogDto
			{
				RequestDateTime = getLog.RequestDateTime,
				RequestCreatorIP = getLog.RequestCreatorIP,
				RequestClientInfo = getLog.RequestClientInfo,
				SecretMessageId = getLog.SecretMessageId,
				SecretMessageExisted = getLog.SecretMessageExisted,
				SecretMessageCreatedDateTime = getLog.SecretMessageCreatedDateTime,
				SecretMessageCreatorIP = getLog.SecretMessageCreatorIP,
				SecretMessageCreatorClientInfo = getLog.SecretMessageCreatorClientInfo
			};
		}
	}
}
