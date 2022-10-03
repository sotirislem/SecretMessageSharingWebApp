using FastEndpoints;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints;

public sealed class AcquireSecretMessageOtpEndpoint : EndpointWithoutRequest<bool>
{
	public override void Configure()
	{
		Verbs(Http.GET);
		Routes(Constants.ApiRoutes.AcquireSecretMessageOtp);
		AllowAnonymous();
	}

	private readonly ISecretMessagesService _secretMessagesService;
	private readonly IMemoryCacheService _memoryCacheService;
	private readonly IOtpService _otpService;
	private readonly ISendGridEmailService _sendGridEmailService;

	public AcquireSecretMessageOtpEndpoint(
		ISecretMessagesService secretMessagesService,
		IMemoryCacheService memoryCacheService,
		IOtpService otpService,
		ISendGridEmailService sendGridEmailService)
	{
		_secretMessagesService = secretMessagesService;
		_otpService = otpService;
		_memoryCacheService = memoryCacheService;
		_sendGridEmailService = sendGridEmailService;
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var messageId = Route<string>("id")!;

		var result = _secretMessagesService.VerifyExistence(messageId);
		var otpRequired = (result.otp?.Required ?? false);

		if (!(result.exists && otpRequired))
		{
			ThrowError("Bad request");
		}

		var oneTimePassword = _otpService.Generate();
		_memoryCacheService.SetValue(messageId, oneTimePassword, Constants.MemoryKey_SecretMessageOtp);

#if DEBUG
		Console.WriteLine(oneTimePassword);
#else
		var otpSent = await _sendGridEmailService.SendOtp(messageId, oneTimePassword.Code, result.otp!.RecipientsEmail);
		if (!otpSent)
		{
			_memoryCacheService.RemoveValue(messageId, Constants.MemoryKey_SecretMessageOtp);
			ThrowError("Could not sent OTP to recipient's Email.");
		}
#endif

		await SendOkAsync(true, cancellation: ct);
	}
}
