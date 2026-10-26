namespace SecretMessageSharingWebApp;

public static class Constants
{
	public const string AppName = "SecretMessageSharingWebApp";

	public const string AppUrl = "https://secret-message-sharing-web-app.azurewebsites.net/";

	public const int SessionIdleTimeoutInMinutes = 15;
	public const int MemoryCacheValueExpirationTimeInMinutes = 15;
	public const int DbAutoCleanerBackgroundServiceRunIntervalInMinutes = 10;

	public const int DeleteOldLogsAfterDays = 1;
	public const int DeleteOldMessagesAfterHours = 1;

	public const int OtpSize = 8;
	public const int OtpExpirationMinutes = 3;
	public const int OtpMaxValidationAttempts = 3;

	public const int JwtTokenExpirationMinutes = 1;

	// The UI limits attachments to 1 MiB, which becomes roughly 1.9 MB after encryption and Base64 encoding.
	// This cap keeps the stored Cosmos DB document under the 2 MB item limit:
	// 1,500,000 decoded ciphertext bytes -> 2,000,000 Base64 characters -> ~2.0 MB document.
	public const int MaxSecretMessageCipherTextBytes = 1_500_000;
	// Kestrel-level guard for the HTTP request body. The largest valid body is ~2 MB
	// (2,000,000 Base64 chars plus JSON metadata), so 3 MiB leaves headroom.
	public const int MaxRequestBodyBytes = 3 * 1024 * 1024;

	public static class SessionKeys
	{
		public const string RecentlyStoredSecretMessagesList = "RECENTLY_STORED_SECRET_MESSAGES_LIST";
	}

	public static class MemoryKeys
	{
		public const string SecretMessageOtp = "SECRET_MESSAGE_OTP";
		public const string SecretMessageDeliveryNotificationQueue = "SECRET_MESSAGE_DELIVERY_NOTIFICATION_QUEUE";
	}

	public static class LocationSources
	{
		public const string Device = "device";
		public const string Ip = "ip";
	}

	public static class RateLimit
	{
		public const string NoLimit = "no-limit";
		public const string GlobalLimit = "global-rate-limit";

		public const int GlobalPermitLimitPerMinute = 1000;
		public const int EndpointHitLimitPerMinute = 30;

		// SignalR handshakes and reconnects get their own per-IP bucket so a reconnect storm
		// cannot exhaust the shared application-wide limit.
		public const int SignalRPermitLimitPerMinute = 120;
	}
}