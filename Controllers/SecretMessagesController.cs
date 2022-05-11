using CSharpVitamins;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SecretsManagerWebApp.Helpers;
using SecretsManagerWebApp.Hubs;
using SecretsManagerWebApp.Models.Api;
using SecretsManagerWebApp.Repositories;
using SecretsManagerWebApp.Services;
using System.Diagnostics;

namespace SecretsManagerWebApp.Controllers
{
	[ApiController]
	[Route("api/secret-messages")]
	public class SecretMessagesController : ControllerBase
	{
		private readonly ISecretMessagesRepository _secretMessagesRepository;
		private readonly IGetLogsRepository _getLogsRepository;
		private readonly MemoryCacheService _memoryCacheService;
		private readonly IHubContext<SecretMessageDeliveryNotificationHub> _secretMessageReadNotificationHub;

		public SecretMessagesController(
			ISecretMessagesRepository secretMessagesRepository,
			IGetLogsRepository getLogsRepository,
			MemoryCacheService memoryCacheService,
			IHubContext<SecretMessageDeliveryNotificationHub> secretMessageReadNotificationHub)
		{
			this._secretMessagesRepository = secretMessagesRepository;
			this._getLogsRepository = getLogsRepository;
			this._memoryCacheService = memoryCacheService;
			this._secretMessageReadNotificationHub = secretMessageReadNotificationHub;
		}

		[HttpPost("store")]
		public string Store(SecretMessage secretMessageData)
		{
			var secretMessage = new Models.DbContext.SecretMessage
			{
				JsonData = JsonConvert.SerializeObject(secretMessageData),
				CreatorIP = HttpContext.GetClientIP(),
				CreatorClientInfo = HttpContext.GetClientInfo()
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
				RequestDateTime = HttpContext.GetRequestDateTime(),
				RequestCreatorIP = HttpContext.GetClientIP(),
				RequestClientInfo = HttpContext.GetClientInfo(),
				SecretMessageId = id,
				SecretMessageExisted = (secretMessage is not null),
				SecretMessageCreatedDateTime = secretMessage?.CreatedDateTime,
				SecretMessageCreatorIP = secretMessage?.CreatorIP,
				SecretMessageCreatorClientInfo = secretMessage?.CreatorClientInfo
			};
			_getLogsRepository.Add(getLog);

			if (secretMessage is not null)
			{
				TrySendSecretMessageDeliveryNotification(secretMessage.Id, getLog);
				return JsonConvert.DeserializeObject<SecretMessage>(secretMessage.JsonData);
			}
			return null;
		}

		private void SaveSecretMessageCreatorSignalRConnectionId(string secretMessageId)
		{
			var signalRConnectionId = Request.GetRequestHeaderValue("SignalR-ConnectionId");
			if (!string.IsNullOrEmpty(signalRConnectionId))
			{
				_memoryCacheService.SetValue(secretMessageId, signalRConnectionId);
			}
		}

		private void TrySendSecretMessageDeliveryNotification(string secretMessageId, Models.DbContext.GetLog getLog)
		{
			(var signalRConnectionIdExists, var signalRConnectionId) = _memoryCacheService.GetValue(secretMessageId);
			if (signalRConnectionIdExists)
			{
				var messageDeliveryNotification = new MessageDeliveryNotification
				{
					MessageCreatedOn = getLog.SecretMessageCreatedDateTime!.Value,
					MessageDeliveredOn = getLog.RequestDateTime,
					RecipientIp = getLog.RequestCreatorIP!,
					RecipientClientInfo = getLog.RequestClientInfo!
				};

				_secretMessageReadNotificationHub.TrySendNotification(signalRConnectionId, messageDeliveryNotification);
			}
		}
	}
}