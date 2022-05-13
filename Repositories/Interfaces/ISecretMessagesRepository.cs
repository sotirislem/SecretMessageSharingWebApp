using SecretMessageSharingWebApp.Models.DbContext;

namespace SecretMessageSharingWebApp.Repositories
{
	public interface ISecretMessagesRepository
	{
		SecretMessage Store(SecretMessage secretMessage);

		Task<SecretMessage?> Get(string id);

		void DeleteOldMessages();
	}
}
