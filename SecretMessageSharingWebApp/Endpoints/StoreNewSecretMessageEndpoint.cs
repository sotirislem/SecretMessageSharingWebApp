using FastEndpoints;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints;

public sealed class StoreNewSecretMessageEndpoint : Endpoint<StoreNewSecretMessageRequest, string>
{
	public override void Configure()
	{
		Verbs(Http.POST);
		Routes(Constants.ApiRoutes.StoreNewSecretMessage);
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
		var clientId = HttpContext.GetRequestHeaderValue("Client-Id");
		if (string.IsNullOrEmpty(clientId))
		{
			ThrowError("Request.Headers: 'Client-Id' is missing");
		}

		var secretMessage = req.ToSecretMessage();
		secretMessage.CreatedDateTime = HttpContext.GetRequestDateTime();
		secretMessage.CreatorIP = HttpContext.GetClientIP();
		secretMessage.CreatorClientInfo = HttpContext.GetClientInfo();

		secretMessage = await _secretMessagesService.Store(secretMessage);

		SaveSecretMessageToRecentlyStoredSecretMessagesList(secretMessage.Id);
		SaveSecretMessageCreatorClientIdToMemoryCache(secretMessage.Id, clientId);

		await SendCreatedAtAsync<GetSecretMessageEndpoint>(new { id = secretMessage.Id }, secretMessage.Id, cancellation: ct);
	}

	private void SaveSecretMessageCreatorClientIdToMemoryCache(string secretMessageId, string clientId)
	{
		_memoryCacheService.SetValue(secretMessageId, clientId, Constants.MemoryKey_SecretMessageCreatorClientId);
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
