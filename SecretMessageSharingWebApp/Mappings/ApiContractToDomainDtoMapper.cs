using SecretMessageSharingWebApp.Models;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Mappings;

public static class ApiContractToDomainDtoMapper
{
	public static SecretMessage ToSecretMessage(this StoreNewSecretMessageRequest request, HttpContextClientInfo httpContextClientInfo) => new()
	{
		CreatedDateTime = httpContextClientInfo.RequestDateTime,
		Data = new SecretMessageData()
		{
			IV = request.SecretMessageData.IV,
			Salt = request.SecretMessageData.Salt,
			CT = request.SecretMessageData.CT
		},
		Otp = new OtpSettings()
		{
			Required = request.Otp.Required,
			RecipientsEmail = request.Otp.RecipientsEmail.Trim()
		},
		EncryptionKeySha256 = request.EncryptionKeySha256,
		CreatorClientId = request.ClientId,
		CreatorIP = httpContextClientInfo.ClientIP,
		CreatorClientInfo = httpContextClientInfo.ClientInfo
	};
}
