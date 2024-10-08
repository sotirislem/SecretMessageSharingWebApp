import { Component } from '@angular/core';
import { FormBuilder, FormControl } from '@angular/forms';

import { SjclService } from '../../services/sjcl.service';
import { ApiClientService } from '../../services/api-client.service';
import { FileService } from '../../services/file.service';

import { Router } from '@angular/router';
import { Routes, Constants } from '../../../constants';

import { SecretMessage } from '../../models/secret-message.model';
import { OtpSettings, StoreNewSecretMessageRequest } from '../../models/api/store-new-secret-message-request.model';
import { finalize } from 'rxjs';

@Component({
	selector: 'create-secret-message',
	templateUrl: './create-secret-message.component.html',
	styleUrls: ['./create-secret-message.component.css']
})
export class CreateSecretMessageComponent {
	formSubmitted: boolean
	attachedFile: File | null;

	newSecretMessageForm = this.formBuilder.nonNullable.group({
		secretMsgPlainTextFormControl: [''],
		includeUsernameAndPasswordFormControl: [false],
		usernameFormControl: [{ value: '', disabled: true }],
		passwordFormControl: [{ value: '', disabled: true }],
		includeAttachedFileFormControl: [false],
		attachedFileFormControl: new FormControl({ value: '', disabled: true }),
		secretMsgRequiresOtpFormControl: [false],
		secretMsgOtpRecipientsEmailFormControl: [{ value: '', disabled: true }]
	});

	constructor(
		private formBuilder: FormBuilder,
		private router: Router,
		private sjclService: SjclService,
		private apiClientService: ApiClientService,
		private fileService: FileService
	) { }

	get secretMsgPlainTextFormControl() { return this.newSecretMessageForm.controls.secretMsgPlainTextFormControl }
	get secretMsgPlainText(): string { return this.secretMsgPlainTextFormControl.value }

	get includeUsernameAndPasswordFormControl() { return this.newSecretMessageForm.controls.includeUsernameAndPasswordFormControl }
	get includeUsernameAndPassword(): boolean { return this.includeUsernameAndPasswordFormControl.value }

	get usernameFormControl() { return this.newSecretMessageForm.controls.usernameFormControl }
	get username(): string { return this.usernameFormControl.value }

	get passwordFormControl() { return this.newSecretMessageForm.controls.passwordFormControl }
	get password(): string { return this.passwordFormControl.value }

	get includeAttachedFileFormControl() { return this.newSecretMessageForm.controls.includeAttachedFileFormControl }
	get includeAttachedFile(): boolean { return this.includeAttachedFileFormControl.value }

	get attachedFileFormControl() { return this.newSecretMessageForm.controls.attachedFileFormControl }
	get attachedFileName(): string | null { return this.attachedFileFormControl.value }

	get secretMsgRequiresOtpFormControl() { return this.newSecretMessageForm.controls.secretMsgRequiresOtpFormControl }
	get secretMsgRequiresOtp(): boolean { return this.secretMsgRequiresOtpFormControl.value }

	get secretMsgOtpRecipientsEmailFormControl() { return this.newSecretMessageForm.controls.secretMsgOtpRecipientsEmailFormControl }
	get secretMsgOtpRecipientsEmail(): string { return this.secretMsgOtpRecipientsEmailFormControl.value }

	includeUsernameAndPasswordCheckboxToggle() {
		this.usernameFormControl.setValue('');
		this.passwordFormControl.setValue('');

		if (this.includeUsernameAndPassword) {
			this.usernameFormControl.enable();
			this.passwordFormControl.enable();
		} else {
			this.usernameFormControl.disable();
			this.passwordFormControl.disable();
		}
	}

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
		const secretMsgObj = <SecretMessage>{
			plainText: this.secretMsgPlainText.trim(),
			containsFile: this.includeAttachedFile,
			fileName: this.attachedFile?.name ?? null,
			base64BlobFile: this.includeAttachedFile ? await this.fileService.readFileAsBase64Blob(this.attachedFile!) : null,
			containsUsernameAndPassword: this.includeUsernameAndPassword,
			username: this.username.trim(),
			password: this.password.trim()
		};

		setTimeout(() => {
			const encryptionResult = this.sjclService.encryptMessage(secretMsgObj);

			const request = <StoreNewSecretMessageRequest>{
				secretMessageData: encryptionResult.secretMessageData,
				otp: <OtpSettings>{
					required: this.secretMsgRequiresOtp,
					recipientsEmail: this.secretMsgOtpRecipientsEmail.trim()
				},
				encryptionKeySha256: encryptionResult.encryptionKeySha256
			};

			this.apiClientService.storeSecretMessage(request)
				.pipe(
					finalize(() => {
						this.formSubmitted = false;
					})
				)
				.subscribe(secretMsgId => {
					this.navigateToNewSecretMessagePage(secretMsgObj, secretMsgId, encryptionResult.encryptionKeyAsBase64url);
				});
		}, 250);
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
