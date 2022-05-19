import { Component } from '@angular/core';
import { finalize } from 'rxjs/operators';

import { SjclService } from '../../services/sjcl.service';
import { ApiClientService } from '../../services/api-client.service';
import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';

import { Router } from '@angular/router';
import { Routes } from '../../../constants';

@Component({
	selector: 'app-home',
	templateUrl: './home.component.html',
	styleUrls: ['./home.component.css']
})
export class HomeComponent {
	readonly Routes = Routes;

	secretMsgPlainText: string;
	encryptionInProgress: boolean;

	constructor(
		private router: Router,
		private sjclService: SjclService,
		private apiClientService: ApiClientService,
		private secretMessageDeliveryNotificationHubService: SecretMessageDeliveryNotificationHubService
	) { }

	async onEncryptButtonClick() {
		this.encryptionInProgress = true;

		await this.secretMessageDeliveryNotificationHubService.initHubConnection();

		setTimeout(() => {
			const [secretMessage, encryptionKey] = this.sjclService.encryptMessage(this.secretMsgPlainText.trim());

			this.apiClientService.storeSecretMessage(secretMessage)
				.pipe(finalize(() => {
					this.encryptionInProgress = false;
				}))
				.subscribe(response => {
					this.navigateToNewSecretMessagePage(response, encryptionKey);
				});
		}, 500);
	}

	private navigateToNewSecretMessagePage(secretMsgId: string, secretMsgKey: string) {
		this.router.navigate([Routes.NewSecretMessage], {
			state: {
				secretMsgPlainText: this.secretMsgPlainText,
				secretMsgId: secretMsgId,
				secretMsgKey: secretMsgKey
			}
		});
	}
}
