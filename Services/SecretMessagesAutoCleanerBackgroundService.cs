using SecretsManagerWebApp.Data;
using Microsoft.EntityFrameworkCore;
using SecretsManagerWebApp.Repositories;

namespace SecretsManagerWebApp.Services
{
    public class SecretMessagesAutoCleanerBackgroundService : BackgroundService
    {
        public IServiceProvider Services { get; }

        private readonly ILogger<SecretMessagesAutoCleanerBackgroundService> _logger;
        private Timer _timer = null!;

        public SecretMessagesAutoCleanerBackgroundService(IServiceProvider services, ILogger<SecretMessagesAutoCleanerBackgroundService> logger)
        {
            Services = services;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SecretMessagesAutoCleanerBackgroundService: Service running...");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            _logger.LogInformation("SecretMessagesAutoCleanerBackgroundService: Service triggered DoWork().");

            using (var scope = Services.CreateScope())
            {
                var secretMessagesRepository = scope.ServiceProvider.GetRequiredService<ISecretMessagesRepository>();
                secretMessagesRepository.DeleteOldMessages();
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SecretMessagesAutoCleanerBackgroundService: Service is stopping...");

            _timer?.Change(Timeout.Infinite, 0);

            await base.StopAsync(stoppingToken);
        }
    }
}
