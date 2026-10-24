import { ChangeDetectorRef, Component, DestroyRef, OnDestroy, TemplateRef, ViewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NgClass } from '@angular/common';
import { RouterModule } from '@angular/router';
import { IndividualConfig, ToastrService } from 'ngx-toastr';
import { ActivatedRoute } from '@angular/router';
import { NgbModalOptions, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { Clipboard, ClipboardModule } from '@angular/cdk/clipboard';

import { GetSecretMessageResponse } from '../../models/api/get-secret-message-response.model';
import { DecryptionResult, DecryptionStatus } from '../../models/decryption-result.model';
import { VerifySecretMessageResponse } from '../../models/api/verify-secret-message-response.model';

import { ApiClientService } from '../../services/api-client.service';
import { CryptoService } from '../../services/crypto.service';
import { FileService } from '../../services/file.service';
import { ModalService } from '../../services/modal.service';
import { OtpInputModalComponent } from '../modals/otp-input-modal/otp-input-modal.component';
import { RouterHelperService } from '../../services/router-helper.service';

enum ComponentState {
	LoadingMessage,
	ReadyNoMessage,
	ReadyWithMessage,
	MissingEncryptionKey,
	Aborted,
	Error
}

@Component({
	templateUrl: './get-secret-message.component.html',
	styleUrls: ['./get-secret-message.component.css'],
	standalone: true,
	imports: [NgClass, RouterModule, NgbModule, ClipboardModule]
})
export class GetSecretMessageComponent implements OnDestroy {
	@ViewChild('getSecretMessageConfirmationModalBody') getSecretMessageConfirmationModalBody: TemplateRef<any>;

	readonly ComponentState = ComponentState;
	readonly DecryptionStatus = DecryptionStatus;

	componentState: ComponentState;

	messageId: string;
	decryptionResult: DecryptionResult;
	msgAutoClearTimeoutTriggered: boolean;
	secretMessageTextAsHtml: string;

	private autoClearWorker: Worker | null = null;

	constructor(
		route: ActivatedRoute,
		private routerHelperService: RouterHelperService,
		private apiClientService: ApiClientService,
		private cryptoService: CryptoService,
		private toastrService: ToastrService,
		private fileService: FileService,
		private clipboard: Clipboard,
		private modalService: ModalService,
		private cdr: ChangeDetectorRef,
		private destroyRef: DestroyRef
	) {
		this.messageId = route.snapshot.queryParams.id;
		const encryptionKey = route.snapshot.fragment ?? '';

		this.componentState = ComponentState.LoadingMessage;

		apiClientService.verifySecretMessage(this.messageId)
			.pipe(takeUntilDestroyed(this.destroyRef))
			.subscribe((response: VerifySecretMessageResponse) => {
				if (!response.exists) {
					this.removeEncryptionKeyFromAddressBar();
					this.componentState = ComponentState.ReadyNoMessage;
					this.cdr.detectChanges();
					return;
				}

				// The route guard allows an id without a fragment so that an already-used
				// link can still show the "no message found" state. An existing message
				// without its key cannot be retrieved, so explain why instead of opening
				// a confirmation modal that could only fail.
				if (!encryptionKey) {
					this.componentState = ComponentState.MissingEncryptionKey;
					this.cdr.detectChanges();
					return;
				}

				this.tryGetSecretMessage(encryptionKey, response.requiresOtp!);
			}, (error) => {
				this.componentState = ComponentState.Error;
				this.cdr.detectChanges();
			});
	}

	ngOnDestroy(): void {
		this.autoClearWorker?.terminate();
		this.autoClearWorker = null;

		// Do not leave plaintext or key material reachable through the destroyed component.
		this.decryptionResult = null!;
		this.secretMessageTextAsHtml = '';
	}

	reload() {
		this.routerHelperService.reloadCurrentPage();
	}

	downloadAttachment(): void {
		if (this.decryptionResult.decryptedMsg.containsFile) {
			const base64Blob = this.decryptionResult.decryptedMsg.base64BlobFile;
			const saveFileName = this.decryptionResult.decryptedMsg.fileName;

			if (!base64Blob || !saveFileName) return;

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
				this.cdr.detectChanges();
				return;
			}

			if (requiresOtp) {
				this.openOtpInputModal(encryptionKey);
			} else {
				this.getSecretMessage(encryptionKey);
			}
		}).catch(() => {
			this.componentState = ComponentState.Aborted;
			this.cdr.detectChanges();
		});
	}

	private openOtpInputModal(encryptionKey: string) {
		this.modalService.openModal<string>(
			OtpInputModalComponent,
			{ messageId: this.messageId },
			<NgbModalOptions>{ centered: true, backdrop: 'static' }
		).then((authToken?: string) => {
			if (authToken) {
				this.getSecretMessage(encryptionKey, authToken);
			} else {
				this.componentState = ComponentState.Aborted;
				this.cdr.detectChanges();
			}
		}).catch(() => {
			this.componentState = ComponentState.Aborted;
			this.cdr.detectChanges();
		});
	}

	private async getSecretMessage(encryptionKey: string, authToken?: string) {
		try {
			const encryptionKeySha256 = await this.cryptoService.getSha256(encryptionKey);

			// The user may have navigated away while the OTP modal or hashing was pending.
			// Do not consume the one-time message after the component is gone.
			if (this.destroyRef.destroyed) return;

			this.apiClientService.getSecretMessage(this.messageId, encryptionKeySha256, authToken)
				.pipe(takeUntilDestroyed(this.destroyRef))
				.subscribe({
					next: async (response: GetSecretMessageResponse) => {
						// The server deletes the message before returning it, so the key
						// in the fragment is no longer useful and should not stay in history.
						this.removeEncryptionKeyFromAddressBar();

						const decryptionResult = await this.cryptoService.decryptMessage(response.data, encryptionKey);

						// Decryption is asynchronous; the user may have navigated away meanwhile.
						// Touching the destroyed view or starting a worker would leak/throw.
						if (this.destroyRef.destroyed) return;

						this.componentState = ComponentState.ReadyWithMessage;
						this.decryptionResult = decryptionResult;

						if (this.decryptionResult.status === DecryptionStatus.OK) {
							this.convertDecryptedMsgPlainTextToHtml();
							this.setDecryptedMsgAutoclearTimeout();
						}

						this.displayDeliveryNotificationSentToast(response.deliveryNotificationSent);
						this.cdr.detectChanges();
					},
					error: () => {
						this.componentState = ComponentState.Error;
						this.cdr.detectChanges();
					}
				});
		} catch {
			this.componentState = ComponentState.Error;
			this.cdr.detectChanges();
		}
	}

	private convertDecryptedMsgPlainTextToHtml(): void {
		this.secretMessageTextAsHtml = this.decryptionResult.decryptedMsg.plainText.linkify();
	}

	private setDecryptedMsgAutoclearTimeout() {
		this.autoClearWorker?.terminate();
		this.autoClearWorker = new Worker(new URL('../../workers/auto-clear-timeout.worker', import.meta.url));

		this.autoClearWorker.onmessage = ({ data }) => {
			if (data.clear) {
				this.msgAutoClearTimeoutTriggered = true;

				this.decryptionResult.decryptedMsg = {
					plainText: '',
					containsFile: false,
					fileName: '',
					base64BlobFile: '',
					containsUsernameAndPassword: false,
					username: '',
					password: ''
				};
				this.secretMessageTextAsHtml = '--- Message auto deleted ---';
				this.cdr.detectChanges();

				this.autoClearWorker?.terminate();
				this.autoClearWorker = null;
			}
		};
	}

	private removeEncryptionKeyFromAddressBar(): void {
		if (!window.location.hash) return;

		// Preserve Angular's history state so router navigation continues to work.
		const urlWithoutFragment = window.location.pathname + window.location.search;
		window.history.replaceState(window.history.state, '', urlWithoutFragment);
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
