export interface VerifySecretMessageResponse {
	id: string;
	exists: boolean;
	requiresOtp?: boolean;
}
