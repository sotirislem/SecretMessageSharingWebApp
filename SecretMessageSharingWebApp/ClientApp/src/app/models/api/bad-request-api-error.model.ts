export interface BadRequestApiError {
	statusCode: number;
	message: string;
	errors: { [key: string]: string[] };
}
