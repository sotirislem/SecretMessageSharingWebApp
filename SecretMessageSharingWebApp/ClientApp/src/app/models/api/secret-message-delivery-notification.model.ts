export interface SecretMessageDeliveryNotification {
	messageId: string;
	messageCreatedOn: Date;
	messageDeliveredOn: Date;
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
