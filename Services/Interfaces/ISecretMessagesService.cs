using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Services
{
	public interface ISecretMessagesService
	{
		SecretMessage Insert(SecretMessage secretMessage);

		Task<SecretMessage?> Retrieve(string id);

		Task<bool> Delete(string id);

		IEnumerable<RecentlyStoredSecretMessage> GetRecentlyStoredSecretMessagesInfo(IEnumerable<string> recentlyStoredSecretMessagesList);
	}
}
