using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface IRecentlyStoredMessagesService
{
	Task<List<RecentlyStoredSecretMessage>> GetAll();

	List<string> GetRecentlyStoredSecretMessagesList();
}
