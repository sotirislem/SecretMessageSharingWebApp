export interface MessageDeliveryNotification {
	messageCreatedOn: Date;
	messageDeliveredOn: Date;
	recipientIp: string;
	recipientClientInfo: string;
}
