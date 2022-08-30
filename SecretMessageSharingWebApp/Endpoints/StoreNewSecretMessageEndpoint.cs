using FastEndpoints;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints
{
	public class StoreNewSecretMessageEndpoint : Endpoint<StoreNewSecretMessageRequest>
	{
		public override void Configure()
		{
			Verbs(Http.POST);
			Routes("/api/secret-messages/store");
			AllowAnonymous();
		}

		private readonly ISecretMessagesService _secretMessagesService;
		private readonly IMemoryCacheService _memoryCacheService;

		public StoreNewSecretMessageEndpoint(ISecretMessagesService secretMessagesService, IMemoryCacheService memoryCacheService)
		{
			_secretMessagesService = secretMessagesService;
			_memoryCacheService = memoryCacheService;
		}

        public override async Task HandleAsync(StoreNewSecretMessageRequest req, CancellationToken ct)
        {
            var secretMessage = req.ToSecretMessage();
            secretMessage.CreatedDateTime = HttpContext.GetRequestDateTime();
            secretMessage.CreatorIP = HttpContext.GetClientIP();
            secretMessage.CreatorClientInfo = HttpContext.GetClientInfo();

            secretMessage = await _secretMessagesService.Store(secretMessage);

            SaveSecretMessageToRecentlyStoredSecretMessagesList(secretMessage.Id);
            SaveSecretMessageCreatorSignalRConnectionIdToMemoryCache(secretMessage.Id);

            await SendStringAsync(secretMessage.Id, cancellation: ct);
        }

		private void SaveSecretMessageCreatorSignalRConnectionIdToMemoryCache(string secretMessageId)
		{
			var signalRConnectionId = HttpContext.Request.GetRequestHeaderValue("SignalR-ConnectionId");
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
	}
}
