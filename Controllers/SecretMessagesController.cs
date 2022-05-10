using CSharpVitamins;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SecretsManagerWebApp.Helpers;
using SecretsManagerWebApp.Hubs;
using SecretsManagerWebApp.Models.Api;
using SecretsManagerWebApp.Repositories;
using System.Diagnostics;

namespace SecretsManagerWebApp.Controllers
{
	[ApiController]
	[Route("api/secret-messages")]
	public class SecretMessagesController : ControllerBase
	{
		private readonly ISecretMessagesRepository _secretMessagesRepository;
		private readonly IGetLogsRepository _getLoggerRepository;
		private readonly IMemoryCache _memoryCache;
		private readonly IHubContext<SecretMessageDeliveryNotificationHub> _secretMessageReadNotificationHub;

		public SecretMessagesController(
			ISecretMessagesRepository secretMessagesRepository,
			IGetLogsRepository getLoggerRepository,
			IMemoryCache memoryCache,
			IHubContext<SecretMessageDeliveryNotificationHub> hub)
		{
			this._secretMessagesRepository = secretMessagesRepository;
			this._getLoggerRepository = getLoggerRepository;
			this._memoryCache = memoryCache;
			this._secretMessageReadNotificationHub = hub;
		}

		[HttpPost("store")]
		public string Store(SecretMessage secretMessageData)
		{
			var secretMessage = new Models.DbContext.SecretMessage
			{
				JsonData = JsonConvert.SerializeObject(secretMessageData),
				CreatorIP = HttpContextHelper.GetClientIP(HttpContext),
				CreatorClientInfo = HttpContextHelper.GetClientInfo(HttpContext)
			};
			_secretMessagesRepository.Store(secretMessage);

			SaveSecretMessageCreatorSignalRConnectionId(secretMessage.Id);

			return secretMessage.Id;
		}

		[HttpGet("get/{id}")]
		public async Task<SecretMessage?> Get(string id)
		{
			var secretMessage = await _secretMessagesRepository.Get(id);
			var getLog = new Models.DbContext.GetLog
			{
				RequestDateTime = HttpContextHelper.GetRequestDateTime(HttpContext),
				RequestCreatorIP = HttpContextHelper.GetClientIP(HttpContext),
				RequestClientInfo = HttpContextHelper.GetClientInfo(HttpContext),
				SecretMessageId = id,
				SecretMessageExisted = (secretMessage is not null),
				SecretMessageCreatedDateTime = secretMessage?.CreatedDateTime,
				SecretMessageCreatorIP = secretMessage?.CreatorIP,
				SecretMessageCreatorClientInfo = secretMessage?.CreatorClientInfo
			};
			_getLoggerRepository.Add(getLog);

			if (secretMessage is not null)
			{
				TryToSendSecretMessageDeliveryNotification(secretMessage.Id, getLog);
				return JsonConvert.DeserializeObject<SecretMessage>(secretMessage.JsonData);
			}
			return null;
		}

		private void SaveSecretMessageCreatorSignalRConnectionId(string secretMessageId)
		{
			var signalRConnectionId = HttpContextHelper.GetRequestHeaderValue(Request, "SignalR-ConnectionId");
			if (!string.IsNullOrEmpty(signalRConnectionId))
			{
				_memoryCache.Set(secretMessageId, signalRConnectionId);
			}
		}

		private async void TryToSendSecretMessageDeliveryNotification(string secretMessageId, Models.DbContext.GetLog getLog)
		{
			var messageCreatorSignalRConnectionIdExists = _memoryCache.TryGetValue(secretMessageId, out string messageCreatorSignalRConnectionId);
			if (messageCreatorSignalRConnectionIdExists)
			{
				_memoryCache.Remove(secretMessageId);

				if (SecretMessageDeliveryNotificationHub.ActiveConnections.Contains(messageCreatorSignalRConnectionId))
				{
					var messageDeliveryNotification = new MessageDeliveryNotification
					{
						MessageCreatedOn = getLog.SecretMessageCreatedDateTime!.Value,
						MessageDeliveredOn = getLog.RequestDateTime,
						RecipientIp = getLog.RequestCreatorIP!,
						RecipientClientInfo = getLog.RequestClientInfo!
					};
					await _secretMessageReadNotificationHub.Clients.Client(messageCreatorSignalRConnectionId).SendAsync("secret-message-delivery-notification", messageDeliveryNotification);
				}
				
			}
		}
	}
}