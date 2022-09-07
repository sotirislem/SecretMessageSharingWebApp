namespace SecretMessageSharingWebApp.Services.Interfaces
{
	public interface ISendGridEmailService
	{
		Task<bool> SendOtp(string messageId, string otpCode, string recipientsEmail);
	}
}
