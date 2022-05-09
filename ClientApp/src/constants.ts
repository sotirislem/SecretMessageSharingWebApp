export abstract class Constants {
	static readonly ENCRYPTION_KEY_NAME: string = 'encryptionKey';
}

export abstract class Routes {
	static readonly Root: string = '/';
	static readonly Root_AppInfo: string = 'app-info';
	static readonly Root_SaveSecretMessage: string = 'save-secret-message';
	static readonly Root_GetSecretMessage: string = 'get-secret-message';
}
