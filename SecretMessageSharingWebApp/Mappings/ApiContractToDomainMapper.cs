using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Mappings
{
	public static class ApiContractToDomainMapper
	{
		public static SecretMessage ToSecretMessage(this StoreNewSecretMessageRequest storeNewSecretMessageRequest)
		{
			return new SecretMessage
			{
				Data = new Models.Common.SecretMessageData(
					IV: storeNewSecretMessageRequest.Data.IV,
					Salt: storeNewSecretMessageRequest.Data.Salt,
					CT: storeNewSecretMessageRequest.Data.CT
				)
			};
		}
	}
}
