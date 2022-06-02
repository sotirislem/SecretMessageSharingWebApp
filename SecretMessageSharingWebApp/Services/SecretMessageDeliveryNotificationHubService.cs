using Microsoft.AspNetCore.SignalR;
using SecretMessageSharingWebApp.Hubs;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Services
{
	public class SecretMessageDeliveryNotificationHubService : ISecretMessageDeliveryNotificationHubService
	{
		private readonly ILogger<SecretMessageDeliveryNotificationHubService> _logger;
		private readonly IMemoryCacheService _memoryCacheService;
		private readonly IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> _secretMessageDeliveryNotificationHub;

		public SecretMessageDeliveryNotificationHubService(
			ILogger<SecretMessageDeliveryNotificationHubService> logger,
			IMemoryCacheService memoryCacheService,
			IHubContext<SecretMessageDeliveryNotificationHub,ISecretMessageDeliveryNotificationHub> secretMessageDeliveryNotificationHub)
		{
			_logger = logger;
			_memoryCacheService = memoryCacheService;
			_secretMessageDeliveryNotificationHub = secretMessageDeliveryNotificationHub;
		}

		public async Task<bool> Send(SecretMessageDeliveryNotification secretMessageDeliveryNotification)
		{
			bool notificationSent = false;

			(var signalRConnectionIdExists, var signalRConnectionId) = _memoryCacheService.GetValue(secretMessageDeliveryNotification.MessageId);
			if (signalRConnectionIdExists)
			{
				notificationSent = await TrySend(signalRConnectionId, secretMessageDeliveryNotification);
			}

			return notificationSent;
		}

		private async Task<bool> TrySend(string connectionId, SecretMessageDeliveryNotification secretMessageDeliveryNotification)
		{
			if (SecretMessageDeliveryNotificationHub.ActiveConnections.Contains(connectionId))
			{
				await _secretMessageDeliveryNotificationHub.Clients.Client(connectionId).SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);

				_logger.LogInformation("SecretMessageDeliveryNotificationHubService => Sent delivery notification to client: {connectionId}", connectionId);
				return true;
			}

			return false;
		}
	}
}
