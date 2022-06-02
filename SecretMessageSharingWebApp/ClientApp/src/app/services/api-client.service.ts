import { HttpClient, HttpResponse } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GetSecretMessageResponse } from '../models/api/get-secret-message-response.model';
import { RecentlyStoredSecretMessagesResponse } from '../models/api/recently-stored-secret-messages-response.model';
import { SecretMessageData } from '../models/common/secret-message-data.model';
import { StoreNewSecretMessageRequest } from '../models/api/store-new-secret-message-request.model';

@Injectable({
	providedIn: 'root'
})
export class ApiClientService {
	private readonly baseApiUrl: string;

	constructor(@Inject('API_URL') apiUrl: string, private httpClient: HttpClient) {
		this.baseApiUrl = `${apiUrl}/api`;
	}

	storeSecretMessage(secretMessageData: SecretMessageData): Observable<string> {
		const request = <StoreNewSecretMessageRequest>{ data: secretMessageData };

		const url = this.getApiUrl('secret-messages/store');
		return this.httpClient.post<string>(url, request, { responseType: 'text' as 'json' });
	}

	getSecretMessage(id: string): Observable<GetSecretMessageResponse> {
		const url = this.getApiUrl('secret-messages/get/' + id);
		return this.httpClient.get<GetSecretMessageResponse>(url);
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
