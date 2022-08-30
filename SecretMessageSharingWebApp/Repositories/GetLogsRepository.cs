using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Repositories.Interfaces;

namespace SecretMessageSharingWebApp.Repositories
{
	public class GetLogsRepository : GeneralRepository<GetLogDto>, IGetLogsRepository
	{
		public GetLogsRepository(SecretMessagesDbContext context) : base(context)
		{ }

		public async Task<int> DeleteOldLogs()
		{
			var comparisonDateTime = DateTime.Now.AddDays(-1);
			var deletedLogs = await DeleteRangeBasedOnPredicate(m => m.RequestDateTime < comparisonDateTime);

			return deletedLogs;
		}
	}
}
