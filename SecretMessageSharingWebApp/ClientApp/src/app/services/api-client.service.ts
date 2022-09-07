import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { StoreNewSecretMessageRequest } from '../models/api/store-new-secret-message-request.model';
import { ValidateSecretMessageOtpRequest } from '../models/api/validate-secret-message-otp-request.model';

import { GetSecretMessageResponse } from '../models/api/get-secret-message-response.model';
import { RecentlyStoredSecretMessagesResponse } from '../models/api/recently-stored-secret-messages-response.model';
import { VerifySecretMessageResponse } from '../models/api/verify-secret-message-response.model';
import { ValidateSecretMessageOtpResponse } from '../models/api/validate-secret-message-otp-response.model';

@Injectable({
	providedIn: 'root'
})
export class ApiClientService {
	private readonly baseApiUrl: string;

	constructor(@Inject('API_URL') apiUrl: string, private httpClient: HttpClient) {
		this.baseApiUrl = `${apiUrl}/api`;
	}

	storeSecretMessage(request: StoreNewSecretMessageRequest): Observable<string> {
		const url = this.getApiUrl('secret-messages/store');
		return this.httpClient.post<string>(url, request);
	}

	verifySecretMessage(id: string): Observable<VerifySecretMessageResponse> {
		const url = this.getApiUrl('secret-messages/verify/' + id);
		return this.httpClient.get<VerifySecretMessageResponse>(url);
	}

	acquireSecretMessageOtp(id: string): Observable<boolean> {
		const url = this.getApiUrl('secret-messages/acquire-otp/' + id);
		return this.httpClient.post<boolean>(url, null);
	}

	validateSecretMessageOtp(id: string, otpCode: string): Observable<ValidateSecretMessageOtpResponse> {
		const url = this.getApiUrl('secret-messages/validate-otp/' + id);
		return this.httpClient.post<ValidateSecretMessageOtpResponse>(url, <ValidateSecretMessageOtpRequest>{ otpCode });
	}

	getSecretMessage(id: string, token?: string): Observable<GetSecretMessageResponse> {
		const url = this.getApiUrl('secret-messages/get/' + id);

		let headers = new HttpHeaders();
		if (token) {
			headers = headers.set('Authorization', 'Bearer ' + token);
		}

		return this.httpClient.get<GetSecretMessageResponse>(url, { headers });
	}

	getRecentlyStoredSecretMessages(): Observable<RecentlyStoredSecretMessagesResponse> {
		const url = this.getApiUrl('secret-messages/getRecentlyStoredSecretMessages');
		return this.httpClient.get<RecentlyStoredSecretMessagesResponse>(url);
	}

	deleteRecentlyStoredSecretMessage(id: string): Observable<boolean> {
		const url = this.getApiUrl('secret-messages/deleteRecentlyStoredSecretMessage/' + id);
		return this.httpClient.delete<boolean>(url);
	}

	private getApiUrl(apiPath: string) {
		return `${this.baseApiUrl}/${apiPath}`;
	}
}
