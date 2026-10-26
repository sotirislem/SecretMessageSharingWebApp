import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { StoreNewSecretMessageRequest } from '../models/api/store-new-secret-message-request.model';
import { ValidateSecretMessageOtpRequest } from '../models/api/validate-secret-message-otp-request.model';
import { GetSecretMessageResponse } from '../models/api/get-secret-message-response.model';
import { RecentlyStoredSecretMessagesResponse } from '../models/api/recently-stored-secret-messages-response.model';
import { VerifySecretMessageResponse } from '../models/api/verify-secret-message-response.model';
import { ValidateSecretMessageOtpResponse } from '../models/api/validate-secret-message-otp-response.model';
import { ClientLocation } from '../models/client-location.model';
import { API_URL } from '../tokens';

@Injectable({
	providedIn: 'root'
})
export class ApiClientService {
	private readonly httpClient = inject(HttpClient);
	private readonly baseApiUrl = `${inject(API_URL)}/api/secret-messages`;

	storeSecretMessage(request: StoreNewSecretMessageRequest): Observable<string> {
		return this.httpClient.post<string>(this.getApiUrl(), request);
	}

	verifySecretMessage(id: string): Observable<VerifySecretMessageResponse> {
		return this.httpClient.get<VerifySecretMessageResponse>(this.getApiUrl('verify/' + id));
	}

	acquireSecretMessageOtp(id: string, location?: ClientLocation): Observable<void> {
		// The OTP email renders its expiry in the recipient's time zone, so the
		// request context is sent here too. Coordinates are not needed and never sent.
		const headers = this.appendLocationHeaders(new HttpHeaders(), location);

		return this.httpClient.get<void>(this.getApiUrl('otp/' + id), { headers });
	}

	validateSecretMessageOtp(id: string, otpCode: string): Observable<ValidateSecretMessageOtpResponse> {
		return this.httpClient.post<ValidateSecretMessageOtpResponse>(
			this.getApiUrl('otp/' + id),
			<ValidateSecretMessageOtpRequest>{ otpCode });
	}

	getSecretMessage(
		id: string,
		keyHash: string,
		authToken?: string,
		location?: ClientLocation): Observable<GetSecretMessageResponse> {
		// Keep the key-hash verifier out of the URL so it cannot leak through
		// browser history, server access logs, proxies, or analytics.
		let headers = new HttpHeaders().set('X-Key-Hash', keyHash);
		if (authToken) {
			headers = headers.append('Authorization', 'Bearer ' + authToken);
		}

		headers = this.appendLocationHeaders(headers, location);

		return this.httpClient.get<GetSecretMessageResponse>(this.getApiUrl(id), { headers });
	}

	getRecentlyStoredSecretMessages(): Observable<RecentlyStoredSecretMessagesResponse> {
		return this.httpClient.get<RecentlyStoredSecretMessagesResponse>(this.getApiUrl());
	}

	getRecentlyStoredSecretMessagesUrl(): string {
		return this.getApiUrl();
	}

	deleteRecentlyStoredSecretMessage(id: string): Observable<void> {
		return this.httpClient.delete<void>(this.getApiUrl(id));
	}

	private getApiUrl(apiPath: string = ''): string {
		return apiPath ? `${this.baseApiUrl}/${apiPath}` : this.baseApiUrl;
	}

	/**
	 * Coordinates are only sent as headers: keeping them out of the URL means they
	 * cannot end up in browser history, server access logs, or proxy logs.
	 */
	private appendLocationHeaders(headers: HttpHeaders, location?: ClientLocation): HttpHeaders {
		if (!location) {
			return headers;
		}

		if (location.timezone) {
			headers = headers.set('X-Client-Timezone', location.timezone);
		}

		if (location.utcOffsetMinutes !== undefined) {
			headers = headers.set('X-Client-Utc-Offset-Minutes', location.utcOffsetMinutes.toString());
		}

		if (location.language) {
			headers = headers.set('X-Client-Language', location.language);
		}

		if (location.deviceModel) {
			headers = headers.set('X-Client-Device-Model', location.deviceModel);
		}

		if (location.latitude !== undefined && location.longitude !== undefined) {
			headers = headers
				.set('X-Client-Latitude', location.latitude.toFixed(3))
				.set('X-Client-Longitude', location.longitude.toFixed(3));
		}

		return headers;
	}
}
