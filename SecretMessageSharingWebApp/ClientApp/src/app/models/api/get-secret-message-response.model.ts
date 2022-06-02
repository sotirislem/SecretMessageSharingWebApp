import { SecretMessageData } from "../common/secret-message-data.model";

export interface GetSecretMessageResponse {
	data: SecretMessageData;
	deliveryNotificationSent: boolean;
}
