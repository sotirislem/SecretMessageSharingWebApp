import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';

import { Clipboard } from '@angular/cdk/clipboard';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap'

import { RouterHelperService } from '../../services/router-helper.service';
import { UrlHelperService } from '../../services/url-helper.service';
import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';

import { Routes } from '../../../constants';
import * as QRCode from 'qrcode';

import { SecretMessage } from '../../models/secret-message.model';
import { Subject, takeUntil } from 'rxjs';

@Component({
	templateUrl: './new-secret-message.component.html',
	styleUrls: ['./new-secret-message.component.css']
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
	qrCodeCanvas: HTMLCanvasElement;

	componentDestroyed$: Subject<boolean> = new Subject();

	constructor(
		private clipboard: Clipboard,
		routerHelperService: RouterHelperService,
		urlHelperService: UrlHelperService,
		secretMessageDeliveryNotificationHubService: SecretMessageDeliveryNotificationHubService
	) {
		this.secretMsgObj = routerHelperService.getCurrentNavigationStateData('secretMsgObj');
		this.secretMsgId = routerHelperService.getCurrentNavigationStateData('secretMsgId');
		this.secretMsgKey = routerHelperService.getCurrentNavigationStateData('secretMsgKey');

		this.secretMsgUrl = urlHelperService.createLocalUrlWithParams(Routes.GetSecretMessage, { id: this.secretMsgId }, this.secretMsgKey);

		secretMessageDeliveryNotificationHubService.receivedDeliveryNotificationsObservable$
			.pipe(takeUntil(this.componentDestroyed$))
			.subscribe((secretMessageId) => this.receivedDeliveryNotificationsObserver(secretMessageId));
	}

	ngAfterViewInit(): void {
		this.qrCodeCanvas = this.secretMsgShareElementRef.nativeElement.querySelector('#qrCodeCanvas') as HTMLCanvasElement;
		QRCode.toCanvas(this.qrCodeCanvas, this.secretMsgUrl);
	}

	ngOnDestroy(): void {
		this.componentDestroyed$.next(true);
		this.componentDestroyed$.complete();
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
		}
	}
}
