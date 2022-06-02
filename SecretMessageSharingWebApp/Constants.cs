namespace SecretMessageSharingWebApp
{
	public static class Constants
	{
		public const string SessionKey_RecentlyStoredSecretMessagesList = "RECENTLY_STORED_SECRET_MESSAGES_LIST";

		public const int SessionIdleTimeoutInMinutes = 15;
		public const int MemoryCacheValueExpirationTimeInMinutes = 15;
		public const int SecretMessagesAutoCleanerBackgroundServiceRunIntervalInMinutes = 10;
	}
}
