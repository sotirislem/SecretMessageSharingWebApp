import { ChangeDetectorRef, AfterViewInit, Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';

import { Clipboard } from '@angular/cdk/clipboard';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap'

import { NewSecretMessageStateService } from '../../services/new-secret-message-state.service';
import { UrlHelperService } from '../../services/url-helper.service';
import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';

import { Routes } from '../../../constants';
import * as QRCode from 'qrcode';

import { SecretMessage } from '../../models/secret-message.model';
import { Subject, takeUntil } from 'rxjs';

import { NgClass } from '@angular/common';
import { ClipboardModule } from '@angular/cdk/clipboard';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

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
	styleUrls: ['./new-secret-message.component.css'],
	standalone: true,
	imports: [NgClass, ClipboardModule, NgbModule]
})
export class NewSecretMessageComponent implements AfterViewInit, OnDestroy {
	@ViewChild('ngbTooltipElement') ngbTooltip: NgbTooltip;
	@ViewChild('secretMsgShareElementRef') secretMsgShareElementRef: ElementRef;
	ngbTooltipClearTimer: NodeJS.Timeout;

	secretMsgObj: SecretMessage;
	secretMsgId: string;
	secretMsgKey: string;
	secretMsgUrl: string;
	secretMsgDelivered: boolean;
	secretMsgContentCleared: boolean;
	qrCodeCanvas: HTMLCanvasElement;

	componentDestroyed$: Subject<boolean> = new Subject();

	private autoClearWorker: Worker | null = null;

	constructor(
		private clipboard: Clipboard,
		private cdr: ChangeDetectorRef,
		private newSecretMessageStateService: NewSecretMessageStateService,
		urlHelperService: UrlHelperService,
		secretMessageDeliveryNotificationHubService: SecretMessageDeliveryNotificationHubService
	) {
		// The route guard guarantees this state exists; it is held only in memory.
		const state = this.newSecretMessageStateService.get()!;

		this.secretMsgObj = state.secretMsgObj;
		this.secretMsgId = state.secretMsgId;
		this.secretMsgKey = state.secretMsgKey;
		this.secretMsgContentCleared = false;

		this.secretMsgUrl = urlHelperService.createLocalUrlWithParams(Routes.GetSecretMessage, { id: this.secretMsgId }, this.secretMsgKey);

		secretMessageDeliveryNotificationHubService.receivedDeliveryNotificationsObservable$
			.pipe(takeUntil(this.componentDestroyed$))
			.subscribe((secretMessageId) => this.receivedDeliveryNotificationsObserver(secretMessageId));

		this.startAutoClearTimer();
	}

	ngAfterViewInit(): void {
		this.qrCodeCanvas = this.secretMsgShareElementRef.nativeElement.querySelector('#qrCodeCanvas') as HTMLCanvasElement;
		QRCode.toCanvas(this.qrCodeCanvas, this.secretMsgUrl);
	}

	ngOnDestroy(): void {
		this.componentDestroyed$.next(true);
		this.componentDestroyed$.complete();

		clearTimeout(this.ngbTooltipClearTimer);
		this.stopAutoClearTimer();

		// Drop the plaintext, key, and URL as soon as the user leaves the page.
		this.newSecretMessageStateService.clear();
		this.secretMsgObj = null!;
		this.secretMsgId = '';
		this.secretMsgKey = '';
		this.secretMsgUrl = '';

		this.clearQrCode();
	}

	saveQrCodeToFile(): void {
		const link = document.createElement('a');
		link.href = this.qrCodeCanvas.toDataURL();
		link.download = 'QRCode.png';
		link.click();
	}

	copySecretMsgUrlToClipboard() {
		this.clipboard.copy(this.secretMsgUrl);

		clearTimeout(this.ngbTooltipClearTimer);
		if (!this.ngbTooltip.isOpen()) this.ngbTooltip.open();

		this.ngbTooltipClearTimer = setTimeout(() => {
			this.ngbTooltip?.close();
		}, 5000);
	}

	receivedDeliveryNotificationsObserver(secretMessageId: string) {
		if (secretMessageId == this.secretMsgId) {
			if (this.ngbTooltip.isOpen()) this.ngbTooltip.close();
			this.secretMsgDelivered = true;
			this.cdr.detectChanges();
		}
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

		this.secretMsgContentCleared = true;
		this.secretMsgObj = { ...EMPTY_SECRET_MESSAGE };

		// Keep the stored state free of the plaintext too, but keep the id and key
		// because the Message URL must remain usable.
		this.newSecretMessageStateService.set({
			secretMsgObj: this.secretMsgObj,
			secretMsgId: this.secretMsgId,
			secretMsgKey: this.secretMsgKey
		});

		this.cdr.detectChanges();
	}

	private clearQrCode(): void {
		if (!this.qrCodeCanvas) return;

		this.qrCodeCanvas.width = 0;
		this.qrCodeCanvas.height = 0;
	}
}
