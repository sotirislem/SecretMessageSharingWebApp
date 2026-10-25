import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { Constants } from '../../constants';
import { EncryptionResult } from '../models/encryption-result.model';
import { SecretMessage } from '../models/secret-message.model';
import { ApiClientService } from './api-client.service';
import { CryptoService } from './crypto.service';
import { SecretMessageCreationService } from './secret-message-creation.service';
import { ToastService } from './toast.service';

describe('SecretMessageCreationService', () => {
	const secretMsgObj: SecretMessage = {
		plainText: 'secret',
		containsFile: false,
		fileName: null,
		base64BlobFile: null,
		containsUsernameAndPassword: false,
		username: '',
		password: ''
	};

	const otp = { required: true, recipientsEmail: 'recipient@example.com' };

	let cryptoService: jasmine.SpyObj<CryptoService>;
	let apiClient: jasmine.SpyObj<ApiClientService>;
	let toastService: jasmine.SpyObj<ToastService>;
	let service: SecretMessageCreationService;

	function encryptionResult(ct = 'ciphertext'): EncryptionResult {
		return {
			secretMessageData: { iv: 'iv', ct },
			encryptionKeyAsBase64Url: 'encryption-key',
			encryptionKeySha256: 'key-hash'
		};
	}

	beforeEach(() => {
		cryptoService = jasmine.createSpyObj<CryptoService>('CryptoService', ['encryptMessage']);
		cryptoService.encryptMessage.and.returnValue(Promise.resolve(encryptionResult()));

		apiClient = jasmine.createSpyObj<ApiClientService>('ApiClientService', ['storeSecretMessage']);
		apiClient.storeSecretMessage.and.returnValue(of('message-id'));

		toastService = jasmine.createSpyObj<ToastService>(
			'ToastService',
			['success', 'error', 'warning', 'info', 'dismiss', 'clear']);

		TestBed.configureTestingModule({
			providers: [
				{ provide: CryptoService, useValue: cryptoService },
				{ provide: ApiClientService, useValue: apiClient },
				{ provide: ToastService, useValue: toastService }
			]
		});

		service = TestBed.inject(SecretMessageCreationService);
	});

	it('should encrypt the message and store the ciphertext with the OTP settings', async () => {
		const result = await service.create(secretMsgObj, otp);

		expect(cryptoService.encryptMessage).toHaveBeenCalledWith(secretMsgObj);
		expect(apiClient.storeSecretMessage).toHaveBeenCalledWith({
			secretMessageData: { iv: 'iv', ct: 'ciphertext' },
			otp,
			encryptionKeySha256: 'key-hash'
		});
		expect(result).toEqual({ secretMsgId: 'message-id', secretMsgKey: 'encryption-key' });
		expect(toastService.warning).not.toHaveBeenCalled();
		expect(toastService.error).not.toHaveBeenCalled();
	});

	it('should refuse an encrypted payload that is too large', async () => {
		cryptoService.encryptMessage.and.returnValue(Promise.resolve(
			encryptionResult('a'.repeat(Constants.MAX_ENCRYPTED_PAYLOAD_BASE64_LENGTH + 1))));

		const result = await service.create(secretMsgObj, otp);

		expect(result).toBeNull();
		expect(toastService.warning).toHaveBeenCalledWith(
			'The encrypted message is too large to store. Reduce the message text or attach a smaller file.',
			'Message too large');
		expect(apiClient.storeSecretMessage).not.toHaveBeenCalled();
	});

	it('should not report an HTTP failure that the interceptor already reported', async () => {
		apiClient.storeSecretMessage.and.returnValue(throwError(() =>
			new HttpErrorResponse({ status: 500, error: 'failure' })));

		const result = await service.create(secretMsgObj, otp);

		expect(result).toBeNull();
		expect(toastService.error).not.toHaveBeenCalled();
	});

	it('should report an encryption failure', async () => {
		cryptoService.encryptMessage.and.returnValue(Promise.reject(new Error('crypto failure')));

		const result = await service.create(secretMsgObj, otp);

		expect(result).toBeNull();
		expect(toastService.error).toHaveBeenCalledWith('Could not encrypt the message. Please try again.');
		expect(apiClient.storeSecretMessage).not.toHaveBeenCalled();
	});
});
