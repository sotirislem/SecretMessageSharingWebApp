import { Component, ViewChild } from '@angular/core';
import { IndividualConfig, ToastrService } from 'ngx-toastr';
import { ActivatedRoute } from '@angular/router';
import { NgbModal, NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { Clipboard } from '@angular/cdk/clipboard';

import { GetSecretMessageResponse } from '../../models/api/get-secret-message-response.model';
import { DecryptionResult, SjclDecryptionResult } from '../../models/sjcl-decryption-result.model';
import { SecretMessage } from '../../models/secret-message.model';
import { VerifySecretMessageResponse } from '../../models/api/verify-secret-message-response.model';
import { Constants } from '../../../constants';

import { ApiClientService } from '../../services/api-client.service';
import { SjclService } from '../../services/sjcl.service';
import { FileService } from '../../services/file.service';
import { OtpInputModalComponent } from '../otp-input-modal/otp-input-modal.component';

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
	@ViewChild('ngbTooltipElement') ngbTooltip: NgbTooltip;
	ngbTooltipClearTimer: NodeJS.Timeout;

	readonly ComponentState = ComponentState;
	readonly DecryptionResult = DecryptionResult;

	componentState: ComponentState;
	messageId: string;
	decryptionResult: SjclDecryptionResult;
	msgAutoClearTimeoutTriggered: boolean;
	secretMessageTextAsHtml: string;

	constructor(
		route: ActivatedRoute,
		private apiClientService: ApiClientService,
		private sjclService: SjclService,
		private toastrService: ToastrService,
		private fileService: FileService,
		private clipboard: Clipboard,
		private modalService: NgbModal
	) {
		this.messageId = route.snapshot.queryParams.id;
		const encryptionKey = route.snapshot.fragment!;

		this.componentState = ComponentState.LoadingMessage;

		apiClientService.verifySecretMessage(this.messageId).subscribe((response: VerifySecretMessageResponse) => {
			if (!response.exists) {
				this.componentState = ComponentState.ReadyNoMessage;
			}
			else if (!response.requiresOtp) {
				this.getSecretMessage(encryptionKey);
			}
			else {
				const modal = this.modalService.open(OtpInputModalComponent, { centered: true, backdrop: 'static' });
				modal.componentInstance.messageId = this.messageId;

				modal.result.then((token: string) => {
					this.getSecretMessage(encryptionKey, token);
				});
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

	copyMessageContentToClipboard() {
		if (this.msgAutoClearTimeoutTriggered) return;

		this.clipboard.copy(this.decryptionResult.decryptedMsg.plainText);

		clearTimeout(this.ngbTooltipClearTimer);
		if (!this.ngbTooltip.isOpen()) this.ngbTooltip.open();

		this.ngbTooltipClearTimer = setTimeout(() => {
			this.ngbTooltip.close();
		}, 5000);
	}

	private getSecretMessage(encryptionKey: string, token?: string) {
		this.apiClientService.getSecretMessage(this.messageId, token).subscribe((response: GetSecretMessageResponse) => {
			if (!response) return;

			this.decryptionResult = this.sjclService.decryptMessage(response.data, encryptionKey);

			this.componentState = ComponentState.ReadyWithMessage;
			if (this.decryptionResult.result === DecryptionResult.OK) {
				this.convertDecryptedMsgPlainTextToHtml();
				this.setDecryptedMsgAutoclearTimeout();
			}

			this.displayDeliveryNotificationSentToast(response.deliveryNotificationSent);
		});
	}

	private convertDecryptedMsgPlainTextToHtml(): void {
		this.secretMessageTextAsHtml = this.decryptionResult.decryptedMsg.plainText.linkify();
	}

	private setDecryptedMsgAutoclearTimeout() {
		const clearTimeout = Constants.AUTOCLEAR_INTERVAL_MINUTES * (60 * 1000);

		setTimeout(() => {
			this.msgAutoClearTimeoutTriggered = true;

			this.decryptionResult.decryptedMsg = {} as SecretMessage
			this.secretMessageTextAsHtml = '--- Message auto deleted ---';
		}, clearTimeout);
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
}
