using FastEndpoints;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints;

public sealed class StoreNewSecretMessageEndpoint(ISecretMessagesService secretMessagesService) : Endpoint<StoreNewSecretMessageRequest, string>
{
	public override void Configure()
	{
		Verbs(Http.POST);
		Routes("api/secret-messages");
		AllowAnonymous();
	}

	public override async Task HandleAsync(StoreNewSecretMessageRequest req, CancellationToken ct)
	{
		var clientId = HttpContext.GetRequestHeaderValue("Client-Id");

		if (string.IsNullOrEmpty(clientId))
		{
			ThrowError("Request.Headers: 'Client-Id' is missing");
		}

		var httpContextClientInfo = HttpContext.GetHttpContextClientInfo();

		var secretMessage = await secretMessagesService.Store(
			req.ToSecretMessage(httpContextClientInfo),
			clientId);

		await SendCreatedAtAsync<GetSecretMessageEndpoint>(
			routeValues: new
			{
				id = secretMessage.Id
			},
			responseBody: secretMessage.Id,
			cancellation: ct);
	}
}
