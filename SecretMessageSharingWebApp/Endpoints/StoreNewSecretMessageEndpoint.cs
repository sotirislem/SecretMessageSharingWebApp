using FastEndpoints;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints;

public sealed class StoreNewSecretMessageEndpoint(
	ISecretMessagesService secretMessagesService) : Endpoint<StoreNewSecretMessageRequest, string>
{
	public override void Configure()
	{
		Verbs(Http.POST);
		Routes("api/secret-messages");
		AllowAnonymous();

		Description(builder => builder
			.ClearDefaultProduces()
			.Produces(StatusCodes.Status201Created, typeof(string))
			.Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
			.Produces(StatusCodes.Status500InternalServerError, typeof(InternalErrorResponse)));

		Summary(s =>
		{
			s.Summary = "Store a new Secret Message";
		});
	}

	public override async Task HandleAsync(StoreNewSecretMessageRequest req, CancellationToken ct)
	{
		var httpContextClientInfo = HttpContext.GetHttpContextClientInfo();

		var secretMessage = await secretMessagesService.Store(
			req.ToSecretMessage(httpContextClientInfo));

		await SendCreatedAtAsync<GetSecretMessageEndpoint>(
			routeValues: new
			{
				id = secretMessage.Id
			},
			responseBody: secretMessage.Id,
			cancellation: ct);
	}
}
