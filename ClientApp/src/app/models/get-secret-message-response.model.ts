import { SecretMessageData } from "./secret-message-data.model";

export interface GetSecretMessageResponse {
	secretMessageData: SecretMessageData;
	deliveryNotificationSent: boolean;
}
