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

		return new OneTimePassword()
		{
			Code = otpBuilder.ToString(),
			CreatedTimestamp = dateTimeProviderService.UtcNowUnixTimeSeconds()
		};
	}

	public (bool isValid, bool hasExpired) Validate(string otpInputCode, OneTimePassword otp)
	{
		if (otp.TotalCodeAccesses >= Constants.OtpMaxValidationRetries)
		{
			return (isValid: false, hasExpired: true);
		}

		var nowTimestamp = dateTimeProviderService.UtcNowUnixTimeSeconds();
		var validTimestampRange = TimeSpan.FromMinutes(Constants.OtpExpirationMinutes).TotalSeconds;

		var otpTimestampValid = (nowTimestamp - otp.CreatedTimestamp <= validTimestampRange);
		if (!otpTimestampValid)
		{
			return (isValid: false, hasExpired: true);
		}

		return (isValid: otpInputCode == otp.Code, hasExpired: false);
	}
}
