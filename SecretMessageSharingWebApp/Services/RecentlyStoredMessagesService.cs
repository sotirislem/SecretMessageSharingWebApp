using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Services;

public sealed class RecentlyStoredMessagesService(
	IHttpContextAccessor httpContextAccessor,
	ISecretMessagesService secretMessagesService,
	IGetLogsService getLogsService) : IRecentlyStoredMessagesService
{
	public async Task<List<RecentlyStoredSecretMessage>> GetAll()
	{
		var recentStoredSecretMessagesList = GetRecentlyStoredSecretMessagesList();
		if (recentStoredSecretMessagesList.Count == 0)
		{
			return [];
		}

		var resultsPartA = await secretMessagesService.GetRecentlyStoredSecretMessagesInfo(recentStoredSecretMessagesList);
		var resultsPartB = await getLogsService.GetRecentlyStoredSecretMessagesInfo(recentStoredSecretMessagesList);

		var recentlyStoredSecretMessages = resultsPartA.Union(resultsPartB)
			.OrderByDescending(x => x.CreatedDateTime)
			.ToList();

		return recentlyStoredSecretMessages;
	}

	public List<string> GetRecentlyStoredSecretMessagesList()
	{
		var session = httpContextAccessor.HttpContext!.Session;

		return session.GetObject<List<string>>(Constants.SessionKeys.RecentlyStoredSecretMessagesList) ?? [];
	}
}
