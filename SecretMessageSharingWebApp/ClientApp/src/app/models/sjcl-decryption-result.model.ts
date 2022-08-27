import { SecretMessage } from "./secret-message.model"

export enum DecryptionResult {
	OK,
	Error
}

export type SjclDecryptionResult = {
	result: DecryptionResult,
	decryptedMsg: SecretMessage,
	errorMsg: string
}
