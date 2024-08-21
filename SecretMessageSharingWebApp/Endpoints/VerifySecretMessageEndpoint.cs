using FastEndpoints;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints;

public sealed class VerifySecretMessageEndpoint(
	ISecretMessagesService secretMessagesService) : EndpointWithoutRequest<VerifySecretMessageResponse>
{
	public override void Configure()
	{
		Verbs(Http.GET);
		Routes("api/secret-messages/verify/{id}");
		AllowAnonymous();

		Description(builder => builder
			.ClearDefaultProduces()
			.Produces(StatusCodes.Status200OK, typeof(VerifySecretMessageResponse))
			.Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
			.Produces(StatusCodes.Status500InternalServerError, typeof(InternalErrorResponse)));

		Summary(s =>
		{
			s.Summary = "Verify Secret Message existence, check if OTP is required for retrieval";
		});
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var messageId = Route<string>("id")!;

		var (exists, otpSettings) = await secretMessagesService.Exists(messageId);

		var response = new VerifySecretMessageResponse
		{
			Id = messageId,
			Exists = exists,
			RequiresOtp = otpSettings?.Required
		};

		await SendOkAsync(response, cancellation: ct);
	}
}
