namespace SecretMessageSharingWebApp;

public static class Constants
{
	public const string AppName = "SecretMessageSharingWebApp";
	public const string LocalTimeZone = "GTB Standard Time";

	public const string SessionKey_RecentlyStoredSecretMessagesList = "RECENTLY_STORED_SECRET_MESSAGES_LIST";
	public const string MemoryKey_SecretMessageCreatorClientId = "SECRET_MESSAGE_CREATOR_CLIENT_ID";
	public const string MemoryKey_SecretMessageOtp = "SECRET_MESSAGE_OTP";
	public const string MemoryKey_SecretMessageDeliveryNotificationQueue = "SECRET_MESSAGE_DELIVERY_NOTIFICATION_QUEUE";

	public const int SessionIdleTimeoutInMinutes = 15;
	public const int MemoryCacheValueExpirationTimeInMinutes = 15;
	public const int DbAutoCleanerBackgroundServiceRunIntervalInMinutes = 10;

	public const int OtpSize = 8;
	public const int OtpExpirationMinutes = 3;
	public const int OtpMaxValidationRetries = 3;

	public const int JwtDefaultExpirationMinutes = 1;

	public static class ApiRoutes
	{
		private const string Base = "/api/secret-messages";

		public const string StoreNewSecretMessage = $"{Base}";						// POST
		public const string GetSecretMessage = $"{Base}/{{id}}";					// GET
		public const string VerifySecretMessage = $"{Base}/verify/{{id}}";			// GET
		public const string AcquireSecretMessageOtp = $"{Base}/otp/{{id}}";			// GET
		public const string ValidateSecretMessageOtp = $"{Base}/otp/{{id}}";		// POST
		public const string GetRecentlyStoredSecretMessages = $"{Base}";			// GET
		public const string DeleteRecentlyStoredSecretMessage = $"{Base}/{{id}}";	// DELETE
	}
}
