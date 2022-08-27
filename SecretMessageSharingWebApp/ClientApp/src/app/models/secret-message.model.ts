export interface SecretMessage {
	plainText: string,
	containsFile: boolean,
	base64BlobFile: string,
	fileName: string
}
