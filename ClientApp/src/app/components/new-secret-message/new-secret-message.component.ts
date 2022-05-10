import { Component, OnInit, ViewChild } from '@angular/core';
import { Clipboard } from '@angular/cdk/clipboard';
import { NgbModalRef, NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { RouterHelperService } from '../../services/router-helper.service';
import { Router } from '@angular/router';
import { UrlHelperService } from '../../services/url-helper.service';
import { Routes } from '../../../constants';
import * as QRCode from 'qrcode'
import { SignalRService } from '../../services/signalr.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ElementRef } from '@angular/core';
import { MessageDeliveryNotification } from '../../models/message-delivery-notification.model';

@Component({
	selector: 'app-new-secret-message',
	templateUrl: './new-secret-message.component.html',
	styleUrls: ['./new-secret-message.component.css']
})
export class NewSecretMessageComponent implements OnInit {
	@ViewChild('msgDeliveryNotificationModalElement') msgDeliveryNotificationModal: NgbModalRef;
	@ViewChild('ngbTooltipElement') ngbTooltip: NgbTooltip;

	ngbTooltipTimer: any;
	secretMsgPlainText: string;
	secretMsgId: string;
	secretMsgKey: string;
	secretMsgUrl: string;
	qrCodeCanvas: HTMLCanvasElement;
	messageDeliveryNotificationData: MessageDeliveryNotification;

	constructor(
		private clipboard: Clipboard,
		routerHelperService: RouterHelperService,
		urlHelperService: UrlHelperService,
		signalRService: SignalRService,
		modalService: NgbModal
	) {
		this.secretMsgPlainText = routerHelperService.getCurrentNavigationStateData('secretMsgPlainText');
		this.secretMsgId = routerHelperService.getCurrentNavigationStateData('secretMsgId');
		this.secretMsgKey = routerHelperService.getCurrentNavigationStateData('secretMsgKey');
		this.secretMsgUrl = urlHelperService.createLocalUrlWithParams(Routes.Root_GetSecretMessage, { id: this.secretMsgId }, this.secretMsgKey);

		signalRService.registerNewSecretMessageDeliveryNotificationHandler((data: MessageDeliveryNotification) => {
			this.ngbTooltip.close();

			this.messageDeliveryNotificationData = data;
			modalService.open(this.msgDeliveryNotificationModal, { centered: true });
		});
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

		clearTimeout(this.ngbTooltipTimer);
		this.ngbTooltip.open();
		this.ngbTooltipTimer = setTimeout(() => {
			this.ngbTooltip.close();
		}, 5000);
	}
}
