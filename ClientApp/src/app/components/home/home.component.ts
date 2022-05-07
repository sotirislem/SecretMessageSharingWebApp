import { Component } from '@angular/core';
import { SjclService } from '../../services/sjcl.service';
import { Router } from '@angular/router';
import { ApiClientService } from '../../services/api-client.service';
import { Routes } from '../../../constants';
import { SecretMessage } from '../../models/SecretMessage';

@Component({
	selector: 'app-home',
	templateUrl: './home.component.html',
	styleUrls: ['./home.component.css']
})
export class HomeComponent {
	secretTextArea: string;
	encryptionInProgress: boolean;

	constructor(private router: Router, private sjclService: SjclService, private apiClientService: ApiClientService) {
	}

	onSubmit() {
		this.encryptionInProgress = true;

		setTimeout(() => {
			const [ secretMessage, encryptionKey ] = this.sjclService.encryptMessage(this.secretTextArea.trim());
			this.apiClientService.saveSecretMessage(secretMessage).subscribe(response => {

				this.router.navigate([Routes.Root_SaveSecretMessage], {
					state: {
						secretMsgId: response,
						secretMsgKey: encryptionKey
					}
				});

			});
		}, 500);
	}
}
