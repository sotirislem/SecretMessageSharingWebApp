using FastEndpoints;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Models;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints;

public sealed class GetSecretMessageEndpoint(
	ISecretMessagesManager secretMessagesManager) : Endpoint<GetSecretMessageRequest, GetSecretMessageResponse?>
{
	public override void Configure()
	{
		Verbs(Http.GET);
		Routes("api/secret-messages/{id}");
		AllowAnonymous();

		Description(builder => builder
			.ClearDefaultProduces()
			.Produces(StatusCodes.Status200OK, typeof(GetSecretMessageResponse))
			.Produces(StatusCodes.Status404NotFound)
			.Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
			.Produces(StatusCodes.Status401Unauthorized)
			.Produces(StatusCodes.Status500InternalServerError, typeof(InternalErrorResponse)));

		Summary(s =>
		{
			s.Summary = "Get stored Secret Message (message will be deleted upon retrieval)";
		});
	}

	public override async Task HandleAsync(GetSecretMessageRequest req, CancellationToken ct)
	{
		var messageId = Route<string>("id")!;

		var jwtToken = HttpContext.ExtractJwtTokenFromRequestHeaders();
		var httpContextClientInfo = HttpContext.GetHttpContextClientInfo();

		var apiResult = await secretMessagesManager.GetMessage(messageId, req.EncryptionKeySha256, jwtToken, httpContextClientInfo);

		await SendResultAsync(apiResult.HttpResult);
	}
}
