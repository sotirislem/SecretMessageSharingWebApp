import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SecretMessage } from '../models/SecretMessage';

@Injectable({
	providedIn: 'root'
})
export class ApiClientService {
	private readonly baseApiUrl: string;

	constructor(@Inject('BASE_URL') baseUrl: string, private httpClient: HttpClient) {
		this.baseApiUrl = `${baseUrl}/api`;
	}

	saveSecretMessage(secretMessage: SecretMessage): Observable<string> {
		const url = this.getApiUrl('secret-messages/store');
		return this.httpClient.post(url, secretMessage, { responseType: "text" });
	}

	getSavedSecretMessage(id: string): Observable<SecretMessage> {
		const url = this.getApiUrl('secret-messages/get/' + id);
		return this.httpClient.get<SecretMessage>(url);
	}

	private getApiUrl(apiPath: string) {
		return `${this.baseApiUrl}/${apiPath}`;
	}
}
