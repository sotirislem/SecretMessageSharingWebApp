namespace SecretMessageSharingWebApp
{
	public static class Constants
	{
		public const string SessionKey_RecentlyStoredSecretMessagesList = "RECENTLY_STORED_SECRET_MESSAGES_LIST";
		public const string MemoryKey_SecretMessageSignalRConnectionId = "SignalR_Connection_ID";
		public const string MemoryKey_SecretMessageOtp = "SECRET_MESSAGE_OTP";

		public const int SessionIdleTimeoutInMinutes = 15;
		public const int MemoryCacheValueExpirationTimeInMinutes = 15;
		public const int DbAutoCleanerBackgroundServiceRunIntervalInMinutes = 10;

		public const string LocalTimeZone = "GTB Standard Time";
	}
}
