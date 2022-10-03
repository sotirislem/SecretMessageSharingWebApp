using SecretMessageSharingWebApp.Models.Domain;
using System.Runtime.CompilerServices;

namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface ISecretMessagesService
{
	Task<SecretMessage> Store(SecretMessage secretMessage);

	(bool exists, OtpSettings? otp) VerifyExistence(string id);

	Task<SecretMessage?> Retrieve(string id);

	Task<bool> Delete(string id);

	IEnumerable<RecentlyStoredSecretMessage> GetRecentlyStoredSecretMessagesInfo(List<string> recentlyStoredSecretMessagesList);
}
