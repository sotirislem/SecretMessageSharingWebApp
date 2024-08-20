namespace SecretMessageSharingWebApp;

public static class Constants
{
	public const string AppName = "SecretMessageSharingWebApp";
	public const string LocalTimeZone = "GTB Standard Time";

	public const int SessionIdleTimeoutInMinutes = 15;
	public const int MemoryCacheValueExpirationTimeInMinutes = 15;
	public const int DbAutoCleanerBackgroundServiceRunIntervalInMinutes = 10;

	public const int DeleteOldLogsAfterDays = 1;
	public const int DeleteOldMessagesAfterHours = 1;

	public const int OtpSize = 8;
	public const int OtpExpirationMinutes = 3;
	public const int OtpMaxValidationRetries = 3;

	public const int JwtTokenExpirationMinutes = 1;

	public static class SessionKeys
	{
		public const string RecentlyStoredSecretMessagesList = "RECENTLY_STORED_SECRET_MESSAGES_LIST";
	}

	public static class MemoryKeys
	{
		public const string SecretMessageCreatorClientId = "SECRET_MESSAGE_CREATOR_CLIENT_ID";
		public const string SecretMessageOtp = "SECRET_MESSAGE_OTP";
		public const string SecretMessageDeliveryNotificationQueue = "SECRET_MESSAGE_DELIVERY_NOTIFICATION_QUEUE";
	}
}