namespace SecretMessageSharingWebApp;

public static class Constants
{
	public const string AppName = "SecretMessageSharingWebApp";

	public const int SessionIdleTimeoutInMinutes = 15;
	public const int MemoryCacheValueExpirationTimeInMinutes = 15;
	public const int DbAutoCleanerBackgroundServiceRunIntervalInMinutes = 10;

	public const int DeleteOldLogsAfterDays = 1;
	public const int DeleteOldMessagesAfterHours = 1;

	public const int OtpSize = 8;
	public const int OtpExpirationMinutes = 3;
	public const int OtpMaxValidationAttempts = 3;

	public const int JwtTokenExpirationMinutes = 1;

	// The UI limits attachments to 1.2 MB, which becomes roughly 1.6 MB after Base64 encoding.
	// 4 MiB leaves room for the message text while still bounding memory/DB usage per request.
	public const int MaxSecretMessageCipherTextBytes = 4 * 1024 * 1024;
	public const int MaxRequestBodyBytes = 8 * 1024 * 1024;

	public static class SessionKeys
	{
		public const string RecentlyStoredSecretMessagesList = "RECENTLY_STORED_SECRET_MESSAGES_LIST";
	}

	public static class MemoryKeys
	{
		public const string SecretMessageOtp = "SECRET_MESSAGE_OTP";
		public const string SecretMessageDeliveryNotificationQueue = "SECRET_MESSAGE_DELIVERY_NOTIFICATION_QUEUE";
	}

	public static class RateLimit
	{
		public const string NoLimit = "no-limit";
		public const string GlobalLimit = "global-rate-limit";

		public const int GlobalPermitLimitPerMinute = 1000;
		public const int EndpointHitLimitPerMinute = 30;
	}
}