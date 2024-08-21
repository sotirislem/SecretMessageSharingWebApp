export interface ApiErrorResponse {
	statusCode: number;
	message: string;
	errors: { [key: string]: string[] };
}
