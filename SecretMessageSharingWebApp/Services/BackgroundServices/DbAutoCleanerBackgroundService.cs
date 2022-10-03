using SecretMessageSharingWebApp.Repositories.Interfaces;

namespace SecretMessageSharingWebApp.Services.BackgroundServices;

public sealed class DbAutoCleanerBackgroundService : BackgroundService
{
	public IServiceProvider Services { get; }

	private readonly ILogger<DbAutoCleanerBackgroundService> _logger;
	private readonly PeriodicTimer _timer;

	public DbAutoCleanerBackgroundService(IServiceProvider services, ILogger<DbAutoCleanerBackgroundService> logger)
	{
		Services = services;
		_logger = logger;

		_timer = new(TimeSpan.FromMinutes(Constants.DbAutoCleanerBackgroundServiceRunIntervalInMinutes));
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("DbAutoCleanerBackgroundService: Service running...");

		do
		{
			await DoWorkAsync();
		}
		while (await _timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
	}

	public override async Task StopAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("DbAutoCleanerBackgroundService: Service is stopping...");

		_timer.Dispose();

		await base.StopAsync(stoppingToken);
	}

	private async Task DoWorkAsync()
	{
		_logger.LogInformation("DbAutoCleanerBackgroundService: Service triggered DoWorkAsync()");

		using (var scope = Services.CreateScope())
		{
			await DeleteOldSecretMessages(scope);
			await DeleteOldGetLogs(scope);
		}
	}

	private async Task DeleteOldSecretMessages(IServiceScope scope)
	{
		var secretMessagesRepository = scope.ServiceProvider.GetRequiredService<ISecretMessagesRepository>();

		var deletedMessages = await secretMessagesRepository.DeleteOldMessages();
		if (deletedMessages > 0)
		{
			_logger.LogInformation("DbAutoCleanerBackgroundService: Deleted {deletedMessages} old message(s)", deletedMessages);
		}
	}

	private async Task DeleteOldGetLogs(IServiceScope scope)
	{
		var getLogsRepository = scope.ServiceProvider.GetRequiredService<IGetLogsRepository>();

		var deletedLogs = await getLogsRepository.DeleteOldLogs();
		if (deletedLogs > 0)
		{
			_logger.LogInformation("DbAutoCleanerBackgroundService: Deleted {deletedLogs} old log(s)", deletedLogs);
		}
	}
}
