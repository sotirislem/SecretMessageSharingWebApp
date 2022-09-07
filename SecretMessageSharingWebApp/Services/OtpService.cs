using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SecretMessageSharingWebApp.Services
{
	public class OtpService : IOtpService
	{
		private const int OtpSize = 8;
		private const int OtpExpirationMinutes = 3;
		private const int OtpMaxValidationRetries = 3;
		private const string Digits = "0123456789";

		public OtpService()
		{ }

		public OneTimePassword Generate()
		{
			var otpBuilder = new StringBuilder();

			for (var i = 0; i < OtpSize; i++)
			{
				var index = RandomNumberGenerator.GetInt32(Digits.Length);
				otpBuilder.Append(Digits[index]);
			}

			var otp = otpBuilder.ToString();
			var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

			return new OneTimePassword(otp, timestamp)
			{
				AvailableValidationAttempts = OtpMaxValidationRetries
			};
		}

		public (bool isValid, bool canRetry, bool hasExpired) Validate(string otpInputCode, OneTimePassword inMemoryOtp)
		{
			if (inMemoryOtp.AvailableValidationAttempts <= 0)
			{
				return (false, false, true);
			}

			var nowTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			var validTimestampRange = TimeSpan.FromMinutes(OtpExpirationMinutes).TotalSeconds;

			var otpCodeValid= (otpInputCode == inMemoryOtp.Code);
			var otpTimestampValid = (nowTimestamp - inMemoryOtp.CreatedTimestamp <= validTimestampRange);

			inMemoryOtp.AvailableValidationAttempts--;

			var isValid = (otpCodeValid && otpTimestampValid);
			var canRetry = (inMemoryOtp.AvailableValidationAttempts > 0);
			var hasExpired = !otpTimestampValid;
			return (isValid, canRetry, hasExpired);
		}
	}
}
