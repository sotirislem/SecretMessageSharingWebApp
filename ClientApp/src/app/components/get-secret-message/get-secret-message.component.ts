import { Component } from '@angular/core';
import { SecretMessage } from '../../models/secret-message.model';
import { DecryptionResult, SjclDecryptionResult } from '../../models/sjcl-decryption-result.model';

import { ApiClientService } from '../../services/api-client.service';
import { SjclService } from '../../services/sjcl.service';

import { ActivatedRoute } from '@angular/router';

enum ComponentState {
	LoadingMessage,
	ReadyNoMessage,
	ReadyWithMessage
}

@Component({
	selector: 'app-get-secret-message',
	templateUrl: './get-secret-message.component.html',
	styleUrls: ['./get-secret-message.component.css']
})
export class GetSecretMessageComponent {
	readonly ComponentState = ComponentState;
	readonly DecryptionResult = DecryptionResult;

	componentState: ComponentState;
	messageId: string;
	decryptionResult: SjclDecryptionResult;

	constructor(route: ActivatedRoute, apiClientService: ApiClientService, private sjclService: SjclService) {
		const id = route.snapshot.queryParams.id;
		const encryptionKey = route.snapshot.fragment!;

		this.componentState = ComponentState.LoadingMessage;
		apiClientService.getSavedSecretMessage(id).subscribe((secretMessageData: SecretMessage) => {
			if (secretMessageData) {
				this.decryptionResult = this.sjclService.decryptMessage(secretMessageData, encryptionKey);
				this.componentState = ComponentState.ReadyWithMessage;
			} else {
				this.componentState = ComponentState.ReadyNoMessage;
			}
		});

		this.messageId = id;
	}
}
