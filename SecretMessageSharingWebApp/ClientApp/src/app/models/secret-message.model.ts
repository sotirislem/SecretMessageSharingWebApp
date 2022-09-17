export interface SecretMessage {
	plainText: string,
	containsFile: boolean,
	fileName: string,
	base64BlobFile: string,
	containsUsernameAndPassword: boolean,
	username: string,
	password: string
}
