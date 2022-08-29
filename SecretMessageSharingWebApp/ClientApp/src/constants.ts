export abstract class Constants {
	static readonly ATTACHMENT_FILESIZE_MAX_MB = 1.2;
	static readonly AUTOCLEAR_INTERVAL_MINUTES = 3;
}

export abstract class Routes {
	static readonly Root: string = '/';
	static readonly AppInfo: string = 'app-info';
	static readonly NewSecretMessage: string = 'new-secret-message';
	static readonly GetSecretMessage: string = 'get-secret-message';
	static readonly RecentlyStoredMessages: string = 'recently-stored-messages';
}
