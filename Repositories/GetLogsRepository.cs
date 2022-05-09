using SecretsManagerWebApp.Data;
using SecretsManagerWebApp.Models.DbContext;
using System.Diagnostics;

namespace SecretsManagerWebApp.Repositories
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
			var dbSaveResult = _context.SaveChanges();
			if (dbSaveResult < 1)
			{
				throw new Exception("GetLogsRepository: Add");
			}

			_logger.LogInformation("GetLogs: Added new Log.");
			return getLog;
		}
	}
}
