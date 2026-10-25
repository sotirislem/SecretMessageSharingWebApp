import { afterNextRender, ChangeDetectionStrategy, Component, computed, DestroyRef, effect, ElementRef, inject, signal, viewChild } from '@angular/core';
import { Clipboard } from '@angular/cdk/clipboard';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import * as QRCode from 'qrcode';

import { NewSecretMessageStateService } from '../../services/new-secret-message-state.service';
import { UrlHelperService } from '../../services/url-helper.service';
import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';

import { Routes } from '../../../constants';
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
	private readonly hubService = inject(SecretMessageDeliveryNotificationHubService);
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

	readonly statusLabel = computed(() => this.secretMsgDelivered() ? 'Opened' : 'Waiting for recipient');

	private readonly tooltip = viewChild<NgbTooltip>('ngbTooltipElement');
	private readonly qrCodeCanvas = viewChild<ElementRef<HTMLCanvasElement>>('qrCodeCanvas');

	private autoClearWorker: Worker | null = null;
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
			this.stopAutoClearTimer();
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
		this.autoClearWorker = new Worker(new URL('../../workers/auto-clear-timeout.worker', import.meta.url));

		this.autoClearWorker.onmessage = ({ data }) => {
			if (data.clear) {
				this.clearSecretMessageContent();
			}
		};
	}

	private stopAutoClearTimer(): void {
		this.autoClearWorker?.terminate();
		this.autoClearWorker = null;
	}

	private clearSecretMessageContent(): void {
		this.stopAutoClearTimer();

		this.secretMsgContentCleared.set(true);

		const emptySecretMessage = { ...EMPTY_SECRET_MESSAGE };
		this.secretMsgObj.set(emptySecretMessage);

		// Keep the stored state free of the plaintext too, but keep the id and key
		// because the Message URL must remain usable.
		this.newSecretMessageStateService.set({
			secretMsgObj: emptySecretMessage,
			secretMsgId: this.secretMsgId(),
			secretMsgKey: this.secretMsgKey()
		});
	}

	private clearQrCode(): void {
		const canvas = this.qrCodeCanvas()?.nativeElement;
		if (!canvas) return;

		canvas.width = 0;
		canvas.height = 0;
	}
}
