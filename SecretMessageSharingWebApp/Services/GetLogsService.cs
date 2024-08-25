using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Services.Interfaces;
using SecretMessageSharingWebApp.Repositories.Interfaces;
using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Services;

public sealed class GetLogsService : IGetLogsService
{
	private readonly ILogger<GetLogsService> _logger;
	private readonly IGetLogsRepository _getLogsRepository;

	public GetLogsService(IGetLogsRepository getLogsRepository, ILogger<GetLogsService> logger)
	{
		_getLogsRepository = getLogsRepository;
		_logger = logger;
	}

	public async Task<GetLog> CreateNewLog(GetLog getLog)
	{
		var getLogDto = getLog.ToEntity();
		await _getLogsRepository.Insert(getLogDto);

		_logger.LogInformation("GetLogsService:Insert => ID: {getLogId}", getLogDto.Id);

		return getLogDto.ToDomain();
	}

	public async Task<List<RecentlyStoredSecretMessage>> GetRecentlyStoredSecretMessagesInfo(List<string> recentlyStoredSecretMessagesList)
	{
		return (await _getLogsRepository
			.SelectEntitiesWhere(getLogEntity => recentlyStoredSecretMessagesList.Contains(getLogEntity.SecretMessageId)))
			.Select(getLogEntity => getLogEntity.ToRecentlyStoredSecretMessage())
			.ToList();
	}
}
