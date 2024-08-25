import { SecretMessageData } from "./common/secret-message-data.model"

export type SjclEncryptionResult = {
	secretMessageData: SecretMessageData,
	encryptionKeyAsBase64url: string,
	encryptionKeySha256: string
}
