using System.Diagnostics;
using SecretMessageSharingWebApp.Configuration;
using SecretMessageSharingWebApp.Services.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SecretMessageSharingWebApp.Services;

public sealed class SendGridEmailService(SendGridConfigurationSettings sendGridConfigurationSettings) : ISendGridEmailService
{
	public async Task<bool> SendOtp(string messageId, string otpCode, string recipientsEmail)
	{
		if (Debugger.IsAttached)
		{
			Console.WriteLine($"OTP Code: {otpCode}");
			return true;
		}

		var client = new SendGridClient(sendGridConfigurationSettings.ApiKey);
		var from = new EmailAddress(sendGridConfigurationSettings.AuthSender, Constants.AppName);
		var to = new EmailAddress(recipientsEmail);

		var subject = $"OTP - Message Id: {messageId}";
		var htmlContent = $"OTP: <strong>{otpCode}</strong>";

		var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
		var response = await client.SendEmailAsync(msg);

		return response.IsSuccessStatusCode;
	}
}
