export interface ValidateSecretMessageOtpResponse {
	isValid: boolean;
	hasExpired: boolean;
	authToken?: string;
}
