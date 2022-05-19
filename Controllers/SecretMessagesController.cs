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
		private readonly IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> _secretMessageDeliveryNotificationHub;

		public SecretMessagesController(
			ILogger<SecretMessagesController> logger,
			ISecretMessagesRepository secretMessagesRepository,
			IGetLogsRepository getLogsRepository,
			MemoryCacheService memoryCacheService,
			IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> secretMessageDeliveryNotificationHub)
		{
			_logger = logger;
			_secretMessagesRepository = secretMessagesRepository;
			_getLogsRepository = getLogsRepository;
			_memoryCacheService = memoryCacheService;
			_secretMessageDeliveryNotificationHub = secretMessageDeliveryNotificationHub;
		}

		#region Endpoints
		[HttpPost("store")]
		public string Store(SecretMessageData secretMessageData)
		{
			var secretMessage = new Models.Entities.SecretMessage
			{
				JsonData = JsonConvert.SerializeObject(secretMessageData),
				CreatorIP = HttpContext.GetClientIP(),
				CreatorClientInfo = HttpContext.GetClientInfo()
			};
			_secretMessagesRepository.Insert(secretMessage, true);

			SaveMsgToRecentlyStoredSecretMessagesList(secretMessage.Id);
			SaveSecretMessageCreatorSignalRConnectionId(secretMessage.Id);

			return secretMessage.Id;
		}

		[HttpGet("get/{id}")]
		public async Task<GetSecretMessageResponse?> Get(string id)
		{
			var secretMessage = _secretMessagesRepository.Retrieve(id);
			var getLog = new Models.Entities.GetLog
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
			_getLogsRepository.Insert(getLog, true);

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

		[HttpGet("getRecentMessages")]
		public List<RecentlyStoredMessage> GetRecentlyStoredMessages()
		{
			var recentStoredSecretMessagesList = GetRecentlyStoredSecretMessagesList();
			if (!recentStoredSecretMessagesList.Any())
			{
				return new List<RecentlyStoredMessage>();
			}

			var resultsA = _secretMessagesRepository.GetAll()
				.Where(o => recentStoredSecretMessagesList.Contains(o.Id))
				.Select(o => new RecentlyStoredMessage
				{
					Id = o.Id,
					CreatedDateTime = o.CreatedDateTime
				})
				.AsEnumerable();

			var resultsB = _getLogsRepository.GetAll()
				.Where(o => o.SecretMessageExisted && recentStoredSecretMessagesList.Contains(o.SecretMessageId))
				.Select(o => new RecentlyStoredMessage
				{
					Id = o.SecretMessageId,
					CreatedDateTime = o.SecretMessageCreatedDateTime!.Value,
					DeliveryDetails = new DeliveryDetails { DeliveredAt = o.RequestDateTime, RecipientIP = o.RequestCreatorIP!, RecipientClientInfo = o.RequestClientInfo! }
				})
				.AsEnumerable();
			
			var results = resultsA.Union(resultsB).OrderByDescending(o => o.CreatedDateTime).ToList();

			return results;
		}

		[HttpDelete("deleteRecentMessage/{id}")]
		public bool DeleteRecentlyStoredMessage(string id)
		{
			var recentStoredSecretMessagesList = GetRecentlyStoredSecretMessagesList();
			if (!recentStoredSecretMessagesList.Contains(id))
			{
				return false;
			}
			
			var secretMessage = _secretMessagesRepository.Get(id);
			if (secretMessage is null)
			{
				return false;
			}

			_secretMessagesRepository.Delete(secretMessage!, true);
			return true;
		}
		#endregion Endpoints

		#region Private Helpers
		public void SaveMsgToRecentlyStoredSecretMessagesList(string secretMessageId)
		{
			if (HttpContext.Session.GetObject<List<string>>(Constants.SessionKey_RecentlyStoredSecretMessagesList) is List<string> storedSecretMessagesList)
			{
				storedSecretMessagesList.Add(secretMessageId);
			}
			else
			{
				storedSecretMessagesList = new List<string> { secretMessageId };
			}

			HttpContext.Session.SetObject(Constants.SessionKey_RecentlyStoredSecretMessagesList, storedSecretMessagesList);
		}

		public List<string> GetRecentlyStoredSecretMessagesList()
		{
			return HttpContext.Session.GetObject<List<string>>(Constants.SessionKey_RecentlyStoredSecretMessagesList) ?? new List<string>();
		}

		private void SaveSecretMessageCreatorSignalRConnectionId(string secretMessageId)
		{
			var signalRConnectionId = Request.GetRequestHeaderValue("SignalR-ConnectionId");
			if (!string.IsNullOrEmpty(signalRConnectionId))
			{
				_memoryCacheService.SetValue(secretMessageId, signalRConnectionId);
			}
		}

		private Task<bool> TrySendSecretMessageDeliveryNotification(string secretMessageId, Models.Entities.GetLog getLog)
		{
			(var signalRConnectionIdExists, var signalRConnectionId) = _memoryCacheService.GetValue(secretMessageId);
			if (signalRConnectionIdExists)
			{
				var messageDeliveryNotification = new MessageDeliveryDetails
				{
					MessageId = secretMessageId,
					MessageCreatedOn = getLog.SecretMessageCreatedDateTime!.Value,
					MessageDeliveredOn = getLog.RequestDateTime,
					RecipientIp = getLog.RequestCreatorIP!,
					RecipientClientInfo = getLog.RequestClientInfo!
				};

				return _secretMessageDeliveryNotificationHub.TrySendMessageDeliveryNotification(signalRConnectionId, messageDeliveryNotification, _logger);
			}

			return Task.FromResult(false);
		}
		#endregion
	}
}