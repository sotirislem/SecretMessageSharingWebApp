export abstract class Constants {
	static readonly ATTACHMENT_FILESIZE_MAX_MB = 1;
	// Keep in sync with Constants.MaxSecretMessageCipherTextBytes on the server.
	static readonly MAX_ENCRYPTED_PAYLOAD_BYTES = 1_500_000;
	static readonly MAX_ENCRYPTED_PAYLOAD_BASE64_LENGTH = Math.ceil(Constants.MAX_ENCRYPTED_PAYLOAD_BYTES / 3) * 4;
	static readonly AUTOCLEAR_INTERVAL_MINUTES = 5;
	static readonly OTP_EXPIRATION_MINUTES = 3;
}

export abstract class Routes {
	static readonly Root: string = '/';
	static readonly AppInfo: string = 'app-info';
	static readonly CreateSecretMessage: string = 'create-secret-message';
	static readonly NewSecretMessage: string = 'new-secret-message';
	static readonly GetSecretMessage: string = 'get-secret-message';
	static readonly RecentlyStoredMessages: string = 'recently-stored-messages';
}
