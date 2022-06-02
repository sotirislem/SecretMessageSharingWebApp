using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Services.Interfaces
{
	public interface ISecretMessagesService
	{
		SecretMessage Store(SecretMessage secretMessage);

		Task<SecretMessage?> Retrieve(string id);

		Task<bool> Delete(string id);

		IEnumerable<RecentlyStoredSecretMessage> GetRecentlyStoredSecretMessagesInfo(IEnumerable<string> recentlyStoredSecretMessagesList);
	}
}
