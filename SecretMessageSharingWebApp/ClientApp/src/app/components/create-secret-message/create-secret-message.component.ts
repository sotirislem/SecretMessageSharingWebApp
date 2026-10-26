import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { FileService } from '../../services/file.service';
import { NewSecretMessageStateService } from '../../services/new-secret-message-state.service';
import { SecretMessageCreationService } from '../../services/secret-message-creation.service';
import { ToastService } from '../../services/toast.service';

import { Constants, Routes } from '../../../constants';
import { SecretMessage } from '../../models/secret-message.model';
import { OtpSettings } from '../../models/api/store-new-secret-message-request.model';

@Component({
	selector: 'create-secret-message',
	templateUrl: './create-secret-message.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [ReactiveFormsModule]
})
export class CreateSecretMessageComponent {
	private readonly formBuilder = inject(NonNullableFormBuilder);
	private readonly router = inject(Router);
	private readonly fileService = inject(FileService);
	private readonly newSecretMessageStateService = inject(NewSecretMessageStateService);
	private readonly secretMessageCreationService = inject(SecretMessageCreationService);
	private readonly toastService = inject(ToastService);

	readonly formSubmitted = signal(false);

	attachedFile: File | null = null;

	readonly newSecretMessageForm = this.formBuilder.group({
		secretMsgPlainTextFormControl: ['', Validators.required],
		includeUsernameAndPasswordFormControl: [false],
		usernameFormControl: [{ value: '', disabled: true }, Validators.required],
		passwordFormControl: [{ value: '', disabled: true }, Validators.required],
		includeAttachedFileFormControl: [false],
		attachedFileFormControl: new FormControl<string | null>({ value: null, disabled: true }, Validators.required),
		secretMsgRequiresOtpFormControl: [false],
		secretMsgOtpRecipientsEmailFormControl: [{ value: '', disabled: true }, [
			Validators.required,
			Validators.pattern(/[\w.]+@[a-zA-Z_-]+?(?:\.[a-zA-Z]{2,6})+$/)
		]],
		secretMsgNotifyOnDeliveryFormControl: [false],
		secretMsgNotificationEmailFormControl: [{ value: '', disabled: true }, [
			Validators.required,
			Validators.pattern(/[\w.]+@[a-zA-Z_-]+?(?:\.[a-zA-Z]{2,6})+$/)
		]]
	});

	get secretMsgPlainTextFormControl() { return this.newSecretMessageForm.controls.secretMsgPlainTextFormControl; }
	get secretMsgPlainText(): string { return this.secretMsgPlainTextFormControl.value; }

	get includeUsernameAndPasswordFormControl() { return this.newSecretMessageForm.controls.includeUsernameAndPasswordFormControl; }
	get includeUsernameAndPassword(): boolean { return this.includeUsernameAndPasswordFormControl.value; }

	get usernameFormControl() { return this.newSecretMessageForm.controls.usernameFormControl; }
	get username(): string { return this.usernameFormControl.value; }

	get passwordFormControl() { return this.newSecretMessageForm.controls.passwordFormControl; }
	get password(): string { return this.passwordFormControl.value; }

	get includeAttachedFileFormControl() { return this.newSecretMessageForm.controls.includeAttachedFileFormControl; }
	get includeAttachedFile(): boolean { return this.includeAttachedFileFormControl.value; }

	get attachedFileFormControl() { return this.newSecretMessageForm.controls.attachedFileFormControl; }

	get secretMsgRequiresOtpFormControl() { return this.newSecretMessageForm.controls.secretMsgRequiresOtpFormControl; }
	get secretMsgRequiresOtp(): boolean { return this.secretMsgRequiresOtpFormControl.value; }

	get secretMsgOtpRecipientsEmailFormControl() { return this.newSecretMessageForm.controls.secretMsgOtpRecipientsEmailFormControl; }
	get secretMsgOtpRecipientsEmail(): string { return this.secretMsgOtpRecipientsEmailFormControl.value; }

	get secretMsgNotifyOnDeliveryFormControl() { return this.newSecretMessageForm.controls.secretMsgNotifyOnDeliveryFormControl; }
	get secretMsgNotifyOnDelivery(): boolean { return this.secretMsgNotifyOnDeliveryFormControl.value; }

	get secretMsgNotificationEmailFormControl() { return this.newSecretMessageForm.controls.secretMsgNotificationEmailFormControl; }
	get secretMsgNotificationEmail(): string { return this.secretMsgNotificationEmailFormControl.value; }

	includeUsernameAndPasswordCheckboxToggle(): void {
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

	includeAttachedFileCheckboxToggle(): void {
		this.resetAttachedFile();

		if (this.includeAttachedFile) {
			this.attachedFileFormControl.enable();
		} else {
			this.attachedFileFormControl.disable();
		}
	}

	secretMsgRequiresOtpCheckboxToggle(): void {
		this.secretMsgOtpRecipientsEmailFormControl.setValue('');

		if (this.secretMsgRequiresOtp) {
			this.secretMsgOtpRecipientsEmailFormControl.enable();
		} else {
			this.secretMsgOtpRecipientsEmailFormControl.disable();
		}
	}

	secretMsgNotifyOnDeliveryCheckboxToggle(): void {
		this.secretMsgNotificationEmailFormControl.setValue('');

		if (this.secretMsgNotifyOnDelivery) {
			this.secretMsgNotificationEmailFormControl.enable();
		} else {
			this.secretMsgNotificationEmailFormControl.disable();
		}
	}

	submitNewSecretMessageForm(): void {
		if (this.newSecretMessageForm.invalid) return;

		this.formSubmitted.set(true);
		this.createSecretMessage();
	}

	async createSecretMessage(): Promise<void> {
		try {
			const secretMsgObj = <SecretMessage>{
				plainText: this.secretMsgPlainText.trim(),
				containsFile: this.includeAttachedFile,
				fileName: this.attachedFile?.name ?? null,
				base64BlobFile: this.includeAttachedFile && this.attachedFile
					? await this.fileService.readFileAsBase64Blob(this.attachedFile)
					: null,
				containsUsernameAndPassword: this.includeUsernameAndPassword,
				username: this.username.trim(),
				password: this.password.trim()
			};

			const otp = <OtpSettings>{
				required: this.secretMsgRequiresOtp,
				recipientsEmail: this.secretMsgOtpRecipientsEmail.trim()
			};

			const notificationEmail = this.secretMsgNotifyOnDelivery
				? this.secretMsgNotificationEmail.trim()
				: null;

			const createdSecretMessage = await this.secretMessageCreationService.create(secretMsgObj, otp, notificationEmail);
			if (!createdSecretMessage) return;

			this.navigateToNewSecretMessagePage(
				secretMsgObj,
				otp,
				notificationEmail,
				createdSecretMessage.secretMsgId,
				createdSecretMessage.secretMsgKey);
		} catch {
			// Reading the attachment failed; HTTP and encryption failures are reported
			// by the creation service and the HTTP error interceptor.
			this.toastService.error('Could not encrypt the message. Please try again.');
		} finally {
			this.formSubmitted.set(false);
		}
	}

	onFileSelection(event: Event): void {
		const fileInput = event.target as HTMLInputElement;

		if (!fileInput.files?.length) {
			this.resetAttachedFile();
			return;
		}

		const selectedFile = fileInput.files[0];
		const selectedFileSize = (selectedFile.size / 1024) / 1024;

		if (selectedFileSize > Constants.ATTACHMENT_FILESIZE_MAX_MB) {
			this.toastService.warning(`Selected file size can not exceed ${Constants.ATTACHMENT_FILESIZE_MAX_MB}MB`);

			this.resetAttachedFile();
			return;
		}

		this.attachedFile = selectedFile;
	}

	private resetAttachedFile(): void {
		this.attachedFileFormControl.setValue(null);
		this.attachedFile = null;
	}

	private navigateToNewSecretMessagePage(
		secretMsgObj: SecretMessage,
		otp: OtpSettings,
		notificationEmail: string | null,
		secretMsgId: string,
		secretMsgKey: string): void {
		// Keep the plaintext and key out of history.state. The new-message
		// component clears this in-memory state when it is destroyed.
		this.newSecretMessageStateService.set({
			secretMsgObj,
			secretMsgId,
			secretMsgKey,
			otp,
			notificationEmail
		});

		this.router.navigate([Routes.NewSecretMessage]);
	}
}
