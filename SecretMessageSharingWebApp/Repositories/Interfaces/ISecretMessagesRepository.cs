using SecretMessageSharingWebApp.Data.Entities;

namespace SecretMessageSharingWebApp.Repositories.Interfaces
{
	public interface ISecretMessagesRepository : IGeneralRepository<SecretMessageDto>
	{
		int DeleteOldMessages();
	}
}
