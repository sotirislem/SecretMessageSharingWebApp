import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Clipboard } from '@angular/cdk/clipboard';
import { NgbModalOptions, NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { firstValueFrom } from 'rxjs';

import { GetSecretMessageResponse } from '../../models/api/get-secret-message-response.model';
import { DecryptionResult, DecryptionStatus } from '../../models/decryption-result.model';
import { SecretMessage } from '../../models/secret-message.model';

import { ApiClientService } from '../../services/api-client.service';
import { CryptoService } from '../../services/crypto.service';
import { FileService } from '../../services/file.service';
import { ModalService } from '../../services/modal.service';
import { ToastService } from '../../services/toast.service';
import { OtpInputModalComponent, OtpInputModalData } from '../modals/otp-input-modal/otp-input-modal.component';

enum ComponentState {
	LoadingMessage,
	ReadyNoMessage,
	ReadyWithMessage,
	MissingEncryptionKey,
	Aborted,
	Error
}

const EMPTY_SECRET_MESSAGE: SecretMessage = {
	plainText: '',
	containsFile: false,
	fileName: '',
	base64BlobFile: '',
	containsUsernameAndPassword: false,
	username: '',
	password: ''
};

@Component({
	templateUrl: './get-secret-message.component.html',
	styleUrl: './get-secret-message.component.css',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [NgbTooltip]
})
export class GetSecretMessageComponent {
	private readonly route = inject(ActivatedRoute);
	private readonly apiClientService = inject(ApiClientService);
	private readonly cryptoService = inject(CryptoService);
	private readonly toastService = inject(ToastService);
	private readonly fileService = inject(FileService);
	private readonly clipboard = inject(Clipboard);
	private readonly modalService = inject(ModalService);
	private readonly destroyRef = inject(DestroyRef);

	private readonly confirmationModalBody = viewChild.required<TemplateRef<unknown>>('getSecretMessageConfirmationModalBody');

	// The fragment is only removed from the address bar after a successful
	// retrieval, so it is still available when the user retries.
	private readonly encryptionKey = this.route.snapshot.fragment ?? '';

	readonly ComponentState = ComponentState;
	readonly DecryptionStatus = DecryptionStatus;

	readonly componentState = signal(ComponentState.LoadingMessage);
	readonly messageId = signal('');
	readonly decryptionResult = signal<DecryptionResult | null>(null);
	readonly msgAutoClearTimeoutTriggered = signal(false);
	readonly secretMessageTextAsHtml = signal('');

	private autoClearWorker: Worker | null = null;

	constructor() {
		this.messageId.set(this.route.snapshot.queryParams['id'] ?? '');

		void this.verifyMessage();

		this.destroyRef.onDestroy(() => {
			this.autoClearWorker?.terminate();
			this.autoClearWorker = null;

			// Do not leave plaintext or key material reachable through the destroyed component.
			this.decryptionResult.set(null);
			this.secretMessageTextAsHtml.set('');
		});
	}

	/**
	 * Retries the retrieval without reloading the route. The route guard has
	 * already accepted this URL and the encryption key is still in the fragment,
	 * so only the verification/confirmation flow has to run again.
	 */
	retry(): void {
		this.componentState.set(ComponentState.LoadingMessage);
		void this.verifyMessage();
	}

	downloadAttachment(): void {
		const decryptedMsg = this.decryptionResult()?.decryptedMsg;
		if (!decryptedMsg?.containsFile) return;

		const base64Blob = decryptedMsg.base64BlobFile;
		const saveFileName = decryptedMsg.fileName;

		if (!base64Blob || !saveFileName) return;

		this.fileService.saveBase64BlobAsFile(base64Blob, saveFileName);
	}

	copyUsernameToClipboard(): void {
		if (this.msgAutoClearTimeoutTriggered()) return;

		const username = this.decryptionResult()?.decryptedMsg.username;
		if (username) {
			this.clipboard.copy(username);
		}
	}

	copyPasswordToClipboard(): void {
		if (this.msgAutoClearTimeoutTriggered()) return;

		const password = this.decryptionResult()?.decryptedMsg.password;
		if (password) {
			this.clipboard.copy(password);
		}
	}

	copyMessageContentToClipboard(): void {
		if (this.msgAutoClearTimeoutTriggered()) return;

		const decryptedMsg = this.decryptionResult()?.decryptedMsg;
		if (!decryptedMsg) return;

		let msgContent = decryptedMsg.plainText;
		if (decryptedMsg.containsUsernameAndPassword) {
			msgContent += `\r\nUsername: ${decryptedMsg.username}`;
			msgContent += `\r\nPassword: ${decryptedMsg.password}`;
		}

		this.clipboard.copy(msgContent);
	}

	private async verifyMessage(): Promise<void> {
		try {
			const response = await firstValueFrom(this.apiClientService.verifySecretMessage(this.messageId()));

			if (!response.exists) {
				this.removeEncryptionKeyFromAddressBar();
				this.componentState.set(ComponentState.ReadyNoMessage);
				return;
			}

			// The route guard allows an id without a fragment so that an already-used
			// link can still show the "no message found" state. An existing message
			// without its key cannot be retrieved, so explain why instead of opening
			// a confirmation modal that could only fail.
			if (!this.encryptionKey) {
				this.componentState.set(ComponentState.MissingEncryptionKey);
				return;
			}

			await this.tryGetSecretMessage(response.requiresOtp ?? false);
		} catch {
			if (!this.destroyRef.destroyed) {
				this.componentState.set(ComponentState.Error);
			}
		}
	}

	private async tryGetSecretMessage(requiresOtp: boolean): Promise<void> {
		try {
			const confirmed = await this.modalService.openConfirmationModal(
				this.confirmationModalBody(),
				'Message Retrieve Confirmation');

			if (this.destroyRef.destroyed) return;

			if (!confirmed) {
				this.componentState.set(ComponentState.Aborted);
				return;
			}

			if (requiresOtp) {
				const authToken = await this.modalService.openModal<string, OtpInputModalData>(
					OtpInputModalComponent,
					{ messageId: this.messageId() },
					<NgbModalOptions>{ centered: true, backdrop: 'static' });

				if (this.destroyRef.destroyed) return;

				if (!authToken) {
					this.componentState.set(ComponentState.Aborted);
					return;
				}

				await this.getSecretMessage(authToken);
			} else {
				await this.getSecretMessage();
			}
		} catch {
			if (!this.destroyRef.destroyed) {
				this.componentState.set(ComponentState.Aborted);
			}
		}
	}

	private async getSecretMessage(authToken?: string): Promise<void> {
		try {
			const encryptionKeySha256 = await this.cryptoService.getSha256(this.encryptionKey);

			// The user may have navigated away while the OTP modal or hashing was pending.
			// Do not consume the one-time message after the component is gone.
			if (this.destroyRef.destroyed) return;

			const response: GetSecretMessageResponse = await firstValueFrom(
				this.apiClientService.getSecretMessage(this.messageId(), encryptionKeySha256, authToken));

			// The server deletes the message before returning it, so the key
			// in the fragment is no longer useful and should not stay in history.
			this.removeEncryptionKeyFromAddressBar();

			const decryptionResult = await this.cryptoService.decryptMessage(response.data, this.encryptionKey);

			// Decryption is asynchronous; the user may have navigated away meanwhile.
			// Touching the destroyed view or starting a worker would leak/throw.
			if (this.destroyRef.destroyed) return;

			this.componentState.set(ComponentState.ReadyWithMessage);
			this.decryptionResult.set(decryptionResult);

			if (decryptionResult.status === DecryptionStatus.OK) {
				this.secretMessageTextAsHtml.set(decryptionResult.decryptedMsg.plainText.linkify());
				this.setDecryptedMsgAutoclearTimeout();
			}

			this.displayDeliveryNotificationSentToast(response.deliveryNotificationSent);
		} catch {
			if (!this.destroyRef.destroyed) {
				this.componentState.set(ComponentState.Error);
			}
		}
	}

	private setDecryptedMsgAutoclearTimeout(): void {
		this.autoClearWorker?.terminate();
		this.autoClearWorker = new Worker(new URL('../../workers/auto-clear-timeout.worker', import.meta.url));

		this.autoClearWorker.onmessage = ({ data }) => {
			if (!data.clear) return;

			this.msgAutoClearTimeoutTriggered.set(true);

			this.decryptionResult.update(result => result
				? { ...result, decryptedMsg: { ...EMPTY_SECRET_MESSAGE } }
				: result);
			this.secretMessageTextAsHtml.set('--- Message auto deleted ---');

			this.autoClearWorker?.terminate();
			this.autoClearWorker = null;
		};
	}

	private removeEncryptionKeyFromAddressBar(): void {
		if (!window.location.hash) return;

		// Preserve Angular's history state so router navigation continues to work.
		const urlWithoutFragment = window.location.pathname + window.location.search;
		window.history.replaceState(window.history.state, '', urlWithoutFragment);
	}

	private displayDeliveryNotificationSentToast(deliveryNotificationSent: boolean): void {
		if (deliveryNotificationSent) {
			this.toastService.success('Delivery notification was sent', '', { timeOut: 7_000 });
		} else {
			this.toastService.warning('Delivery notification could not be sent', '', { timeOut: 7_000 });
		}
	}
}
