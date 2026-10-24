import { SecretMessageData } from "./common/secret-message-data.model"

export interface EncryptionResult {
	secretMessageData: SecretMessageData,
	encryptionKeyAsBase64Url: string,
	encryptionKeySha256: string
}
