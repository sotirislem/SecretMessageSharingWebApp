using FastEndpoints;
using SecretMessageSharingWebApp.Models;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints;

public sealed class ValidateSecretMessageOtpEndpoint(
	ISecretMessagesManager secretMessagesManager) : Endpoint<ValidateSecretMessageOtpRequest, ValidateSecretMessageOtpResponse?>
{
	public override void Configure()
	{
		Verbs(Http.POST);
		Routes("api/secret-messages/otp/{id}");
		AllowAnonymous();
	}

	public override async Task HandleAsync(ValidateSecretMessageOtpRequest req, CancellationToken ct)
	{
		var messageId = Route<string>("id")!;

		var apiResult = await secretMessagesManager.ValidateOtp(messageId, req.OtpCode);

		if (apiResult is SuccessResult<ValidateSecretMessageOtpResponse> successResult)
		{
			await SendOkAsync(successResult.Data, cancellation: ct);
			return;
		}

		await SendAsync(response: default, statusCode: apiResult.HttpStatusCode, cancellation: ct);
	}
}
