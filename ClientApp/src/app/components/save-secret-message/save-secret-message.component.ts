import { Component, OnInit } from '@angular/core';
import { Clipboard } from '@angular/cdk/clipboard';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { RouterHelperService } from '../../services/router-helper.service';
import { Router } from '@angular/router';
import { UrlHelperService } from '../../services/url-helper.service';
import { Routes } from '../../../constants';
import * as QRCode from 'qrcode'

@Component({
	selector: 'app-save-secret-message',
	templateUrl: './save-secret-message.component.html',
	styleUrls: ['./save-secret-message.component.css']
})
export class SaveSecretMessageComponent implements OnInit {
	ngbTooltipTimer: any;
	secretMsgPlainText: string;
	secretMsgId: string;
	secretMsgKey: string;
	secretMsgUrl: string;
	qrCodeCanvas: HTMLCanvasElement;

	constructor(private router: Router, private routerHelperService: RouterHelperService, private urlHelperService: UrlHelperService, private clipboard: Clipboard) {
		this.secretMsgPlainText = this.routerHelperService.getCurrentNavigationStateData('secretMsgPlainText');
		this.secretMsgId = this.routerHelperService.getCurrentNavigationStateData('secretMsgId');
		this.secretMsgKey = this.routerHelperService.getCurrentNavigationStateData('secretMsgKey');

		this.secretMsgUrl = urlHelperService.createLocalUrlWithParams(Routes.Root_GetSecretMessage, { id: this.secretMsgId }, this.secretMsgKey);
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

	copySecretMsgUrlToClipboard(ngbTooltip: NgbTooltip) {
		this.clipboard.copy(this.secretMsgUrl);
		this.toggleCopiedTooltip(ngbTooltip);
	}

	private toggleCopiedTooltip(ngbTooltip: NgbTooltip) {
		clearTimeout(this.ngbTooltipTimer);
		ngbTooltip.open();
		this.ngbTooltipTimer = setTimeout(() => {
			ngbTooltip.close();
		}, 5000);
	}
}
