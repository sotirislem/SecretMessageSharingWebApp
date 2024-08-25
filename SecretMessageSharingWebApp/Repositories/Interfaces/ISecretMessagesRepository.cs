using SecretMessageSharingWebApp.Data.Entities;

namespace SecretMessageSharingWebApp.Repositories.Interfaces;

public interface ISecretMessagesRepository : IGeneralRepository<SecretMessageEntity>
{
	Task<int> DeleteOldMessages(CancellationToken ct);
}
