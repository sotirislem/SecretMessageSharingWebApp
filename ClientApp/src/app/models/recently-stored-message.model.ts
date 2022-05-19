export interface RecentlyStoredMessage {
	id: string;
	createdDateTime: Date;
	deliveryDetails?: DeliveryDetails;
}

export interface DeliveryDetails {
	deliveredAt: Date;
	recipientIP: string;
	recipientClientInfo: string;
}
