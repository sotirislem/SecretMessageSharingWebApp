import { Component, OnInit } from '@angular/core';
import { Clipboard } from '@angular/cdk/clipboard';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { RouterHelperService } from '../../services/router-helper.service';
import { Router } from '@angular/router';
import { UrlHelperService } from '../../services/url-helper.service';
import { Routes } from '../../../constants';

@Component({
	selector: 'app-save-secret-message',
	templateUrl: './save-secret-message.component.html',
	styleUrls: ['./save-secret-message.component.css']
})
export class SaveSecretMessageComponent implements OnInit {
	ngbTooltipTimer: any;
	secretMsgUrl: string;

	constructor(private router: Router, private routerHelperService: RouterHelperService, private urlHelperService: UrlHelperService, private clipboard: Clipboard) {
		const secretMsgId = this.routerHelperService.getCurrentNavigationStateData('secretMsgId');
		const secretMsgKey = this.routerHelperService.getCurrentNavigationStateData('secretMsgKey');

		this.secretMsgUrl = urlHelperService.createLocalUrlWithParams(Routes.Root_GetSecretMessage, { id: secretMsgId, encryptionKey: secretMsgKey });
	}

	ngOnInit(): void {
    }

	copySecretMsgUrlToClipboard(ngbTooltip: NgbTooltip) {
		this.clipboard.copy(this.secretMsgUrl);
		this.toggleCopiedTooltip(ngbTooltip);
	}

	redirectToFetchData() {
		window.location.href = this.secretMsgUrl;
	}

	private toggleCopiedTooltip(ngbTooltip: NgbTooltip) {
		clearTimeout(this.ngbTooltipTimer);
		ngbTooltip.open();
		this.ngbTooltipTimer = setTimeout(() => {
			ngbTooltip.close();
		}, 5000);
	}
}
