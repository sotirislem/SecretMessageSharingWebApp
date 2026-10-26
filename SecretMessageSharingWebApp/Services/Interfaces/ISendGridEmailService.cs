using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface ISendGridEmailService
{
	Task<bool> SendOtp(
		OneTimePassword otp,
		string messageId,
		string recipientsEmail,
		string? recipientTimeZone,
		int? recipientUtcOffsetMinutes);

	Task<bool> SendDeliveryNotification(SecretMessageDeliveryNotification notification, string recipientsEmail);
}
