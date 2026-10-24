export interface SecretMessage {
	plainText: string,
	containsFile: boolean,
	fileName: string | null,
	base64BlobFile: string | null,
	containsUsernameAndPassword: boolean,
	username: string,
	password: string
}
