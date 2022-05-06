import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiClientService } from '../../services/api-client.service';
import { SjclService } from '../../services/sjcl.service';

@Component({
	selector: 'app-get-secret-message',
	templateUrl: './get-secret-message.component.html'
})
export class GetSecretMessageComponent {
	decryptedMsg: string;

	constructor(route: ActivatedRoute, apiClientService: ApiClientService, sjclService: SjclService) {
		const id = route.snapshot.queryParams.id;
		const encryptionKey = route.snapshot.queryParams.encryptionKey;

		apiClientService.getSavedSecretMessage(id).subscribe(secretMessageData => {
			sjclService.decryptMessage(encryptionKey, secretMessageData).then(decryptedMsg => {
				this.decryptedMsg = decryptedMsg;
			});
		});
	}
}
