import { SecretMessageData } from "../common/secret-message-data.model";

export interface StoreNewSecretMessageRequest {
	secretMessageData: SecretMessageData;
	otp: OtpSettings;
	encryptionKeySha256: string;
	notificationEmail?: string | null;
}

export interface OtpSettings {
	required: boolean;
	recipientsEmail: string;
}
