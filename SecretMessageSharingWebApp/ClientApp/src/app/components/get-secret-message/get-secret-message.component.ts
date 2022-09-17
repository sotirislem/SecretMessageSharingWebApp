import { Component, TemplateRef, ViewChild } from '@angular/core';
import { IndividualConfig, ToastrService } from 'ngx-toastr';
import { ActivatedRoute } from '@angular/router';
import { NgbModalOptions, NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { Clipboard } from '@angular/cdk/clipboard';

import { GetSecretMessageResponse } from '../../models/api/get-secret-message-response.model';
import { DecryptionResult, SjclDecryptionResult } from '../../models/sjcl-decryption-result.model';
import { SecretMessage } from '../../models/secret-message.model';
import { VerifySecretMessageResponse } from '../../models/api/verify-secret-message-response.model';

import { ApiClientService } from '../../services/api-client.service';
import { SjclService } from '../../services/sjcl.service';
import { FileService } from '../../services/file.service';
import { ModalService } from '../../services/modal.service';
import { OtpInputModalComponent } from '../modals/otp-input-modal/otp-input-modal.component';
import { RouterHelperService } from '../../services/router-helper.service';

enum ComponentState {
	LoadingMessage,
	ReadyNoMessage,
	ReadyWithMessage,
	Aborted,
	Error
}

@Component({
	templateUrl: './get-secret-message.component.html',
	styleUrls: ['./get-secret-message.component.css']
})
export class GetSecretMessageComponent {
	@ViewChild('getSecretMessageConfirmationModalBody') getSecretMessageConfirmationModalBody: TemplateRef<any>;

	readonly ComponentState = ComponentState;
	readonly DecryptionResult = DecryptionResult;

	componentState: ComponentState;

	messageId: string;
	decryptionResult: SjclDecryptionResult;
	msgAutoClearTimeoutTriggered: boolean;
	secretMessageTextAsHtml: string;

	constructor(
		route: ActivatedRoute,
		private routerHelperService: RouterHelperService,
		private apiClientService: ApiClientService,
		private sjclService: SjclService,
		private toastrService: ToastrService,
		private fileService: FileService,
		private clipboard: Clipboard,
		private modalService: ModalService
	) {
		this.messageId = route.snapshot.queryParams.id;
		const encryptionKey = route.snapshot.fragment!;

		this.componentState = ComponentState.LoadingMessage;

		apiClientService.verifySecretMessage(this.messageId).subscribe((response: VerifySecretMessageResponse) => {
			if (!response.exists) {
				this.componentState = ComponentState.ReadyNoMessage;
				return;
			}

			this.tryGetSecretMessage(encryptionKey, response.requiresOtp!);
		}, (error) => { this.componentState = ComponentState.Error; });
	}

	reload() {
		this.routerHelperService.reloadCurrentPage();
	}

	downloadAttachment(): void {
		if (this.decryptionResult.decryptedMsg.containsFile) {
			const base64Blob = this.decryptionResult.decryptedMsg.base64BlobFile;
			const saveFileName = this.decryptionResult.decryptedMsg.fileName;

			this.fileService.saveBase64BlobAsFile(base64Blob, saveFileName);
		}
	}

	copyUsernameToClipboard() {
		if (this.msgAutoClearTimeoutTriggered) return;

		this.clipboard.copy(this.decryptionResult.decryptedMsg.username);
	}

	copyPasswordToClipboard() {
		if (this.msgAutoClearTimeoutTriggered) return;

		this.clipboard.copy(this.decryptionResult.decryptedMsg.password);
	}

	copyMessageContentToClipboard() {
		if (this.msgAutoClearTimeoutTriggered) return;

		let msgContent = this.decryptionResult.decryptedMsg.plainText;
		if (this.decryptionResult.decryptedMsg.containsUsernameAndPassword) {
			msgContent += `\r\nUsername: ${this.decryptionResult.decryptedMsg.username}`;
			msgContent += `\r\nPassword: ${this.decryptionResult.decryptedMsg.password}`;
		}

		this.clipboard.copy(msgContent);
	}

	private tryGetSecretMessage(encryptionKey: string, requiresOtp: boolean) {
		this.modalService.openConfirmationModal(
			this.getSecretMessageConfirmationModalBody,
			'Message Retrieve Confirmation'
		).then((confirm) => {
			if (!confirm) {
				this.componentState = ComponentState.Aborted;
				return;
			}

			if (requiresOtp) {
				this.openOtpInputModal(encryptionKey);
			} else {
				this.getSecretMessage(encryptionKey);
			}
		});
	}

	private openOtpInputModal(encryptionKey: string) {
		this.modalService.openModal<string>(
			OtpInputModalComponent,
			{ messageId: this.messageId },
			<NgbModalOptions>{ centered: true, backdrop: 'static' }
		).then((token: string) => {
			if (token) {
				this.getSecretMessage(encryptionKey, token);
			} else {
				this.componentState = ComponentState.Error;
			}
		});
	}

	private getSecretMessage(encryptionKey: string, token?: string) {
		setTimeout(() => {
			this.apiClientService.getSecretMessage(this.messageId, token).subscribe((response: GetSecretMessageResponse) => {
				this.componentState = ComponentState.ReadyWithMessage;

				this.decryptionResult = this.sjclService.decryptMessage(response.data, encryptionKey);

				if (this.decryptionResult.result === DecryptionResult.OK) {
					this.convertDecryptedMsgPlainTextToHtml();
					this.setDecryptedMsgAutoclearTimeout();
				}

				this.displayDeliveryNotificationSentToast(response.deliveryNotificationSent);
			}, (error) => { this.componentState = ComponentState.Error; });
		}, 500);
	}

	private convertDecryptedMsgPlainTextToHtml(): void {
		this.secretMessageTextAsHtml = this.decryptionResult.decryptedMsg.plainText.linkify();
	}

	private setDecryptedMsgAutoclearTimeout() {
		const worker = new Worker(new URL('./message-autoclear-timeout.worker', import.meta.url));

		worker.onmessage = ({ data }) => {
			if (data.clear) {
				this.msgAutoClearTimeoutTriggered = true;

				this.decryptionResult.decryptedMsg = {} as SecretMessage;
				this.secretMessageTextAsHtml = '--- Message auto deleted ---';
			}
		};
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
