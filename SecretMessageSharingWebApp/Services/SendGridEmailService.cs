using SecretMessageSharingWebApp.Services.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SecretMessageSharingWebApp.Services
{
	public class SendGridEmailService : ISendGridEmailService
	{
		private readonly IConfiguration _configuration;

		public SendGridEmailService(IConfiguration configuration)
        {
	        _configuration = configuration;
        }

		public async Task<bool> SendOtp(string messageId, string otpCode, string recipientsEmail)
		{
            var apiKey = _configuration["SendGrid:ApiKey"];
            var authSender = _configuration["SendGrid:AuthSender"];

            var from = new EmailAddress(authSender, "SecretMessageSharingWebApp");
            var client = new SendGridClient(apiKey);

            var to = new EmailAddress(recipientsEmail);
            var subject = $"OTP - Message Id: {messageId}";
            
            var htmlContent = $"OTP: <strong>{otpCode}</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

            var response = await client.SendEmailAsync(msg);
            return response.IsSuccessStatusCode;
        }
	}
}
