export enum DecryptionResult {
	OK,
	Error
}

export type SjclDecryptionResult = {
	result: DecryptionResult,
	decryptedMsg: string,
	errorMsg: string
}
