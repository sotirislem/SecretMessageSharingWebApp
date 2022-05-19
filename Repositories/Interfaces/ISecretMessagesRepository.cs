using SecretMessageSharingWebApp.Models.Entities;
using SecretMessageSharingWebApp.Repositories.Interfaces;

namespace SecretMessageSharingWebApp.Repositories
{
	public interface ISecretMessagesRepository : IGeneralRepository<SecretMessage>
	{
		SecretMessage? Retrieve(string id);
		void DeleteOldMessages();
	}
}
