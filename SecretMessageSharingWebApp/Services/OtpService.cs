using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SecretMessageSharingWebApp.Services
{
	public class OtpService : IOtpService
	{
		public OtpService()
		{ }

		public OneTimePassword Generate()
		{
			var otpBuilder = new StringBuilder();

			for (var i = 0; i < Constants.OtpSize; i++)
			{
				var randomNumber = RandomNumberGenerator.GetInt32(0, 10);
				otpBuilder.Append(randomNumber);
			}

			var otp = otpBuilder.ToString();
			var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

			return new OneTimePassword(otp, timestamp)
			{
				AvailableValidationAttempts = Constants.OtpMaxValidationRetries
			};
		}

		public (bool isValid, bool canRetry, bool hasExpired) Validate(string otpInputCode, OneTimePassword inMemoryOtp)
		{
			inMemoryOtp.AvailableValidationAttempts--;
			if (inMemoryOtp.AvailableValidationAttempts <= 0)
			{
				return (isValid: false, canRetry: false, hasExpired: true);
			}

			var nowTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			var validTimestampRange = TimeSpan.FromMinutes(Constants.OtpExpirationMinutes).TotalSeconds;
			var otpTimestampValid = (nowTimestamp - inMemoryOtp.CreatedTimestamp <= validTimestampRange);

			if (!otpTimestampValid)
			{
				return (isValid: false, canRetry: true, hasExpired: true);
			}

			var otpCodeValid= (otpInputCode == inMemoryOtp.Code);
			return (isValid: otpCodeValid, canRetry: true, hasExpired: false);
		}
	}
}
