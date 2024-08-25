using SecretMessageSharingWebApp.Models;

namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface ISecretMessagesManager
{
	Task<ApiResult> DeleteRecentMessage(string id);

	Task<ApiResult> SendOtp(string messageId);

	Task<ApiResult> ValidateOtp(string messageId, string otpCode);

	Task<ApiResult> GetMessage(string messageId, string encryptionKeySha256, string? jwtToken, HttpContextClientInfo httpContextClientInfo);
}
