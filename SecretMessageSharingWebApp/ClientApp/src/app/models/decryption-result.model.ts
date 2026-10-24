import { SecretMessage } from "./secret-message.model"

export enum DecryptionStatus {
	OK,
	Error
}

export interface DecryptionResult {
	status: DecryptionStatus,
	decryptedMsg: SecretMessage,
	errorMsg: string
}
