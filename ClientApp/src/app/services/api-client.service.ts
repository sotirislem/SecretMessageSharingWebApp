import { HttpClient, HttpResponse } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GetSecretMessageResponse } from '../models/get-secret-message-response.model';
import { RecentlyStoredMessage } from '../models/recently-stored-message.model';
import { SecretMessageData } from '../models/secret-message-data.model';

@Injectable({
	providedIn: 'root'
})
export class ApiClientService {
	private readonly baseApiUrl: string;

	constructor(@Inject('API_URL') apiUrl: string, private httpClient: HttpClient) {
		this.baseApiUrl = `${apiUrl}/api`;
	}

	storeSecretMessage(secretMessage: SecretMessageData): Observable<string> {
		const url = this.getApiUrl('secret-messages/store');
		return this.httpClient.post<string>(url, secretMessage, { responseType: 'text' as 'json' });
	}

	getSecretMessage(id: string): Observable<GetSecretMessageResponse> {
		const url = this.getApiUrl('secret-messages/get/' + id);
		return this.httpClient.get<GetSecretMessageResponse>(url);
	}

	getRecentlyStoredMessages(): Observable<RecentlyStoredMessage[]> {
		const url = this.getApiUrl('secret-messages/getRecentMessages');
		return this.httpClient.get<RecentlyStoredMessage[]>(url);
	}

	deleteRecentlyStoredMessage(id: string): Observable<boolean> {
		const url = this.getApiUrl('secret-messages/deleteRecentMessage/' + id);
		return this.httpClient.delete<boolean>(url);
	}

	private getApiUrl(apiPath: string) {
		return `${this.baseApiUrl}/${apiPath}`;
	}
}
