using FastEndpoints;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints;

public sealed class GetRecentlyStoredSecretMessagesEndpoint(
	IRecentlyStoredMessagesService recentlyStoredMessagesService) : EndpointWithoutRequest<RecentlyStoredSecretMessagesResponse>
{
	public override void Configure()
	{
		Verbs(Http.GET);
		Routes("api/secret-messages");
		AllowAnonymous();
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var recentlyStoredSecretMessages = await recentlyStoredMessagesService.GetAll();

		var response = new RecentlyStoredSecretMessagesResponse()
		{
			RecentlyStoredSecretMessages = recentlyStoredSecretMessages
		};

		await SendAsync(response, cancellation: ct);
	}
}
