import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { Constants, Routes } from '../../../constants';
import { EncryptionResult } from '../../models/encryption-result.model';
import { ApiClientService } from '../../services/api-client.service';
import { CryptoService } from '../../services/crypto.service';
import { FileService } from '../../services/file.service';
import { NewSecretMessageStateService } from '../../services/new-secret-message-state.service';
import { ToastService } from '../../services/toast.service';
import { CreateSecretMessageComponent } from './create-secret-message.component';

describe('CreateSecretMessageComponent', () => {
	let router: jasmine.SpyObj<Router>;
	let cryptoService: jasmine.SpyObj<CryptoService>;
	let apiClient: jasmine.SpyObj<ApiClientService>;
	let fileService: jasmine.SpyObj<FileService>;
	let toastService: jasmine.SpyObj<ToastService>;
	let stateService: NewSecretMessageStateService;
	let fixture: ReturnType<typeof TestBed.createComponent<CreateSecretMessageComponent>>;
	let component: CreateSecretMessageComponent;

	function encryptionResult(ct = 'ciphertext'): EncryptionResult {
		return {
			secretMessageData: { iv: 'iv', ct },
			encryptionKeyAsBase64Url: 'encryption-key',
			encryptionKeySha256: 'key-hash'
		};
	}

	beforeEach(async () => {
		router = jasmine.createSpyObj<Router>('Router', ['navigate']);
		router.navigate.and.returnValue(Promise.resolve(true));

		cryptoService = jasmine.createSpyObj<CryptoService>('CryptoService', ['encryptMessage']);
		cryptoService.encryptMessage.and.returnValue(Promise.resolve(encryptionResult()));

		apiClient = jasmine.createSpyObj<ApiClientService>('ApiClientService', ['storeSecretMessage']);
		apiClient.storeSecretMessage.and.returnValue(of('message-id'));

		fileService = jasmine.createSpyObj<FileService>('FileService', ['readFileAsBase64Blob']);
		fileService.readFileAsBase64Blob.and.returnValue(Promise.resolve('data:text/plain;base64,aGVsbG8='));

		toastService = jasmine.createSpyObj<ToastService>(
			'ToastService',
			['success', 'error', 'warning', 'info', 'dismiss', 'clear']);

		await TestBed.configureTestingModule({
			imports: [CreateSecretMessageComponent],
			providers: [
				{ provide: Router, useValue: router },
				{ provide: CryptoService, useValue: cryptoService },
				{ provide: ApiClientService, useValue: apiClient },
				{ provide: FileService, useValue: fileService },
				{ provide: ToastService, useValue: toastService }
			]
		}).compileComponents();

		stateService = TestBed.inject(NewSecretMessageStateService);

		fixture = TestBed.createComponent(CreateSecretMessageComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should start with the optional controls disabled', () => {
		expect(component.usernameFormControl.disabled).toBeTrue();
		expect(component.passwordFormControl.disabled).toBeTrue();
		expect(component.attachedFileFormControl.disabled).toBeTrue();
		expect(component.secretMsgOtpRecipientsEmailFormControl.disabled).toBeTrue();
		expect(component.formSubmitted()).toBeFalse();
	});

	it('should enable and reset the username and password fields', () => {
		component.includeUsernameAndPasswordFormControl.setValue(true);
		component.includeUsernameAndPasswordCheckboxToggle();

		expect(component.usernameFormControl.enabled).toBeTrue();
		expect(component.passwordFormControl.enabled).toBeTrue();

		component.usernameFormControl.setValue('user');
		component.passwordFormControl.setValue('pass');
		component.includeUsernameAndPasswordFormControl.setValue(false);
		component.includeUsernameAndPasswordCheckboxToggle();

		expect(component.usernameFormControl.disabled).toBeTrue();
		expect(component.passwordFormControl.disabled).toBeTrue();
		expect(component.username).toBe('');
		expect(component.password).toBe('');
	});

	it('should enable the file input and clear the selected file when disabled', () => {
		component.includeAttachedFileFormControl.setValue(true);
		component.includeAttachedFileCheckboxToggle();
		expect(component.attachedFileFormControl.enabled).toBeTrue();

		const file = new File(['hello'], 'greeting.txt', { type: 'text/plain' });
		component.attachedFile = file;

		component.includeAttachedFileFormControl.setValue(false);
		component.includeAttachedFileCheckboxToggle();

		expect(component.attachedFileFormControl.disabled).toBeTrue();
		expect(component.attachedFile).toBeNull();
		expect(component.attachedFileFormControl.value).toBeNull();
	});

	it('should enable and reset the OTP email field', () => {
		component.secretMsgRequiresOtpFormControl.setValue(true);
		component.secretMsgRequiresOtpCheckboxToggle();
		expect(component.secretMsgOtpRecipientsEmailFormControl.enabled).toBeTrue();

		component.secretMsgOtpRecipientsEmailFormControl.setValue('recipient@example.com');
		component.secretMsgRequiresOtpFormControl.setValue(false);
		component.secretMsgRequiresOtpCheckboxToggle();

		expect(component.secretMsgOtpRecipientsEmailFormControl.disabled).toBeTrue();
		expect(component.secretMsgOtpRecipientsEmail).toBe('');
	});

	it('should not submit an invalid form', () => {
		component.submitNewSecretMessageForm();

		expect(component.formSubmitted()).toBeFalse();
		expect(cryptoService.encryptMessage).not.toHaveBeenCalled();
	});

	it('should encrypt, store and navigate to the new message page', async () => {
		component.secretMsgPlainTextFormControl.setValue('  secret message  ');

		await component.createSecretMessage();

		expect(cryptoService.encryptMessage).toHaveBeenCalledWith({
			plainText: 'secret message',
			containsFile: false,
			fileName: null,
			base64BlobFile: null,
			containsUsernameAndPassword: false,
			username: '',
			password: ''
		});
		expect(apiClient.storeSecretMessage).toHaveBeenCalledWith({
			secretMessageData: { iv: 'iv', ct: 'ciphertext' },
			otp: { required: false, recipientsEmail: '' },
			encryptionKeySha256: 'key-hash'
		});
		expect(stateService.get()).toEqual({
			secretMsgObj: jasmine.objectContaining({ plainText: 'secret message' }),
			secretMsgId: 'message-id',
			secretMsgKey: 'encryption-key',
			otp: { required: false, recipientsEmail: '' }
		});
		expect(router.navigate).toHaveBeenCalledWith([Routes.NewSecretMessage]);
		expect(component.formSubmitted()).toBeFalse();
	});

	it('should include credentials and an attached file in the encrypted payload', async () => {
		component.secretMsgPlainTextFormControl.setValue('secret');
		component.includeUsernameAndPasswordFormControl.setValue(true);
		component.includeUsernameAndPasswordCheckboxToggle();
		component.usernameFormControl.setValue(' user ');
		component.passwordFormControl.setValue(' pass ');
		component.includeAttachedFileFormControl.setValue(true);
		component.includeAttachedFileCheckboxToggle();

		const file = new File(['hello'], 'greeting.txt', { type: 'text/plain' });
		component.attachedFile = file;

		await component.createSecretMessage();

		expect(fileService.readFileAsBase64Blob).toHaveBeenCalledWith(file);
		expect(cryptoService.encryptMessage).toHaveBeenCalledWith({
			plainText: 'secret',
			containsFile: true,
			fileName: 'greeting.txt',
			base64BlobFile: 'data:text/plain;base64,aGVsbG8=',
			containsUsernameAndPassword: true,
			username: 'user',
			password: 'pass'
		});
	});

	it('should send the OTP settings to the API', async () => {
		component.secretMsgPlainTextFormControl.setValue('secret');
		component.secretMsgRequiresOtpFormControl.setValue(true);
		component.secretMsgRequiresOtpCheckboxToggle();
		component.secretMsgOtpRecipientsEmailFormControl.setValue(' recipient@example.com ');

		await component.createSecretMessage();

		expect(apiClient.storeSecretMessage).toHaveBeenCalledWith(jasmine.objectContaining({
			otp: { required: true, recipientsEmail: 'recipient@example.com' }
		}));
	});

	it('should reject an encrypted payload that is too large', async () => {
		component.secretMsgPlainTextFormControl.setValue('secret');
		cryptoService.encryptMessage.and.returnValue(Promise.resolve(
			encryptionResult('a'.repeat(Constants.MAX_ENCRYPTED_PAYLOAD_BASE64_LENGTH + 1))));

		await component.createSecretMessage();

		expect(toastService.warning).toHaveBeenCalledWith(
			'The encrypted message is too large to store. Reduce the message text or attach a smaller file.',
			'Message too large');
		expect(apiClient.storeSecretMessage).not.toHaveBeenCalled();
		expect(router.navigate).not.toHaveBeenCalled();
	});

	it('should not report an HTTP failure again', async () => {
		component.secretMsgPlainTextFormControl.setValue('secret');
		apiClient.storeSecretMessage.and.returnValue(throwError(() =>
			new HttpErrorResponse({ status: 500, error: 'failure' })));

		await component.createSecretMessage();

		expect(toastService.error).not.toHaveBeenCalled();
		expect(component.formSubmitted()).toBeFalse();
	});

	it('should report an encryption failure', async () => {
		component.secretMsgPlainTextFormControl.setValue('secret');
		cryptoService.encryptMessage.and.returnValue(Promise.reject(new Error('crypto failure')));

		await component.createSecretMessage();

		expect(toastService.error).toHaveBeenCalledWith('Could not encrypt the message. Please try again.');
		expect(component.formSubmitted()).toBeFalse();
	});

	it('should keep the form busy until encryption and storage finish', async () => {
		let resolveEncryption!: (result: EncryptionResult) => void;
		cryptoService.encryptMessage.and.returnValue(new Promise(resolve => resolveEncryption = resolve));
		component.secretMsgPlainTextFormControl.setValue('secret');

		component.submitNewSecretMessageForm();

		expect(component.formSubmitted()).toBeTrue();

		resolveEncryption(encryptionResult());
		await new Promise(resolve => setTimeout(resolve, 0));

		expect(component.formSubmitted()).toBeFalse();
		expect(router.navigate).toHaveBeenCalled();
	});

	it('should accept a file that is within the size limit', () => {
		const file = new File(['hello'], 'greeting.txt', { type: 'text/plain' });

		component.onFileSelection({ target: { files: [file] } } as unknown as Event);

		expect(component.attachedFile).toBe(file);
		expect(toastService.warning).not.toHaveBeenCalled();
	});

	it('should reject a file that exceeds the size limit', () => {
		const file = new File([new Uint8Array(1024 * 1024 + 1)], 'large.bin');

		component.onFileSelection({ target: { files: [file] } } as unknown as Event);

		expect(component.attachedFile).toBeNull();
		expect(component.attachedFileFormControl.value).toBeNull();
		expect(toastService.warning).toHaveBeenCalledWith(
			`Selected file size can not exceed ${Constants.ATTACHMENT_FILESIZE_MAX_MB}MB`);
	});

	it('should clear the selection when the file dialog is cancelled', () => {
		component.attachedFile = new File(['hello'], 'greeting.txt');

		component.onFileSelection({ target: { files: [] } } as unknown as Event);

		expect(component.attachedFile).toBeNull();
		expect(component.attachedFileFormControl.value).toBeNull();
	});
});
