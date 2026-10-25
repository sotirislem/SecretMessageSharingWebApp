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
	recipientCity?: string | null;
	recipientRegion?: string | null;
	recipientCountry?: string | null;
	recipientCountryCode?: string | null;
	recipientLocationSource?: string | null;
	recipientIsp?: string | null;
	recipientTimezone?: string | null;
	recipientUtcOffsetMinutes?: number | null;
	recipientLanguage?: string | null;
	otpVerified?: boolean;
}
