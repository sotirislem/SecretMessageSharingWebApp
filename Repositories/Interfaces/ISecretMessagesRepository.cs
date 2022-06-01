using SecretMessageSharingWebApp.Data.Dto;

namespace SecretMessageSharingWebApp.Repositories.Interfaces
{
	public interface ISecretMessagesRepository : IGeneralRepository<SecretMessageDto>
	{
		Task <SecretMessageDto?> Retrieve(string id);

		int DeleteOldMessages();
	}
}
