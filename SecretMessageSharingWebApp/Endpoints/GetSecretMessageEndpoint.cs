using FastEndpoints;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Models;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints;

public sealed class GetSecretMessageEndpoint(ISecretMessagesManager secretMessagesManager) : EndpointWithoutRequest<GetSecretMessageResponse?>
{
	public override void Configure()
	{
		Verbs(Http.GET);
		Routes("api/secret-messages/{id}");
		AllowAnonymous();
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var messageId = Route<string>("id")!;
		var encryptionKeySha256 = Query<string>("keyHash")!;

		var jwtToken = HttpContext.ExtractJwtTokenFromRequestHeaders();
		var httpContextClientInfo = HttpContext.GetHttpContextClientInfo();

		var apiResult = await secretMessagesManager.GetMessage(messageId, encryptionKeySha256, jwtToken, httpContextClientInfo);

		if (apiResult is SuccessResult<GetSecretMessageResponse> successResult)
		{
			await SendOkAsync(successResult.Data, cancellation: ct);
			return;
		}

		await SendAsync(response: default, statusCode: apiResult.HttpStatusCode, cancellation: ct);
	}
}
