using CSharpVitamins;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Hubs;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services;
using System.Diagnostics;

namespace SecretMessageSharingWebApp.Controllers
{
	[ApiController]
	[Route("api/secret-messages")]
	public class SecretMessagesController : ControllerBase
	{
		private readonly ILogger<SecretMessagesController> _logger;
		private readonly ISecretMessagesService _secretMessagesService;
		private readonly IGetLogsService _getLogsService;
		private readonly IMemoryCacheService _memoryCacheService;
		private readonly IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> _secretMessageDeliveryNotificationHub;

		public SecretMessagesController(
			ILogger<SecretMessagesController> logger,
			ISecretMessagesService secretMessagesService,
			IGetLogsService getLogsService,
			IMemoryCacheService memoryCacheService,
			IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> secretMessageDeliveryNotificationHub)
		{
			_logger = logger;
			_secretMessagesService = secretMessagesService;
			_getLogsService = getLogsService;
			_memoryCacheService = memoryCacheService;
			_secretMessageDeliveryNotificationHub = secretMessageDeliveryNotificationHub;
		}

		#region Endpoints
		[HttpPost("store")]
		public string StoreNewSecretMessage(StoreNewSecretMessageRequest storeNewSecretMessageRequest)
		{
			var secretMessage = storeNewSecretMessageRequest.ToSecretMessage();
			secretMessage.CreatedDateTime = HttpContext.GetRequestDateTime();
			secretMessage.CreatorIP = HttpContext.GetClientIP();
			secretMessage.CreatorClientInfo = HttpContext.GetClientInfo();

			secretMessage = _secretMessagesService.Insert(secretMessage);

			SaveSecretMessageToRecentlyStoredSecretMessagesList(secretMessage.Id);
			SaveSecretMessageCreatorSignalRConnectionIdToMemoryCache(secretMessage.Id);

			return secretMessage.Id;
		}

		[HttpGet("get/{id}")]
		public async Task<GetSecretMessageResponse?> GetSecretMessage(string id)
		{
			var secretMessage = await _secretMessagesService.Retrieve(id);

			var getLog = new GetLog
			{
				SecretMessageId = id,
				RequestDateTime = HttpContext.GetRequestDateTime(),
				RequestCreatorIP = HttpContext.GetClientIP(),
				RequestClientInfo = HttpContext.GetClientInfo(),
				SecretMessageExisted = (secretMessage is not null),
				SecretMessageCreatedDateTime = secretMessage?.CreatedDateTime,
				SecretMessageCreatorIP = secretMessage?.CreatorIP,
				SecretMessageCreatorClientInfo = secretMessage?.CreatorClientInfo
			};
			getLog = _getLogsService.CreateNewLog(getLog);

			if (secretMessage is not null)
			{
				var deliveryNotificationSent = await TrySendSecretMessageDeliveryNotification(getLog.ToSecretMessageDeliveryNotification());

				return new GetSecretMessageResponse
				{
					DeliveryNotificationSent = deliveryNotificationSent,
					Data = secretMessage.Data
				};
			}

			return null;
		}

		[HttpGet("getRecentlyStoredSecretMessages")]
		public RecentlyStoredSecretMessagesResponse GetRecentlyStoredSecretMessages()
		{
			var recentStoredSecretMessagesList = GetRecentlyStoredSecretMessagesList();
			if (!recentStoredSecretMessagesList.Any())
			{
				return new RecentlyStoredSecretMessagesResponse();
			}

			var resultsPartA = _secretMessagesService.GetRecentlyStoredSecretMessagesInfo(recentStoredSecretMessagesList);
			var resultsPartB = _getLogsService.GetRecentlyStoredSecretMessagesInfo(recentStoredSecretMessagesList);

			var recentlyStoredSecretMessages = resultsPartA.Union(resultsPartB)
				.OrderByDescending(x => x.CreatedDateTime)
				.Select(x => x.ToApiRecentlyStoredSecretMessage())
				.ToList();

			return new RecentlyStoredSecretMessagesResponse()
			{
				RecentlyStoredSecretMessages = recentlyStoredSecretMessages
			};
		}

		[HttpDelete("deleteRecentlyStoredSecretMessage/{id}")]
		public async Task<bool> DeleteRecentlyStoredSecretMessage(string id)
		{
			var recentStoredSecretMessagesList = GetRecentlyStoredSecretMessagesList();
			if (!recentStoredSecretMessagesList.Contains(id))
			{
				return false;
			}

			var deleted = await _secretMessagesService.Delete(id);
			return deleted;
		}
		#endregion Endpoints

		#region Private Helpers
		private void SaveSecretMessageCreatorSignalRConnectionIdToMemoryCache(string secretMessageId)
		{
			var signalRConnectionId = Request.GetRequestHeaderValue("SignalR-ConnectionId");
			if (!string.IsNullOrEmpty(signalRConnectionId))
			{
				_memoryCacheService.SetValue(secretMessageId, signalRConnectionId);
			}
		}

		private async Task<bool> TrySendSecretMessageDeliveryNotification(SecretMessageDeliveryNotification secretMessageDeliveryNotification)
		{
			bool notificationSent = false;

			(var signalRConnectionIdExists, var signalRConnectionId) = _memoryCacheService.GetValue(secretMessageDeliveryNotification.MessageId);
			if (signalRConnectionIdExists)
			{
				notificationSent = await _secretMessageDeliveryNotificationHub.TrySendSecretMessageDeliveryNotification(_logger, signalRConnectionId, secretMessageDeliveryNotification);
			}

			return notificationSent;
		}

		private void SaveSecretMessageToRecentlyStoredSecretMessagesList(string secretMessageId)
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

		private List<string> GetRecentlyStoredSecretMessagesList()
		{
			return HttpContext.Session.GetObject<List<string>>(Constants.SessionKey_RecentlyStoredSecretMessagesList) ?? new List<string>();
		}
		#endregion
	}
}