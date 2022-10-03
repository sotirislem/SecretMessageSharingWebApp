using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Repositories.Interfaces;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Repositories;

public sealed class GetLogsRepository : GeneralRepository<GetLogDto>, IGetLogsRepository
{
	public GetLogsRepository(SecretMessagesDbContext context, IDateTimeProviderService dateTimeProviderService) : base(context, dateTimeProviderService)
	{ }

	public async Task<int> DeleteOldLogs()
	{
		var comparisonDateTime = _dateTimeProviderService.LocalNow().AddDays(-1);
		var deletedLogs = await DeleteRangeBasedOnPredicate(m => m.RequestDateTime < comparisonDateTime);

		return deletedLogs;
	}
}
