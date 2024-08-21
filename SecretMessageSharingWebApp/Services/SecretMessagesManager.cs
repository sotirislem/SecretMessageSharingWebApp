using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Domain;
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
			return ApiResult.BadRequest();
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
			return ApiResult.BadRequest();
		}

		var oneTimePassword = otpService.Generate();

		var otpSent = await sendGridEmailService.SendOtp(messageId, oneTimePassword.Code, otpSettings.RecipientsEmail);
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
			return ApiResult.BadRequest();
		}

		string? jwtToken = null;

		var (isValid, hasExpired) = otpService.Validate(otpCode, cacheResult.value!);

		if (isValid)
		{
			memoryCacheService.RemoveValue(messageId, Constants.MemoryKeys.SecretMessageOtp);

			jwtToken = jwtService.GenerateToken(messageId);
		}

		return ApiResult.Success(new ValidateSecretMessageOtpResponse
		{
			IsValid = isValid,
			HasExpired = hasExpired,
			AuthToken = jwtToken
		});
	}
	public async Task<ApiResult> GetMessage(string messageId, string encryptionKeySha256, string? jwtToken, HttpContextClientInfo httpContextClientInfo)
	{
		var secretMessage = await secretMessagesService.Retrieve(messageId);

		if (secretMessage is null)
		{
			return ApiResult.NotFound();
		}

		if (secretMessage.EncryptionKeySha256 != encryptionKeySha256)
		{
			return ApiResult.BadRequest();
		}

		if (secretMessage.Otp.Required)
		{
			var jwtTokenValid = jwtService.ValidateToken(jwtToken, messageId);

			if (jwtTokenValid is false)
			{
				return ApiResult.Unauthorized();
			}
		}

		var getLog = await getLogsService.CreateNewLog(new GetLog
		{
			SecretMessageId = messageId,
			RequestDateTime = httpContextClientInfo.RequestDateTime,
			RequestCreatorIP = httpContextClientInfo.ClientIP,
			RequestClientInfo = httpContextClientInfo.ClientInfo,
			SecretMessageCreatedDateTime = secretMessage?.CreatedDateTime,
			SecretMessageCreatorIP = secretMessage?.CreatorIP,
			SecretMessageCreatorClientInfo = secretMessage?.CreatorClientInfo
		});

		var secretMessageDeliveryNotification = getLog.ToSecretMessageDeliveryNotification();
		var deliveryNotificationSent = await notificationHubService.SendNotification(secretMessageDeliveryNotification);

		return ApiResult.Success(new GetSecretMessageResponse
		{
			Data = secretMessage!.Data,
			DeliveryNotificationSent = deliveryNotificationSent
		});
	}
}
