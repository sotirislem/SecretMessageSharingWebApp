using FastEndpoints;
using Microsoft.IdentityModel.Tokens;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SecretMessageSharingWebApp.Endpoints
{
	public class ValidateSecretMessageOtp : Endpoint<ValidateSecretMessageOtpRequest, ValidateSecretMessageOtpResponse>
	{
		public override void Configure()
		{
			Verbs(Http.POST);
			Routes("/api/secret-messages/validate-otp/{id}");
			AllowAnonymous();
		}

		private readonly ISecretMessagesService _secretMessagesService;
		private readonly IOtpService _otpService;
		private readonly IJwtService _jwtService;
		private readonly IMemoryCacheService _memoryCacheService;

		public ValidateSecretMessageOtp(
			ISecretMessagesService secretMessagesService,
			IOtpService otpService,
			IJwtService jwtService,
			IMemoryCacheService memoryCacheService)
		{
			_secretMessagesService = secretMessagesService;
			_otpService = otpService;
			_jwtService = jwtService;
			_memoryCacheService = memoryCacheService;
		}

		public override async Task HandleAsync(ValidateSecretMessageOtpRequest req, CancellationToken ct)
        {
			var messageId = Route<string>("id")!;

			var result = _secretMessagesService.VerifyExistence(messageId);
			var otpRequired = (result.otp?.Required ?? false);

			var inMemoryOtp = _memoryCacheService.GetValue<OneTimePassword>(messageId, Constants.MemoryKey_SecretMessageOtp, false);

			if (!(result.exists && otpRequired && inMemoryOtp.exists))
			{
				ThrowError("Bad request");
			}

			var otpValidationResult = _otpService.Validate(req.OtpCode, inMemoryOtp.value);

			string? token = null;
			if (otpValidationResult.isValid)
			{
				_memoryCacheService.RemoveValue(messageId, Constants.MemoryKey_SecretMessageOtp);

				token = _jwtService.GenerateToken(
					new List<Claim> {
						new Claim("messageId", messageId)
					}
				);
			}

			var response = new ValidateSecretMessageOtpResponse()
			{
				IsValid = otpValidationResult.isValid,
				CanRetry = otpValidationResult.canRetry,
				HasExpired = otpValidationResult.hasExpired,
				Token = token
			};

			await SendOkAsync(response, cancellation: ct);
		}
	}
}
