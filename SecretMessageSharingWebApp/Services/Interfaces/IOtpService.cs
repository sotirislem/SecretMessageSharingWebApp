using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface IOtpService
{
	public OneTimePassword Generate();

	bool IsExpired(OneTimePassword otp);

	public (bool isValid, bool canRetry, bool hasExpired) Validate(string otpInputCode, OneTimePassword otp);
}
