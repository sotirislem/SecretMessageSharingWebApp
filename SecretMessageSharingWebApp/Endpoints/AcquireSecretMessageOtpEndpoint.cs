using FastEndpoints;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints;

public sealed class AcquireSecretMessageOtpEndpoint(ISecretMessagesManager secretMessagesManager) : EndpointWithoutRequest
{
	public override void Configure()
	{
		Verbs(Http.GET);
		Routes("api/secret-messages/otp/{id}");
		AllowAnonymous();
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var messageId = Route<string>("id")!;

		var apiResult = await secretMessagesManager.SendOtp(messageId);

		await SendAsync(response: default, statusCode: apiResult.HttpStatusCode, cancellation: ct);
	}
}
