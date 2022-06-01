using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Services.Interfaces
{
	public interface IGetLogsService
	{
		GetLog CreateNewLog(GetLog getLog);

		IEnumerable<RecentlyStoredSecretMessage> GetRecentlyStoredSecretMessagesInfo(IEnumerable<string> recentlyStoredSecretMessagesList);
	}
}
