using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Mappings;

public static class ApiContractToDomainMapper
{
	public static SecretMessage ToSecretMessage(this StoreNewSecretMessageRequest storeNewSecretMessageRequest)
	{
		return new SecretMessage
		{
			Data = new Models.Common.SecretMessageData(
				IV: storeNewSecretMessageRequest.SecretMessageData.IV,
				Salt: storeNewSecretMessageRequest.SecretMessageData.Salt,
				CT: storeNewSecretMessageRequest.SecretMessageData.CT
			),
			Otp = new Models.Domain.OtpSettings()
			{
				Required = storeNewSecretMessageRequest.Otp.Required,
				RecipientsEmail = storeNewSecretMessageRequest.Otp.RecipientsEmail.Trim()
			}
		};
	}
}
