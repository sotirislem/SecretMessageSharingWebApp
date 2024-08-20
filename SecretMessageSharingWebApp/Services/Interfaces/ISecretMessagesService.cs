using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface ISecretMessagesService
{
	Task<SecretMessage> Store(SecretMessage secretMessage, string clientId);

	Task<(bool exists, OtpSettings? otpSettings)> Exists(string id);

	Task<SecretMessage?> Retrieve(string id);

	Task<bool> Delete(string id);

	Task<List<RecentlyStoredSecretMessage>> GetRecentlyStoredSecretMessagesInfo(List<string> recentlyStoredSecretMessagesList);
}
