using System.Diagnostics;
using SecretMessageSharingWebApp.Configuration;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SecretMessageSharingWebApp.Services;

public sealed class SendGridEmailService(
	ILogger<SendGridEmailService> logger,
	SendGridConfigurationSettings sendGridConfigurationSettings) : ISendGridEmailService
{
	public async Task<bool> SendOtp(OneTimePassword otp, string messageId, string recipientsEmail)
	{
		if (Debugger.IsAttached)
		{
			Console.WriteLine($"OTP Code: {otp.Code}");
			return true;
		}

		var client = new SendGridClient(sendGridConfigurationSettings.ApiKey);
		var from = new EmailAddress(sendGridConfigurationSettings.AuthSender, Constants.AppName);
		var to = new EmailAddress(recipientsEmail);

		var (subject, htmlContent) = CreateEmailContent(otp, messageId);

		var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent);
		var response = await client.SendEmailAsync(msg);

		if (response.IsSuccessStatusCode is false)
		{
			logger.LogError("Failed to send OTP for message: {messageId} | SendGrid API status code: {httpStatusCode} ({httpStatusCodeInt})",
				messageId, response.StatusCode, (int)response.StatusCode);
		}

		return response.IsSuccessStatusCode;
	}

	private (string subject, string htmlContent) CreateEmailContent(OneTimePassword otp, string messageId)
	{
		var subject = $"{Constants.AppName} - OTP";

		var htmlContent =
			$"Message Id: <strong>{messageId}</strong></br>" +
			$"</br>" +
			$"OTP: <strong>{otp.Code}</strong></br>" +
			$"Expires at: {otp.ExpiresAt}";

		return (subject, htmlContent);
	}
}
