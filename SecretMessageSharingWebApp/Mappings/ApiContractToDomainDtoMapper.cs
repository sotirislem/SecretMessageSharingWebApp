using SecretMessageSharingWebApp.Models;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Security;

namespace SecretMessageSharingWebApp.Mappings;

public static class ApiContractToDomainDtoMapper
{
	public static SecretMessage ToSecretMessage(this StoreNewSecretMessageRequest request, HttpContextClientInfo httpContextClientInfo) => new()
	{
		CreatedDateTime = httpContextClientInfo.RequestDateTime,
		Data = new SecretMessageData()
		{
			IV = request.SecretMessageData.IV,
			CT = request.SecretMessageData.CT
		},
		Otp = new OtpSettings()
		{
			Required = request.Otp.Required,
			RecipientsEmail = request.Otp.RecipientsEmail.Trim()
		},
		EncryptionKeySha256 = KeyHashVerifier.Create(request.EncryptionKeySha256),
		CreatorClientId = request.ClientId,
		CreatorTabId = request.TabId,
		CreatorIP = httpContextClientInfo.ClientIP,
		CreatorClientInfo = httpContextClientInfo.ClientInfo,
		NotificationEmail = string.IsNullOrWhiteSpace(request.NotificationEmail)
			? null
			: request.NotificationEmail.Trim()
	};
}
