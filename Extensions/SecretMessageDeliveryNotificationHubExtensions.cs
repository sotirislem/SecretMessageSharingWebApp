using Microsoft.AspNetCore.SignalR;
using SecretMessageSharingWebApp.Models.Api;

namespace SecretMessageSharingWebApp.Hubs
{
    public static class SecretMessageDeliveryNotificationHubExtensions
    {
        public static async Task<bool> TrySendNotification(this IHubContext<SecretMessageDeliveryNotificationHub> hubContext, string connectionId, MessageDeliveryNotification messageDeliveryNotification, ILogger logger)
        {
            if (SecretMessageDeliveryNotificationHub.ActiveConnections.Contains(connectionId))
            {
                await hubContext.Clients.Client(connectionId)
                        .SendAsync(SecretMessageDeliveryNotificationHub.HubMethodName, messageDeliveryNotification);

                logger.LogInformation("SecretMessageDeliveryNotificationHub => Sent delivery notification to client: {connectionId}", connectionId);
                return true;
            }

            return false;
        }
    }
}
