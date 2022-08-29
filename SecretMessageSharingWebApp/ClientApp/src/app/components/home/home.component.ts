import { Component } from '@angular/core';
import { finalize } from 'rxjs/operators';

import { SjclService } from '../../services/sjcl.service';
import { ApiClientService } from '../../services/api-client.service';
import { FileService } from '../../services/file.service';
import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';

import { Router } from '@angular/router';
import { Routes, Constants } from '../../../constants';

import { SecretMessage } from '../../models/secret-message.model';

@Component({
	selector: 'app-home',
	templateUrl: './home.component.html',
	styleUrls: ['./home.component.css']
})
export class HomeComponent {
	readonly Routes = Routes;

	secretMsgPlainText: string;
	includeAttachedFile: boolean;
	attachedFile: File;
	encryptionInProgress: boolean;

	constructor(
		private router: Router,
		private sjclService: SjclService,
		private apiClientService: ApiClientService,
		private fileService: FileService,
		private secretMessageDeliveryNotificationHubService: SecretMessageDeliveryNotificationHubService
	) { }

	async onEncryptButtonClick() {
		this.encryptionInProgress = true;

		await this.secretMessageDeliveryNotificationHubService.initHubConnection();

		const secretMsgObj = <SecretMessage>{
			plainText: this.secretMsgPlainText.trim(),
			containsFile: this.includeAttachedFile,
			base64BlobFile: this.includeAttachedFile ? await this.fileService.readFileAsBase64Blob(this.attachedFile) : null,
			fileName: this.attachedFile?.name ?? null
		};

		setTimeout(() => {
			const [secretMessageData, encryptionKey] = this.sjclService.encryptMessage(secretMsgObj);

			this.apiClientService.storeSecretMessage(secretMessageData)
				.pipe(finalize(() => {
					this.encryptionInProgress = false;
				}))
				.subscribe(secretMsgId => {
					this.navigateToNewSecretMessagePage(secretMsgObj, secretMsgId, encryptionKey);
				});
		}, 500);
	}

	onFileSelection(event: Event) {
		const fileInput = event.target as HTMLInputElement;

		const resetFileInputValues = () => {
			fileInput.value = '';
			fileInput.files = null;
			this.attachedFile = new File([], '');
		}

		if (!fileInput.files?.length) {
			resetFileInputValues();
			return;
		}

		const selectedFile = fileInput.files[0];
		const selectedFileSize = ((selectedFile.size / 1024) / 1024);

		if (selectedFileSize > Constants.ATTACHMENT_FILESIZE_MAX_MB) {
			alert(`Selected file size can not exceed ${Constants.ATTACHMENT_FILESIZE_MAX_MB}MB`);

			resetFileInputValues();
			return;
		}

		this.attachedFile = selectedFile;
	}

	private navigateToNewSecretMessagePage(secretMsgObj: SecretMessage, secretMsgId: string, secretMsgKey: string) {
		this.router.navigate([Routes.NewSecretMessage], {
			state: {
				secretMsgObj: secretMsgObj,
				secretMsgId: secretMsgId,
				secretMsgKey: secretMsgKey
			}
		});
	}
}
