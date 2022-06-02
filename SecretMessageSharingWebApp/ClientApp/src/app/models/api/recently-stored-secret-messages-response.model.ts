export interface RecentlyStoredSecretMessagesResponse {
	recentlyStoredSecretMessages: RecentlyStoredSecretMessage[]
}

export interface RecentlyStoredSecretMessage {
	id: string;
	createdDateTime: Date;
	deliveryDetails?: DeliveryDetails;
}

export interface DeliveryDetails {
	deliveredAt: Date;
	recipientIP: string;
	recipientClientInfo: string;
}
