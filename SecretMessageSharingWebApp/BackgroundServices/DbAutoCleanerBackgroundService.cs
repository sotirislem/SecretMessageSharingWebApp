using SecretMessageSharingWebApp.Repositories.Interfaces;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.BackgroundServices;

public sealed class DbAutoCleanerBackgroundService(
    IServiceProvider services,
    IDateTimeProviderService dateTimeProviderService,
    ILogger<DbAutoCleanerBackgroundService> logger) : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromMinutes(Constants.DbAutoCleanerBackgroundServiceRunIntervalInMinutes));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DbAutoCleanerBackgroundService: Service is running...");

        try
        {
            do
            {
                try
                {
                    await DoWorkAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during database cleanup. The service will continue running.");
                }
            }
            while (await _timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("DbAutoCleanerBackgroundService: Operation was canceled, stopping the timer.");
        }
        finally
        {
            _timer.Dispose();
            logger.LogInformation("DbAutoCleanerBackgroundService: Timer disposed.");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DbAutoCleanerBackgroundService: Service has stopped.");

        await base.StopAsync(stoppingToken);
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DbAutoCleanerBackgroundService: Service triggered `DoWorkAsync()` at {localNow}", dateTimeProviderService.LocalNow());

        using (var scope = services.CreateScope())
        {
            IGetLogsRepository getLogsRepository = scope.ServiceProvider.GetRequiredService<IGetLogsRepository>();
            ISecretMessagesRepository secretMessagesRepository = scope.ServiceProvider.GetRequiredService<ISecretMessagesRepository>();

            await DeleteOldSecretMessages(secretMessagesRepository, stoppingToken);
            await DeleteOldGetLogs(getLogsRepository, stoppingToken);
        }
    }

    private async Task DeleteOldSecretMessages(ISecretMessagesRepository secretMessagesRepository, CancellationToken stoppingToken)
    {
        var deletedMessages = await secretMessagesRepository.DeleteOldMessages(stoppingToken);

        logger.LogInformation("DbAutoCleanerBackgroundService: Deleted {deletedMessages} old message(s)", deletedMessages);
    }

    private async Task DeleteOldGetLogs(IGetLogsRepository getLogsRepository, CancellationToken stoppingToken)
    {
        var deletedLogs = await getLogsRepository.DeleteOldLogs(stoppingToken);

        logger.LogInformation("DbAutoCleanerBackgroundService: Deleted {deletedLogs} old log(s)", deletedLogs);
    }
}
