using SecretMessageSharingWebApp.Data.Entities;

namespace SecretMessageSharingWebApp.Repositories.Interfaces;

public interface IGetLogsRepository : IGeneralRepository<GetLogEntity>
{
	Task<int> DeleteOldLogs(CancellationToken ct);
}
