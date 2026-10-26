import { afterNextRender, ChangeDetectionStrategy, Component, computed, DestroyRef, effect, ElementRef, inject, Injector, signal, viewChild } from '@angular/core';
import { Clipboard } from '@angular/cdk/clipboard';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import * as QRCode from 'qrcode';

import { AutoClearService } from '../../services/auto-clear.service';
import { NewSecretMessageStateService } from '../../services/new-secret-message-state.service';
import { CreatedSecretMessage, SecretMessageCreationService } from '../../services/secret-message-creation.service';
import { ToastService } from '../../services/toast.service';
import { UrlHelperService } from '../../services/url-helper.service';
import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';
import { ModalService } from '../../services/modal.service';
import { MessageDeliveryDetailsModalComponent } from '../modals/message-delivery-details-modal/message-delivery-details-modal.component';

import { Constants, Routes } from '../../../constants';
import { SecretMessage } from '../../models/secret-message.model';

const EMPTY_SECRET_MESSAGE: SecretMessage = {
	plainText: '',
	containsFile: false,
	fileName: null,
	base64BlobFile: null,
	containsUsernameAndPassword: false,
	username: '',
	password: ''
};

@Component({
	templateUrl: './new-secret-message.component.html',
	styleUrl: './new-secret-message.component.css',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [NgbTooltip]
})
export class NewSecretMessageComponent {
	private readonly clipboard = inject(Clipboard);
	private readonly urlHelperService = inject(UrlHelperService);
	private readonly newSecretMessageStateService = inject(NewSecretMessageStateService);
	private readonly secretMessageCreationService = inject(SecretMessageCreationService);
	private readonly hubService = inject(SecretMessageDeliveryNotificationHubService);
	private readonly modalService = inject(ModalService);
	private readonly toastService = inject(ToastService);
	private readonly autoClearService = inject(AutoClearService);
	private readonly injector = inject(Injector);
	private readonly destroyRef = inject(DestroyRef);

	// The route guard guarantees this state exists; it is held only in memory.
	private readonly state = this.newSecretMessageStateService.get()!;

	readonly secretMsgObj = signal<SecretMessage>(this.state.secretMsgObj);
	readonly secretMsgId = signal(this.state.secretMsgId);
	readonly secretMsgKey = signal(this.state.secretMsgKey);
	readonly secretMsgUrl = signal(this.urlHelperService.createLocalUrlWithParams(
		Routes.GetSecretMessage,
		{ id: this.state.secretMsgId },
		this.state.secretMsgKey));
	readonly secretMsgDelivered = signal(false);
	readonly secretMsgContentCleared = signal(false);
	readonly isCreatingAnotherSecretMessage = signal(false);
	readonly otp = this.state.otp;
	readonly notificationEmail = this.state.notificationEmail;

	readonly autoDeletedText = '--- Auto deleted ---';

	readonly statusLabel = computed(() => this.secretMsgDelivered() ? 'Opened' : 'Waiting for recipient');

	/** The delivery record of the message currently shown, used by the ribbon button. */
	private readonly deliveryNotification = computed(() =>
		this.hubService.deliveryNotifications()
			.filter(notification => notification.messageId === this.secretMsgId())
			.at(-1));

	private readonly tooltip = viewChild<NgbTooltip>('ngbTooltipElement');
	private readonly qrCodeCanvas = viewChild<ElementRef<HTMLCanvasElement>>('qrCodeCanvas');

	private tooltipClearTimer: ReturnType<typeof setTimeout> | undefined;

	constructor() {
		// React to delivery notifications for this message without an explicit subscription.
		effect(() => {
			const messageId = this.secretMsgId();

			if (this.hubService.deliveredMessageIds().includes(messageId)) {
				this.tooltip()?.close();
				this.secretMsgDelivered.set(true);
			}
		});

		afterNextRender(() => this.renderQrCode());
		this.startAutoClearTimer();

		this.destroyRef.onDestroy(() => {
			this.autoClearService.stop();
			clearTimeout(this.tooltipClearTimer);

			// Drop the plaintext, key, and URL as soon as the user leaves the page.
			this.newSecretMessageStateService.clear();
			this.secretMsgObj.set(EMPTY_SECRET_MESSAGE);
			this.secretMsgId.set('');
			this.secretMsgKey.set('');
			this.secretMsgUrl.set('');

			this.clearQrCode();
		});
	}

	saveQrCodeToFile(): void {
		const canvas = this.qrCodeCanvas()?.nativeElement;
		if (!canvas) return;

		const link = document.createElement('a');
		link.href = canvas.toDataURL();
		link.download = 'QRCode.png';
		link.click();
	}

	copySecretMsgUrlToClipboard(): void {
		this.clipboard.copy(this.secretMsgUrl());

		clearTimeout(this.tooltipClearTimer);

		const tooltip = this.tooltip();
		if (tooltip && !tooltip.isOpen()) {
			tooltip.open();
		}

		this.tooltipClearTimer = setTimeout(() => this.tooltip()?.close(), 5_000);
	}

	/**
	 * Reopens the delivery details that were shown when the recipient opened the
	 * message, so the sender can inspect them again after dismissing the popup.
	 */
	showDeliveryDetails(): void {
		const deliveryNotification = this.deliveryNotification();
		if (!deliveryNotification) return;

		void this.modalService.openModal(
			MessageDeliveryDetailsModalComponent,
			{ isNotification: false, messageDeliveryDetails: deliveryNotification }
		).catch(() => {
			// The details can be dismissed with Escape or a backdrop click.
		});
	}

	/**
	 * Stores the content again under a new key and replaces the share details on this
	 * page, so the sender gets another single-use link without retyping anything.
	 */
	async createAnotherSecretMessage(): Promise<void> {
		if (this.isCreatingAnotherSecretMessage() || this.secretMsgContentCleared()) return;

		this.isCreatingAnotherSecretMessage.set(true);

		try {
			const createdSecretMessage = await this.secretMessageCreationService.create(
				this.secretMsgObj(),
				this.state.otp,
				this.notificationEmail);

			if (!createdSecretMessage) return;

			this.applyCreatedSecretMessage(createdSecretMessage);

			this.toastService.success(
				'A new single-use link was created from the same content.',
				'New message created');
		} finally {
			this.isCreatingAnotherSecretMessage.set(false);
		}
	}

	private applyCreatedSecretMessage(createdSecretMessage: CreatedSecretMessage): void {
		this.tooltip()?.close();
		this.secretMsgDelivered.set(false);

		this.secretMsgId.set(createdSecretMessage.secretMsgId);
		this.secretMsgKey.set(createdSecretMessage.secretMsgKey);
		this.secretMsgUrl.set(this.urlHelperService.createLocalUrlWithParams(
			Routes.GetSecretMessage,
			{ id: createdSecretMessage.secretMsgId },
			createdSecretMessage.secretMsgKey));

		// A refresh of this page has to show the message that is now being shared.
		this.newSecretMessageStateService.set({
			secretMsgObj: this.secretMsgObj(),
			secretMsgId: createdSecretMessage.secretMsgId,
			secretMsgKey: createdSecretMessage.secretMsgKey,
			otp: this.state.otp,
			notificationEmail: this.notificationEmail
		});

		// The new message gets its own full window before the local content is cleared again.
		this.startAutoClearTimer();

		// The QR canvas is re-created by the view after a delivered message becomes pending
		// again, so the code can only be drawn once the next render has happened.
		afterNextRender(() => this.renderQrCode(), { injector: this.injector });
	}

	private renderQrCode(): void {
		const canvas = this.qrCodeCanvas()?.nativeElement;
		if (!canvas) return;

		void QRCode.toCanvas(canvas, this.secretMsgUrl());
	}

	/**
	 * The page shows the plaintext and credentials while it waits for the delivery
	 * notification. If it is left open and forgotten, clear only that local content.
	 * The message stays on the server and the Message URL / QRCode stay visible, so
	 * the sender can still share it.
	 */
	private startAutoClearTimer(): void {
		this.autoClearService.start(
			Constants.NEW_SECRET_MESSAGE_AUTOCLEAR_INTERVAL_MINUTES,
			() => this.clearSecretMessageContent());
	}

	private clearSecretMessageContent(): void {
		this.secretMsgContentCleared.set(true);

		const emptySecretMessage = {
			...EMPTY_SECRET_MESSAGE,
			// Keep the layout of the content fields so the boxes remain visible with a
			// placeholder instead of disappearing when the content is cleared.
			containsFile: this.secretMsgObj().containsFile,
			containsUsernameAndPassword: this.secretMsgObj().containsUsernameAndPassword
		};
		this.secretMsgObj.set(emptySecretMessage);

		// Keep the stored state free of the plaintext too, but keep the id and key
		// because the Message URL must remain usable.
		this.newSecretMessageStateService.set({
			secretMsgObj: emptySecretMessage,
			secretMsgId: this.secretMsgId(),
			secretMsgKey: this.secretMsgKey(),
			otp: this.state.otp,
			notificationEmail: this.notificationEmail
		});
	}

	private clearQrCode(): void {
		const canvas = this.qrCodeCanvas()?.nativeElement;
		if (!canvas) return;

		canvas.width = 0;
		canvas.height = 0;
	}
}
