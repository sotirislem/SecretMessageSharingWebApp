import { Component, OnInit, ViewChild } from '@angular/core';

import { Clipboard } from '@angular/cdk/clipboard';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap'

import { RouterHelperService } from '../../services/router-helper.service';
import { UrlHelperService } from '../../services/url-helper.service';
import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';

import { Routes } from '../../../constants';
import * as QRCode from 'qrcode';

import { SecretMessage } from '../../models/secret-message.model';

@Component({
	selector: 'app-new-secret-message',
	templateUrl: './new-secret-message.component.html',
	styleUrls: ['./new-secret-message.component.css']
})
export class NewSecretMessageComponent implements OnInit {
	@ViewChild('ngbTooltipElement') ngbTooltip: NgbTooltip;
	ngbTooltipClearTimer: NodeJS.Timeout;

	secretMsgPlainText: string;
	secretMsgContainsFile: boolean;
	secretMsgAttachedFileName: string;
	secretMsgId: string;
	secretMsgKey: string;
	secretMsgUrl: string;
	secretMsgDelivered: boolean;
	qrCodeCanvas: HTMLCanvasElement;

	constructor(
		private clipboard: Clipboard,
		routerHelperService: RouterHelperService,
		urlHelperService: UrlHelperService,
		secretMessageDeliveryNotificationHubService: SecretMessageDeliveryNotificationHubService
	) {
		const secretMsgObj: SecretMessage = routerHelperService.getCurrentNavigationStateData('secretMsgObj');

		this.secretMsgPlainText = secretMsgObj.plainText;
		this.secretMsgContainsFile = secretMsgObj.containsFile;
		this.secretMsgAttachedFileName = secretMsgObj.fileName;
		this.secretMsgId = routerHelperService.getCurrentNavigationStateData('secretMsgId');
		this.secretMsgKey = routerHelperService.getCurrentNavigationStateData('secretMsgKey');
		this.secretMsgUrl = urlHelperService.createLocalUrlWithParams(Routes.GetSecretMessage, { id: this.secretMsgId }, this.secretMsgKey);

		secretMessageDeliveryNotificationHubService.receivedDeliveryNotificationsObservable.subscribe(
			(secretMessageId) => this.receivedDeliveryNotificationsObserver(secretMessageId)
		);
	}

	ngOnInit(): void {
		this.qrCodeCanvas = document.getElementById('qrCodeCanvas') as HTMLCanvasElement;
		QRCode.toCanvas(this.qrCodeCanvas, this.secretMsgUrl);
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
			this.ngbTooltip.close();
		}, 5000);
	}

	receivedDeliveryNotificationsObserver(secretMessageId: string) {
		if (secretMessageId == this.secretMsgId) {
			if (this.ngbTooltip.isOpen()) this.ngbTooltip.close();
			this.secretMsgDelivered = true;
		}
	}
}
