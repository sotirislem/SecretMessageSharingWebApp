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

		Description(builder => builder
			.ClearDefaultProduces()
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status404NotFound)
			.Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
			.Produces(StatusCodes.Status500InternalServerError, typeof(InternalErrorResponse)));

		Summary(s =>
		{
			s.Summary = "Delete a recently stored Secret Message";
		});
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var id = Route<string>("id")!;

		var apiResult = await secretMessagesManager.DeleteRecentMessage(id);

		await SendResultAsync(apiResult.HttpResult);
	}
}
