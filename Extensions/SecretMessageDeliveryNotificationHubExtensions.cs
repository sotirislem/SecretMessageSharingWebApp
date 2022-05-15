using Microsoft.AspNetCore.SignalR;
using SecretMessageSharingWebApp.Models.Api;

namespace SecretMessageSharingWebApp.Hubs
{
    public static class SecretMessageDeliveryNotificationHubExtensions
    {
        public static async Task<bool> TrySendMessageDeliveryNotification(this IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> hubContext, string connectionId, MessageDeliveryNotification messageDeliveryNotification, ILogger logger)
        {
            if (SecretMessageDeliveryNotificationHub.ActiveConnections.Contains(connectionId))
            {
                await hubContext.Clients.Client(connectionId).SendMessageDeliveryNotification(messageDeliveryNotification);

                logger.LogInformation("SecretMessageDeliveryNotificationHub => Sent delivery notification to client: {connectionId}", connectionId);
                return true;
            }

            return false;
        }
    }
}
