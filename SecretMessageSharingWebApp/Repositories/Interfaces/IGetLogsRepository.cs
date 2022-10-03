using SecretMessageSharingWebApp.Data.Entities;

namespace SecretMessageSharingWebApp.Repositories.Interfaces;

public interface IGetLogsRepository : IGeneralRepository<GetLogDto>
{
	Task<int> DeleteOldLogs();
}
