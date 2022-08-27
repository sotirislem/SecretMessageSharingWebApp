import { Component } from '@angular/core';
import { GetSecretMessageResponse } from '../../models/api/get-secret-message-response.model';
import { DecryptionResult, SjclDecryptionResult } from '../../models/sjcl-decryption-result.model';

import { ApiClientService } from '../../services/api-client.service';
import { SjclService } from '../../services/sjcl.service';
import { FileService } from '../../services/file.service';

import { ActivatedRoute } from '@angular/router';
import { IndividualConfig, ToastrService } from 'ngx-toastr';

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

	constructor(
		route: ActivatedRoute,
		apiClientService: ApiClientService,
		private sjclService: SjclService,
		private toastrService: ToastrService,
		private fileService: FileService
	) {
		this.messageId = route.snapshot.queryParams.id;
		const encryptionKey = route.snapshot.fragment!;

		this.componentState = ComponentState.LoadingMessage;
		apiClientService.getSecretMessage(this.messageId).subscribe((response: GetSecretMessageResponse) => {
			if (response) {
				this.decryptionResult = this.sjclService.decryptMessage(response.data, encryptionKey);
				this.componentState = ComponentState.ReadyWithMessage;

				this.displayDeliveryNotificationSentToast(response.deliveryNotificationSent);

				if (this.decryptionResult.result === DecryptionResult.OK)
					this.setDecryptedMsgAutoclearTimeout();
			} else {
				this.componentState = ComponentState.ReadyNoMessage;
			}
		});
	}

	downloadAttachment(): void {
		if (this.decryptionResult.decryptedMsg.containsFile) {
			const base64Blob = this.decryptionResult.decryptedMsg.base64BlobFile;
			const saveFileName = this.decryptionResult.decryptedMsg.fileName;

			this.fileService.saveBase64BlobAsFile(base64Blob, saveFileName);
		}
	}

	private displayDeliveryNotificationSentToast(deliveryNotificationSent: boolean) {
		const toastConfig = <IndividualConfig>{
			timeOut: 7_000,
			positionClass: 'toast-top-center',
			extendedTimeOut: 0
		};

		if (deliveryNotificationSent)
			this.toastrService.success('Delivery notification was sent', '', toastConfig);
		else
			this.toastrService.warning('Delivery notification could not be sent', '', toastConfig);
	}

	private setDecryptedMsgAutoclearTimeout() {
		const clearTimeout = 3 * (60 * 1000);

		setTimeout(() => {
			this.decryptionResult.decryptedMsg.plainText = '--- Message auto deleted ---';
			this.decryptionResult.decryptedMsg.containsFile = false;
			this.decryptionResult.decryptedMsg.base64BlobFile = '';
		}, clearTimeout);
	}
}
