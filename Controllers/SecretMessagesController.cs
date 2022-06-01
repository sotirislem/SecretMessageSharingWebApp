using CSharpVitamins;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services.Interfaces;
using System.Diagnostics;

namespace SecretMessageSharingWebApp.Controllers
{
	[ApiController]
	[Route("api/secret-messages")]
	public class SecretMessagesController : ControllerBase
	{
		private readonly ISecretMessagesService _secretMessagesService;
		private readonly IGetLogsService _getLogsService;
		private readonly IMemoryCacheService _memoryCacheService;
		private readonly ISecretMessageDeliveryNotificationHubService _secretMessageDeliveryNotificationHubService;

		public SecretMessagesController(
			ISecretMessagesService secretMessagesService,
			IGetLogsService getLogsService,
			IMemoryCacheService memoryCacheService,
			ISecretMessageDeliveryNotificationHubService secretMessageDeliveryNotificationHubService)
		{
			_secretMessagesService = secretMessagesService;
			_getLogsService = getLogsService;
			_memoryCacheService = memoryCacheService;
			_secretMessageDeliveryNotificationHubService = secretMessageDeliveryNotificationHubService;
		}

		#region Endpoints
		[HttpPost("store")]
		public string StoreNewSecretMessage(StoreNewSecretMessageRequest storeNewSecretMessageRequest)
		{
			var secretMessage = storeNewSecretMessageRequest.ToSecretMessage();
			secretMessage.CreatedDateTime = HttpContext.GetRequestDateTime();
			secretMessage.CreatorIP = HttpContext.GetClientIP();
			secretMessage.CreatorClientInfo = HttpContext.GetClientInfo();

			secretMessage = _secretMessagesService.Store(secretMessage);

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

			if (secretMessage is null)
			{
				return null;
			}

			var deliveryNotificationSent = await _secretMessageDeliveryNotificationHubService.Send(getLog.ToSecretMessageDeliveryNotification());
			return new GetSecretMessageResponse
			{
				DeliveryNotificationSent = deliveryNotificationSent,
				Data = secretMessage.Data
			};
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

		private void SaveSecretMessageToRecentlyStoredSecretMessagesList(string secretMessageId)
		{
			if (HttpContext.Session.GetObject<List<string>>(Constants.SessionKey_RecentlyStoredSecretMessagesList) is List<string> recentlyStoredSecretMessagesList)
			{
				recentlyStoredSecretMessagesList.Add(secretMessageId);
			}
			else
			{
				recentlyStoredSecretMessagesList = new() { secretMessageId };
			}

			HttpContext.Session.SetObject(Constants.SessionKey_RecentlyStoredSecretMessagesList, recentlyStoredSecretMessagesList);
		}

		private List<string> GetRecentlyStoredSecretMessagesList()
		{
			return HttpContext.Session.GetObject<List<string>>(Constants.SessionKey_RecentlyStoredSecretMessagesList) ?? new();
		}
		#endregion
	}
}