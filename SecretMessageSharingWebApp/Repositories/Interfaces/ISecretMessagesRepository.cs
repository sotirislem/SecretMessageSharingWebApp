using SecretMessageSharingWebApp.Data.Dto;

namespace SecretMessageSharingWebApp.Repositories.Interfaces
{
	public interface ISecretMessagesRepository : IGeneralRepository<SecretMessageDto>
	{
		int DeleteOldMessages();
	}
}
