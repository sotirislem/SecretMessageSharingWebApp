using CSharpVitamins;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Hubs;
using SecretMessageSharingWebApp.Models.Api;
using SecretMessageSharingWebApp.Repositories;
using SecretMessageSharingWebApp.Services;
using System.Diagnostics;

namespace SecretMessageSharingWebApp.Controllers
{
	[ApiController]
	[Route("api/secret-messages")]
	public class SecretMessagesController : ControllerBase
	{
		private readonly ILogger<SecretMessagesController> _logger;
		private readonly ISecretMessagesRepository _secretMessagesRepository;
		private readonly IGetLogsRepository _getLogsRepository;
		private readonly MemoryCacheService _memoryCacheService;
		private readonly IHubContext<SecretMessageDeliveryNotificationHub> _secretMessageReadNotificationHub;

		public SecretMessagesController(
			ISecretMessagesRepository secretMessagesRepository,
			IGetLogsRepository getLogsRepository,
			MemoryCacheService memoryCacheService,
			IHubContext<SecretMessageDeliveryNotificationHub> secretMessageReadNotificationHub,
			ILogger<SecretMessagesController> logger)
		{
			this._logger = logger;
			this._secretMessagesRepository = secretMessagesRepository;
			this._getLogsRepository = getLogsRepository;
			this._memoryCacheService = memoryCacheService;
			this._secretMessageReadNotificationHub = secretMessageReadNotificationHub;
		}

		[HttpPost("store")]
		public string Store(SecretMessageData secretMessageData)
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
		public async Task<GetSecretMessageResponse?> Get(string id)
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
				var deliveryNotificationSent = await TrySendSecretMessageDeliveryNotification(secretMessage.Id, getLog);

				return new GetSecretMessageResponse
				{
					DeliveryNotificationSent = deliveryNotificationSent,
					SecretMessageData = JsonConvert.DeserializeObject<SecretMessageData>(secretMessage.JsonData)!
				};
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

		private Task<bool> TrySendSecretMessageDeliveryNotification(string secretMessageId, Models.DbContext.GetLog getLog)
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

				return _secretMessageReadNotificationHub.TrySendNotification(signalRConnectionId, messageDeliveryNotification, _logger);
			}

			return Task.FromResult(false);
		}
	}
}