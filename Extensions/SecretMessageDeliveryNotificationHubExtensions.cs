using Microsoft.AspNetCore.SignalR;
using SecretMessageSharingWebApp.Models.Api.Responses;

namespace SecretMessageSharingWebApp.Hubs
{
	public static class SecretMessageDeliveryNotificationHubExtensions
	{
		public static async Task<bool> TrySendSecretMessageDeliveryNotification(
			this IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> hubContext,
			ILogger logger,
			string connectionId,
			SecretMessageDeliveryNotification secretMessageDeliveryNotification)
		{
			if (SecretMessageDeliveryNotificationHub.ActiveConnections.Contains(connectionId))
			{
				await hubContext.Clients.Client(connectionId).SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);

				logger.LogInformation("SecretMessageDeliveryNotificationHub => Sent delivery notification to client: {connectionId}", connectionId);
				return true;
			}

			return false;
		}
	}
}
