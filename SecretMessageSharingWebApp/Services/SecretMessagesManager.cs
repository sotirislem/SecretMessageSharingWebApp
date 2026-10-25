using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Security;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Services;

public class SecretMessagesManager(
	ILogger<SecretMessagesManager> logger,
	ISecretMessagesService secretMessagesService,
	IRecentlyStoredMessagesService recentlyStoredMessagesService,
	IGetLogsService getLogsService,
	IOtpService otpService,
	ISendGridEmailService sendGridEmailService,
	IMemoryCacheService memoryCacheService,
	IJwtService jwtService,
	ISecretMessageDeliveryNotificationHubService notificationHubService) : ISecretMessagesManager
{
	public async Task<ApiResult> DeleteRecentMessage(string id)
	{
		var recentStoredSecretMessagesList = recentlyStoredMessagesService.GetRecentlyStoredSecretMessagesList();
		if (!recentStoredSecretMessagesList.Contains(id))
		{
			return ApiResult.BadRequest("Message not on recently stored list");
		}

		bool deleted = await secretMessagesService.Delete(id);

		return deleted
			? ApiResult.SuccessNoContent()
			: ApiResult.NotFound();
	}

	public async Task<ApiResult> SendOtp(string messageId)
	{
		var (exists, otpSettings) = await secretMessagesService.Exists(messageId);

		if (exists is false || otpSettings?.Required is not true)
		{
			return ApiResult.BadRequest("Message does not exist or does not require OTP");
		}

		var cacheResult = memoryCacheService.GetValue<OneTimePassword>(messageId, Constants.MemoryKeys.SecretMessageOtp, remove: false);

		if (cacheResult.exists && otpService.IsExpired(cacheResult.value!) is false)
		{
			return ApiResult.SuccessNoContent();
		}
		
		var oneTimePassword = otpService.Generate();

		var otpSent = await sendGridEmailService.SendOtp(oneTimePassword, messageId, otpSettings.RecipientsEmail);
		if (!otpSent)
		{
			logger.LogError("Could not sent OTP to recipient's Email: {recipientsEmail}", otpSettings.RecipientsEmail);

			return ApiResult.InternalServerError();
		}

		memoryCacheService.SetValue(messageId, oneTimePassword, Constants.MemoryKeys.SecretMessageOtp);

		return ApiResult.SuccessNoContent();
	}

	public async Task<ApiResult> ValidateOtp(string messageId, string otpCode)
	{
		var (exists, otpSettings) = await secretMessagesService.Exists(messageId);
		var cacheResult = memoryCacheService.GetValue<OneTimePassword>(messageId, Constants.MemoryKeys.SecretMessageOtp, remove: false);

		if (exists is false || otpSettings?.Required is not true || cacheResult.exists is false)
		{
			return ApiResult.BadRequest("No acquired OTP to validate");
		}

		string? jwtToken = null;

		var (isValid, canRetry, hasExpired) = otpService.Validate(otpCode, cacheResult.value!);

		if (isValid || hasExpired)
		{
			// A successful OTP is single-use. An expired/attempt-exhausted OTP is
			// also removed so it cannot be retried indefinitely.
			memoryCacheService.RemoveValue(messageId, Constants.MemoryKeys.SecretMessageOtp);
		}

		if (isValid)
		{
			jwtToken = jwtService.GenerateToken(messageId);
		}

		return ApiResult<ValidateSecretMessageOtpResponse>.SuccessWithData(new()
		{
			IsValid = isValid,
			CanRetry = canRetry,
			HasExpired = hasExpired,
			AuthToken = jwtToken
		});
	}

	public async Task<ApiResult> GetMessage(string messageId, string encryptionKeySha256, string? jwtToken, HttpContextClientInfo httpContextClientInfo)
	{
		var secretMessage = await secretMessagesService.Get(messageId);

		if (secretMessage is null)
		{
			return ApiResult.NotFound();
		}

		// The message must only be deleted after the caller has proven knowledge of the encryption key
		// (and, when required, a valid OTP). Otherwise anybody who knows the message id could destroy it.
		if (!KeyHashVerifier.Matches(secretMessage.EncryptionKeySha256, encryptionKeySha256))
		{
			return ApiResult.BadRequest("Bad encryption key hash");
		}

		if (secretMessage.Otp.Required)
		{
			var jwtTokenValid = jwtService.ValidateToken(jwtToken, messageId);

			if (jwtTokenValid is false)
			{
				return ApiResult.Unauthorized();
			}
		}

		// Only a proven retrieval is logged. Logging earlier would let failed key/OTP
		// attempts create rows (write amplification) and would make the message look
		// retrieved in the creator's history.
		var getLog = await getLogsService.CreateNewLog(new GetLog
		{
			SecretMessageId = messageId,
			RequestDateTime = httpContextClientInfo.RequestDateTime,
			RequestCreatorIP = httpContextClientInfo.ClientIP,
			RequestClientInfo = httpContextClientInfo.ClientInfo,
			SecretMessageCreatedDateTime = secretMessage.CreatedDateTime,
			SecretMessageCreatorIP = secretMessage.CreatorIP,
			SecretMessageCreatorClientInfo = secretMessage.CreatorClientInfo
		});

		var deleted = await secretMessagesService.Delete(messageId);
		if (deleted is false)
		{
			return ApiResult.NotFound();
		}

		// All tabs of the same browser share the client id, so a notification about the
		// creator's own retrieval would otherwise pop up in every tab, including the one that is
		// already showing the message. Target the tab that created the message instead.
		var isSelfRead = string.Equals(secretMessage.CreatorClientId, httpContextClientInfo.ClientId, StringComparison.OrdinalIgnoreCase);
		var targetTabId = isSelfRead ? secretMessage.CreatorTabId : null;

		var secretMessageDeliveryNotification = getLog.ToSecretMessageDeliveryNotification();
		var deliveryNotificationSent = await notificationHubService.SendNotification(secretMessage.CreatorClientId, targetTabId, secretMessageDeliveryNotification);

		return ApiResult<GetSecretMessageResponse>.SuccessWithData(new()
		{
			Data = secretMessage.Data,
			DeliveryNotificationSent = deliveryNotificationSent
		});
	}
}
