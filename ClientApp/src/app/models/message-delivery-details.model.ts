export interface MessageDeliveryDetails {
	messageId: string;
	messageCreatedOn: Date;
	messageDeliveredOn: Date;
	recipientIp: string;
	recipientClientInfo: string;
}
