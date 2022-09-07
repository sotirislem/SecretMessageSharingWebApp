import { Component } from '@angular/core';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';

import { SjclService } from '../../services/sjcl.service';
import { ApiClientService } from '../../services/api-client.service';
import { FileService } from '../../services/file.service';
import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';

import { Router } from '@angular/router';
import { Routes, Constants } from '../../../constants';

import { SecretMessage } from '../../models/secret-message.model';
import { OtpSettings, StoreNewSecretMessageRequest } from '../../models/api/store-new-secret-message-request.model';

@Component({
	selector: 'app-home',
	templateUrl: './home.component.html',
	styleUrls: ['./home.component.css']
})
export class HomeComponent {
	readonly Routes = Routes;

	formSubmitted: boolean
	attachedFile: File | null;

	newSecretMessageForm = this.formBuilder.group({
		secretMsgPlainTextFormControl: [''],
		includeAttachedFileFormControl: [false],
		attachedFileFormControl: [{ value: null, disabled: true }],
		secretMsgRequiresOtpFormControl: [false],
		secretMsgOtpRecipientsEmailFormControl: [{ value: '', disabled: true }]
	});

	constructor(
		private formBuilder: FormBuilder,
		private router: Router,
		private sjclService: SjclService,
		private apiClientService: ApiClientService,
		private fileService: FileService,
		private secretMessageDeliveryNotificationHubService: SecretMessageDeliveryNotificationHubService
	) { }

	get secretMsgPlainTextFormControl() { return this.newSecretMessageForm.controls.secretMsgPlainTextFormControl }
	get secretMsgPlainText(): string { return this.secretMsgPlainTextFormControl.value }
	get includeAttachedFileFormControl() { return this.newSecretMessageForm.controls.includeAttachedFileFormControl }
	get includeAttachedFile(): boolean { return this.includeAttachedFileFormControl.value }
	get attachedFileFormControl() { return this.newSecretMessageForm.controls.attachedFileFormControl }
	get attachedFileName(): string { return this.attachedFileFormControl.value }
	get secretMsgRequiresOtpFormControl() { return this.newSecretMessageForm.controls.secretMsgRequiresOtpFormControl }
	get secretMsgRequiresOtp(): boolean { return this.secretMsgRequiresOtpFormControl.value }
	get secretMsgOtpRecipientsEmailFormControl() { return this.newSecretMessageForm.controls.secretMsgOtpRecipientsEmailFormControl }
	get secretMsgOtpRecipientsEmail(): string { return this.secretMsgOtpRecipientsEmailFormControl.value }

	includeAttachedFileCheckboxToggle() {
		this.resetAttachedFile();

		if (this.includeAttachedFile)
			this.attachedFileFormControl.enable();
		else
			this.attachedFileFormControl.disable();
	}

	secretMsgRequiresOtpCheckboxToggle() {
		this.secretMsgOtpRecipientsEmailFormControl.setValue('');

		if (this.secretMsgRequiresOtp)
			this.secretMsgOtpRecipientsEmailFormControl.enable();
		else
			this.secretMsgOtpRecipientsEmailFormControl.disable();
	}

	submitNewSecretMessageForm() {
		if (this.newSecretMessageForm.invalid) return;

		this.formSubmitted = true;

		this.createSecretMessage();
	}

	async createSecretMessage() {
		await this.secretMessageDeliveryNotificationHubService.initHubConnection();

		const secretMsgObj = <SecretMessage>{
			plainText: this.secretMsgPlainText.trim(),
			containsFile: this.includeAttachedFile,
			base64BlobFile: this.includeAttachedFile ? await this.fileService.readFileAsBase64Blob(this.attachedFile!) : null,
			fileName: this.attachedFile?.name ?? null
		};

		setTimeout(() => {
			const [secretMessageData, encryptionKey] = this.sjclService.encryptMessage(secretMsgObj);

			const request = <StoreNewSecretMessageRequest>{
				secretMessageData: secretMessageData,
				otp: <OtpSettings>{
					required: this.secretMsgRequiresOtp,
					recipientsEmail: this.secretMsgOtpRecipientsEmail.trim()
				}
			};
			this.apiClientService.storeSecretMessage(request).subscribe(secretMsgId => {
				this.navigateToNewSecretMessagePage(secretMsgObj, secretMsgId, encryptionKey);
			});
		}, 500);
	}

	onFileSelection(event: Event) {
		const fileInput = event.target as HTMLInputElement;

		if (!fileInput.files?.length) {
			this.resetAttachedFile();
			return;
		}

		const selectedFile = fileInput.files[0];
		const selectedFileSize = ((selectedFile.size / 1024) / 1024);

		if (selectedFileSize > Constants.ATTACHMENT_FILESIZE_MAX_MB) {
			alert(`Selected file size can not exceed ${Constants.ATTACHMENT_FILESIZE_MAX_MB}MB`);

			this.resetAttachedFile();
			return;
		}

		this.attachedFile = selectedFile;
	}

	private resetAttachedFile() {
		this.attachedFileFormControl.setValue(null);
		this.attachedFile = null;
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
