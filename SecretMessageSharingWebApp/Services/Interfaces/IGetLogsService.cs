using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface IGetLogsService
{
	Task<GetLog> CreateNewLog(GetLog getLog);

	Task<List<RecentlyStoredSecretMessage>> GetRecentlyStoredSecretMessagesInfo(List<string> recentlyStoredSecretMessagesList);
}
