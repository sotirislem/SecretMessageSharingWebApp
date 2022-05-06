import { Component } from '@angular/core';
import { SjclService } from '../../services/sjcl.service';
import { Router } from '@angular/router';
import { LoaderService } from '../../services/loader.service';
import { ApiClientService } from '../../services/api-client.service';
import { Routes } from '../../../constants';


@Component({
	selector: 'app-home',
	templateUrl: './home.component.html',
	styleUrls: ['./home.component.css']
})
export class HomeComponent {
	secretTextArea: string;
	enryptionInProgress: boolean;

	constructor(private sjclService: SjclService, private router: Router, private loaderService: LoaderService, private apiClientService: ApiClientService) {
	}

	onSubmit() {
		this.enryptionInProgress = true;

		setTimeout(() => {
			this.sjclService.encryptMessage(this.secretTextArea.trim()).then((secretMessage) => {

				this.apiClientService.saveSecretMessage(secretMessage).subscribe(response => {

					this.router.navigate([Routes.Root_SaveSecretMessage], {
						state: {
							secretMsgId: response,
							secretMsgKey: this.sjclService.retrieveEncryptionKeyFromSessionStorage()
						}
					});

				});

			});
		}, 500);
	}
}
