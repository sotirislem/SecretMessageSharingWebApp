import { HttpClient, HttpResponse } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GetSecretMessageResponse } from '../models/get-secret-message-response.model';
import { SecretMessageData } from '../models/secret-message-data.model';

@Injectable({
	providedIn: 'root'
})
export class ApiClientService {
	private readonly baseApiUrl: string;

	constructor(@Inject('API_URL') apiUrl: string, private httpClient: HttpClient) {
		this.baseApiUrl = `${apiUrl}/api`;
	}

	saveSecretMessage(secretMessage: SecretMessageData): Observable<string> {
		const url = this.getApiUrl('secret-messages/store');
		return this.httpClient.post(url, secretMessage, { responseType: "text" });
	}

	getSavedSecretMessage(id: string): Observable<GetSecretMessageResponse> {
		const url = this.getApiUrl('secret-messages/get/' + id);
		return this.httpClient.get<GetSecretMessageResponse>(url);
	}

	private getApiUrl(apiPath: string) {
		return `${this.baseApiUrl}/${apiPath}`;
	}
}
