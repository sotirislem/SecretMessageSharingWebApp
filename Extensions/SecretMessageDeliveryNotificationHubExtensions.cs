using Microsoft.AspNetCore.SignalR;
using SecretsManagerWebApp.Models.Api;

namespace SecretsManagerWebApp.Hubs
{
    public static class SecretMessageDeliveryNotificationHubExtensions
    {
        public static async void TrySendNotification(this IHubContext<SecretMessageDeliveryNotificationHub> hubContext, string connectionId, MessageDeliveryNotification messageDeliveryNotification)
        {
            if (SecretMessageDeliveryNotificationHub.ActiveConnections.Contains(connectionId))
            {
                await hubContext.Clients.Client(connectionId)
                        .SendAsync(SecretMessageDeliveryNotificationHub.HubMethodName, messageDeliveryNotification);

                LogSentEvent(connectionId);
            }
        }

        private static void LogSentEvent(string connectionId)
		{
            var logger = LoggerFactory
                .Create(logging => logging.AddConsole())
                .CreateLogger<SecretMessageDeliveryNotificationHub>();

            logger.LogInformation("SecretMessageDeliveryNotificationHub => Sent delivery notification to client: {connectionId}", connectionId);
        }
    }
}
