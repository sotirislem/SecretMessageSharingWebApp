using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Services.Interfaces
{
	public interface IGetLogsService
	{
		Task<GetLog> CreateNewLog(GetLog getLog);

		IEnumerable<RecentlyStoredSecretMessage> GetRecentlyStoredSecretMessagesInfo(List<string> recentlyStoredSecretMessagesList);
	}
}
