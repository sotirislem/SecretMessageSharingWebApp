using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Models.DbContext;

namespace SecretMessageSharingWebApp.Repositories
{
	public class GetLogsRepository : IGetLogsRepository
	{
		private readonly GetLogsDbContext _context;
		private readonly ILogger<GetLogsRepository> _logger;

		public GetLogsRepository(GetLogsDbContext context, ILogger<GetLogsRepository> logger)
		{
			this._context = context;
			this._logger = logger;
		}

		public GetLog Add(GetLog getLog)
		{
			_context.GetLogs.Add(getLog);
			_context.SaveChanges();

			_logger.LogInformation("GetLogs => Added new Log.");
			return getLog;
		}
	}
}
