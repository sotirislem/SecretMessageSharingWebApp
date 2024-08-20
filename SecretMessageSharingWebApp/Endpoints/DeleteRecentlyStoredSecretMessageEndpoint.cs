using FastEndpoints;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints;

public sealed class DeleteRecentlyStoredSecretMessageEndpoint(
	ISecretMessagesManager secretMessagesManager) : EndpointWithoutRequest
{
	public override void Configure()
	{
		Verbs(Http.DELETE);
		Routes("api/secret-messages/{id}");
		AllowAnonymous();
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var id = Route<string>("id")!;

		var apiResult = await secretMessagesManager.DeleteRecentMessage(id);

		await SendAsync(response: default, statusCode: apiResult.HttpStatusCode, cancellation: ct);
	}
}
