export interface ValidateSecretMessageOtpResponse {
	isValid: boolean;
	token?: string;
	canRetry: boolean;
	hasExpired: boolean;
}
