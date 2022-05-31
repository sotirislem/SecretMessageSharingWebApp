using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Repositories;
using SecretMessageSharingWebApp.Mappings;

namespace SecretMessageSharingWebApp.Services
{
	public class GetLogsService : IGetLogsService
	{
		private readonly ILogger<GetLogsService> _logger;
		private readonly IGetLogsRepository _getLogsRepository;

		public GetLogsService(IGetLogsRepository getLogsRepository, ILogger<GetLogsService> logger)
		{
			_getLogsRepository = getLogsRepository;
			_logger = logger;
		}

		public GetLog CreateNewLog(GetLog getLog)
		{
			var getLogDto = getLog.ToGetLogDto();
			_getLogsRepository.Insert(getLogDto, true);

			_logger.LogInformation("GetLogsService:Insert => ID: {getLogId}.", getLogDto.Id);

			return getLogDto.ToGetLog();
		}

		public IEnumerable<RecentlyStoredSecretMessage> GetRecentlyStoredSecretMessagesInfo(IEnumerable<string> recentlyStoredSecretMessagesList)
		{
			return _getLogsRepository.GetAll()
				.Where(getLogDto => recentlyStoredSecretMessagesList.Contains(getLogDto.SecretMessageId) && getLogDto.SecretMessageExisted)
				.Select(getLogDto => getLogDto.ToRecentlyStoredSecretMessage())
				.AsEnumerable();
		}
	}
}
