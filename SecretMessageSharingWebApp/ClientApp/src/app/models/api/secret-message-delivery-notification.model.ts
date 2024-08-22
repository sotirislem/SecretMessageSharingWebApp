export interface SecretMessageDeliveryNotification {
	messageId: string;
	messageCreatedOn: Date;
	messageDeliveredOn: Date;
	recipientIP: string;
	recipientClientInfo: string;
	isSelfNotification: boolean;
}
