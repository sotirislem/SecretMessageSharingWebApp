using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SecretMessageSharingWebApp.Services;

public sealed class OtpService(IDateTimeProviderService dateTimeProviderService) : IOtpService
{
	public OneTimePassword Generate()
	{
		var otpBuilder = new StringBuilder();

		for (var i = 0; i < Constants.OtpSize; i++)
		{
			var randomNumber = RandomNumberGenerator.GetInt32(0, 10);
			otpBuilder.Append(randomNumber);
		}

		var expiresAt = dateTimeProviderService
			.UtcNow()
			.AddMinutes(Constants.OtpExpirationMinutes);

		return new OneTimePassword()
		{
			Code = otpBuilder.ToString(),
			ExpiresAt = expiresAt
		};
	}

	public bool IsExpired(OneTimePassword otp)
	{
		if (otp.CodeValidationAttempts >= Constants.OtpMaxValidationAttempts)
		{
			return true;
		}

		if (dateTimeProviderService.UtcNow() >= otp.ExpiresAt)
		{
			return true;
		}

		return false;
	}

	public (bool isValid, bool canRetry, bool hasExpired) Validate(string otpInputCode, OneTimePassword otp)
	{
		if (IsExpired(otp))
		{
			return (isValid: false, canRetry: false, hasExpired: true);
		}

		var isValid = otp.Validate(otpInputCode);
		var canRetry = IsExpired(otp) is false;

		return (isValid, canRetry, hasExpired: false);
	}
}
