using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface ISendGridEmailService
{
	Task<bool> SendOtp(OneTimePassword otp, string messageId, string recipientsEmail);
}
