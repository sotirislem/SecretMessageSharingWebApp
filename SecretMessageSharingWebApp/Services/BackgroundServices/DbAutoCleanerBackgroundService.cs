using SecretMessageSharingWebApp.Repositories.Interfaces;

namespace SecretMessageSharingWebApp.Services.BackgroundServices;

public sealed class DbAutoCleanerBackgroundService(
	IServiceProvider services,
	ILogger<DbAutoCleanerBackgroundService> logger) : BackgroundService
{
	private readonly PeriodicTimer _timer = new(TimeSpan.FromMinutes(Constants.DbAutoCleanerBackgroundServiceRunIntervalInMinutes));

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("DbAutoCleanerBackgroundService: Service running...");

		do
		{
			await DoWorkAsync();
		}
		while (await _timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
	}

	public override async Task StopAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("DbAutoCleanerBackgroundService: Service is stopping...");

		_timer.Dispose();

		await base.StopAsync(stoppingToken);
	}

	private async Task DoWorkAsync()
	{
		logger.LogInformation("DbAutoCleanerBackgroundService: Service triggered DoWorkAsync()");

		using (var scope = services.CreateScope())
		{
			IGetLogsRepository getLogsRepository = scope.ServiceProvider.GetRequiredService<IGetLogsRepository>();
			ISecretMessagesRepository secretMessagesRepository = scope.ServiceProvider.GetRequiredService<ISecretMessagesRepository>();

			await DeleteOldSecretMessages(secretMessagesRepository);
			await DeleteOldGetLogs(getLogsRepository);
		}
	}

	private async Task DeleteOldSecretMessages(ISecretMessagesRepository secretMessagesRepository)
	{
		var deletedMessages = await secretMessagesRepository.DeleteOldMessages();

		logger.LogInformation("DbAutoCleanerBackgroundService: Deleted {deletedMessages} old message(s)", deletedMessages);
	}

	private async Task DeleteOldGetLogs(IGetLogsRepository getLogsRepository)
	{
		var deletedLogs = await getLogsRepository.DeleteOldLogs();

		logger.LogInformation("DbAutoCleanerBackgroundService: Deleted {deletedLogs} old log(s)", deletedLogs);
	}
}
