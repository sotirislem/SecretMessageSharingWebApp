using SecretMessageSharingWebApp.Repositories.Interfaces;

namespace SecretMessageSharingWebApp.Services.BackgroundServices
{
	public class DbAutoCleanerBackgroundService : BackgroundService
	{
		public IServiceProvider Services { get; }

		private readonly ILogger<DbAutoCleanerBackgroundService> _logger;
		private Timer _timer = null!;

		public DbAutoCleanerBackgroundService(IServiceProvider services, ILogger<DbAutoCleanerBackgroundService> logger)
		{
			Services = services;
			_logger = logger;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("DbAutoCleanerBackgroundService: Service running...");

			_timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(Constants.DbAutoCleanerBackgroundServiceRunIntervalInMinutes));

			return Task.CompletedTask;
		}

		public override async Task StopAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("DbAutoCleanerBackgroundService: Service is stopping...");

			_timer?.Change(Timeout.Infinite, 0);

			await base.StopAsync(stoppingToken);
		}

		private async void DoWork(object? state)
		{
			_logger.LogInformation("DbAutoCleanerBackgroundService: Service triggered DoWork()");

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
}
