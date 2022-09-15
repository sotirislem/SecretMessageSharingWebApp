using Microsoft.AspNetCore.SignalR;
using SecretMessageSharingWebApp.Hubs;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Services
{
	public class SecretMessageDeliveryNotificationHubService : ISecretMessageDeliveryNotificationHubService
	{
		private readonly Dictionary<string, string> _activeClients = new();

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

		public void AddConnection(string connectionId, string clientId)
		{
			_activeClients.Add(clientId, connectionId);
		}

		public void RemoveConnection(string clientId)
		{
			_activeClients.Remove(clientId);
		}

		public async Task SendAnyPendingSecretMessageDeliveryNotificationFromMemoryCacheQueue(string clientId)
		{
			var memoryResult = _memoryCacheService.GetValue<Queue<SecretMessageDeliveryNotification>>(clientId, Constants.MemoryKey_SecretMessageDeliveryNotificationQueue);
			if (memoryResult.exists)
			{
				var queue = memoryResult.value;
				while (queue.Count > 0)
				{
					var notification = queue.Dequeue();
					await TrySend(clientId, notification, saveToMemoryCacheQueueOnFailure: false);
				}
			}
		}

		public async Task<bool> SendNotification(SecretMessageDeliveryNotification secretMessageDeliveryNotification)
		{
			bool notificationSent = false;

			(var clientIdExists, var clientId) 
				= _memoryCacheService.GetValue<string>(secretMessageDeliveryNotification.MessageId, Constants.MemoryKey_SecretMessageCreatorClientId);
			
			if (clientIdExists)
			{
				notificationSent = await TrySend(clientId, secretMessageDeliveryNotification);
			}

			return notificationSent;
		}

		private async Task<bool> TrySend(string clientId, SecretMessageDeliveryNotification secretMessageDeliveryNotification, bool saveToMemoryCacheQueueOnFailure = true)
		{
			if (_activeClients.ContainsKey(clientId))
			{
				await _secretMessageDeliveryNotificationHub.Clients.Client(_activeClients[clientId]).SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);

				_logger.LogInformation("SecretMessageDeliveryNotificationHubService => Sent delivery notification to client: {clientId}", clientId);
				
				return true;
			}

			if (saveToMemoryCacheQueueOnFailure)
			{
				SaveNotificationToMemoryCacheQueueForFutureDelivery(clientId, secretMessageDeliveryNotification);
			}
			return false;
		}

		private void SaveNotificationToMemoryCacheQueueForFutureDelivery(string clientId, SecretMessageDeliveryNotification secretMessageDeliveryNotification)
		{
			var memoryResult = _memoryCacheService.GetValue<Queue<SecretMessageDeliveryNotification>>(clientId, Constants.MemoryKey_SecretMessageDeliveryNotificationQueue);

			Queue<SecretMessageDeliveryNotification> secretMessageDeliveryNotificationQueue;
			if (memoryResult.exists)
			{
				secretMessageDeliveryNotificationQueue = memoryResult.value;
			}
			else
			{
				secretMessageDeliveryNotificationQueue = new();
			}

			secretMessageDeliveryNotificationQueue.Enqueue(secretMessageDeliveryNotification);
			_memoryCacheService.SetValue(clientId, secretMessageDeliveryNotificationQueue, Constants.MemoryKey_SecretMessageDeliveryNotificationQueue);
		}
	}
}
