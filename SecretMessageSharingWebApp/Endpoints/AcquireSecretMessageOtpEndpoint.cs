using FastEndpoints;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints;

public sealed class AcquireSecretMessageOtpEndpoint(
	ISecretMessagesManager secretMessagesManager) : EndpointWithoutRequest
{
	public override void Configure()
	{
		Verbs(Http.GET);
		Routes("api/secret-messages/otp/{id}");
		AllowAnonymous();
		
		Throttle(
			hitLimit: Constants.RateLimit.EndpointHitLimitPerMinute,
			durationSeconds: TimeSpan.FromMinutes(1).TotalSeconds
		);
		
		Description(builder => builder
			.ClearDefaultProduces()
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
			.Produces(StatusCodes.Status500InternalServerError, typeof(InternalErrorResponse)));

		Summary(s =>
		{
			s.Summary = "Send Secret Message OTP to registered recepient`s Email (set at Store)";
		});
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var messageId = Route<string>("id")!;

		// The recipient's browser calls this endpoint, so the expiry shown in the
		// OTP email can be rendered in their own time zone instead of UTC.
		var recipientTimeZone = HttpContext.GetClientTimeZone();
		var recipientUtcOffsetMinutes = HttpContext.GetClientUtcOffsetMinutes();

		var apiResult = await secretMessagesManager.SendOtp(messageId, recipientTimeZone, recipientUtcOffsetMinutes);

		await Send.ResultAsync(apiResult.HttpResult);
	}
}
