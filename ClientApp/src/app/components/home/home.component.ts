import { Component } from '@angular/core';
import { SjclService } from '../../services/sjcl.service';
import { Router } from '@angular/router';
import { ApiClientService } from '../../services/api-client.service';
import { Routes } from '../../../constants';
import { finalize } from 'rxjs/operators';
import { SignalRService } from '../../services/signalr.service';

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
		private signalRService: SignalRService
	) { }

	async onEncryptButtonClick() {
		this.encryptionInProgress = true;

		await this.signalRService.initHubConnection();

		setTimeout(() => {
			const [secretMessage, encryptionKey] = this.sjclService.encryptMessage(this.secretMsgPlainText.trim());

			this.apiClientService.saveSecretMessage(secretMessage)
				.pipe(finalize(() => {
					this.encryptionInProgress = false;
				}))
				.subscribe(response => {
					this.router.navigate([Routes.Root_NewSecretMessage], {
						state: {
							secretMsgPlainText: this.secretMsgPlainText,
							secretMsgId: response,
							secretMsgKey: encryptionKey
						}
					});
				});
		}, 500);
	}
}
