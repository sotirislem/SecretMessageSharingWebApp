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

		Description(builder => builder
			.ClearDefaultProduces()
			.Produces(StatusCodes.Status200OK, typeof(ValidateSecretMessageOtpResponse))
			.Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
			.Produces(StatusCodes.Status500InternalServerError, typeof(InternalErrorResponse)));

		Summary(s =>
		{
			s.Summary = "Validate OTP of a Secret Message (generates auth jwtToken upon success)";
		});
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
