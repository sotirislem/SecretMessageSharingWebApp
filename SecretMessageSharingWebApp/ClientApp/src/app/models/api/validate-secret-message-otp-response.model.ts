export interface ValidateSecretMessageOtpResponse {
	isValid: boolean;
	canRetry: boolean;
	hasExpired: boolean;
	authToken?: string;
}
