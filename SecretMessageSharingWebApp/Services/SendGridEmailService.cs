using SecretMessageSharingWebApp.Configuration;
using SecretMessageSharingWebApp.Services.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SecretMessageSharingWebApp.Services
{
	public class SendGridEmailService : ISendGridEmailService
	{
		private readonly SendGridConfigurationSettings _sendGridConfigurationSettings;

		public SendGridEmailService(SendGridConfigurationSettings sendGridConfigurationSettings)
        {
            _sendGridConfigurationSettings = sendGridConfigurationSettings;
        }

		public async Task<bool> SendOtp(string messageId, string otpCode, string recipientsEmail)
		{
            var from = new EmailAddress(_sendGridConfigurationSettings.AuthSender, Constants.AppName);
            var client = new SendGridClient(_sendGridConfigurationSettings.ApiKey);

            var to = new EmailAddress(recipientsEmail);
            var subject = $"OTP - Message Id: {messageId}";
            
            var htmlContent = $"OTP: <strong>{otpCode}</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

            var response = await client.SendEmailAsync(msg);
            return response.IsSuccessStatusCode;
        }
	}
}
