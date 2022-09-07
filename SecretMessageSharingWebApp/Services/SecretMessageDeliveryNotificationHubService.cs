using Microsoft.AspNetCore.SignalR;
using SecretMessageSharingWebApp.Hubs;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Services
{
	public class SecretMessageDeliveryNotificationHubService : ISecretMessageDeliveryNotificationHubService
	{
		private readonly List<string> _activeConnections = new();

		private readonly ILogger<SecretMessageDeliveryNotificationHubService> _logger;
		private readonly IMemoryCacheService _memoryCacheService;
		private readonly IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> _secretMessageDeliveryNotificationHub;

		public SecretMessageDeliveryNotificationHubService(
			ILogger<SecretMessageDeliveryNotificationHubService> logger,
			IMemoryCacheService memoryCacheService,
			IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> secretMessageDeliveryNotificationHub)
		{
			_logger = logger;
			_memoryCacheService = memoryCacheService;
			_secretMessageDeliveryNotificationHub = secretMessageDeliveryNotificationHub;
		}

		public void AddConnection(string connectionId)
		{
			_activeConnections.Add(connectionId);
		}

		public void RemoveConnection(string connectionId)
		{
			_activeConnections.Remove(connectionId);
		}

		public async Task<bool> SendNotification(SecretMessageDeliveryNotification secretMessageDeliveryNotification)
		{
			bool notificationSent = false;

			(var signalRConnectionIdExists, var signalRConnectionId) 
				= _memoryCacheService.GetValue<string>(secretMessageDeliveryNotification.MessageId, Constants.MemoryKey_SecretMessageSignalRConnectionId);
			
			if (signalRConnectionIdExists)
			{
				notificationSent = await TrySend(signalRConnectionId, secretMessageDeliveryNotification);
			}

			return notificationSent;
		}

		private async Task<bool> TrySend(string connectionId, SecretMessageDeliveryNotification secretMessageDeliveryNotification)
		{
			if (_activeConnections.Contains(connectionId))
			{
				await _secretMessageDeliveryNotificationHub.Clients.Client(connectionId).SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);

				_logger.LogInformation("SecretMessageDeliveryNotificationHubService => Sent delivery notification to client: {connectionId}", connectionId);
				
				return true;
			}

			return false;
		}
	}
}
