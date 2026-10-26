import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { Clipboard } from '@angular/cdk/clipboard';
import { from, of, throwError } from 'rxjs';

import '../../../extensions/string.extensions';

import { Constants } from '../../../constants';
import { GetSecretMessageResponse } from '../../models/api/get-secret-message-response.model';
import { ClientLocation } from '../../models/client-location.model';
import { DecryptionResult, DecryptionStatus } from '../../models/decryption-result.model';
import { SecretMessage } from '../../models/secret-message.model';
import { ApiClientService } from '../../services/api-client.service';
import { CryptoService } from '../../services/crypto.service';
import { FileService } from '../../services/file.service';
import { ModalService } from '../../services/modal.service';
import { RecipientLocationService } from '../../services/recipient-location.service';
import { ToastService } from '../../services/toast.service';
import { OtpInputModalComponent } from '../modals/otp-input-modal/otp-input-modal.component';
import { GetSecretMessageComponent } from './get-secret-message.component';

class FakeWorker {
	static instances: FakeWorker[] = [];

	onmessage: ((event: MessageEvent) => void) | null = null;
	postMessage = jasmine.createSpy('postMessage');
	terminate = jasmine.createSpy('terminate');

	constructor() {
		FakeWorker.instances.push(this);
	}

	emit(data: unknown): void {
		this.onmessage?.({ data } as MessageEvent);
	}
}

describe('GetSecretMessageComponent', () => {
	const messageId = 'message-id';
	const encryptionKey = 'encryption-key';
	const keyHash = 'key-hash';

	const decryptedMsg: SecretMessage = {
		plainText: 'Visit https://example.com now',
		containsFile: true,
		fileName: 'secret.txt',
		base64BlobFile: 'data:text/plain;base64,aGVsbG8=',
		containsUsernameAndPassword: true,
		username: 'user',
		password: 'pass'
	};

	const emptyMsg: SecretMessage = {
		plainText: '',
		containsFile: false,
		fileName: '',
		base64BlobFile: '',
		containsUsernameAndPassword: false,
		username: '',
		password: ''
	};

	const route = {
		snapshot: {
			fragment: encryptionKey as string | null,
			queryParams: { id: messageId }
		}
	};

	const clientLocation: ClientLocation = {
		timezone: 'Europe/Bucharest',
		utcOffsetMinutes: 180,
		language: 'en-US'
	};

	let apiClient: jasmine.SpyObj<ApiClientService>;
	let cryptoService: jasmine.SpyObj<CryptoService>;
	let fileService: jasmine.SpyObj<FileService>;
	let modalService: jasmine.SpyObj<ModalService>;
	let recipientLocationService: jasmine.SpyObj<RecipientLocationService>;
	let toastService: jasmine.SpyObj<ToastService>;
	let clipboard: jasmine.SpyObj<Clipboard>;
	let originalWorker: typeof Worker;
	let fixture: ReturnType<typeof TestBed.createComponent<GetSecretMessageComponent>>;
	let component: GetSecretMessageComponent;

	beforeEach(async () => {
		FakeWorker.instances = [];
		originalWorker = window.Worker;
		window.Worker = FakeWorker as unknown as typeof Worker;

		route.snapshot.fragment = encryptionKey;
		route.snapshot.queryParams = { id: messageId };

		apiClient = jasmine.createSpyObj<ApiClientService>(
			'ApiClientService',
			['verifySecretMessage', 'getSecretMessage']);
		apiClient.verifySecretMessage.and.returnValue(of({ id: messageId, exists: true, requiresOtp: false }));
		apiClient.getSecretMessage.and.returnValue(of({
			data: { iv: 'iv', ct: 'ciphertext' },
			deliveryNotificationSent: true
		}));

		cryptoService = jasmine.createSpyObj<CryptoService>('CryptoService', ['getSha256', 'decryptMessage']);
		cryptoService.getSha256.and.returnValue(Promise.resolve(keyHash));
		cryptoService.decryptMessage.and.returnValue(Promise.resolve(<DecryptionResult>{
			status: DecryptionStatus.OK,
			decryptedMsg,
			errorMsg: ''
		}));

		fileService = jasmine.createSpyObj<FileService>('FileService', ['saveBase64BlobAsFile', 'readFileAsBase64Blob']);
		modalService = jasmine.createSpyObj<ModalService>('ModalService', ['openModal', 'openConfirmationModal']);
		modalService.openConfirmationModal.and.returnValue(Promise.resolve(true));
		modalService.openModal.and.returnValue(Promise.resolve('auth-token'));

		recipientLocationService = jasmine.createSpyObj<RecipientLocationService>(
			'RecipientLocationService',
			['collect']);
		recipientLocationService.collect.and.returnValue(Promise.resolve(clientLocation));

		toastService = jasmine.createSpyObj<ToastService>(
			'ToastService',
			['success', 'error', 'warning', 'info', 'dismiss', 'clear']);
		clipboard = jasmine.createSpyObj<Clipboard>('Clipboard', ['copy']);

		await TestBed.configureTestingModule({
			imports: [GetSecretMessageComponent],
			providers: [
				{ provide: ActivatedRoute, useValue: route },
				{ provide: ApiClientService, useValue: apiClient },
				{ provide: CryptoService, useValue: cryptoService },
				{ provide: FileService, useValue: fileService },
				{ provide: ModalService, useValue: modalService },
				{ provide: RecipientLocationService, useValue: recipientLocationService },
				{ provide: ToastService, useValue: toastService },
				{ provide: Clipboard, useValue: clipboard }
			]
		}).compileComponents();
	});

	afterEach(() => {
		window.Worker = originalWorker;
		window.history.replaceState(null, '', window.location.pathname + window.location.search);
	});

	async function create(): Promise<void> {
		fixture = TestBed.createComponent(GetSecretMessageComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
		await fixture.whenStable();
		fixture.detectChanges();
	}

	function deferred<T>(): {
		promise: Promise<T>;
		resolve: (value: T) => void;
		reject: (reason?: unknown) => void;
	} {
		let resolve!: (value: T) => void;
		let reject!: (reason?: unknown) => void;
		const promise = new Promise<T>((res, rej) => {
			resolve = res;
			reject = rej;
		});

		return { promise, resolve, reject };
	}

	async function flushMicrotasks(): Promise<void> {
		for (let i = 0; i < 10; i++) {
			await Promise.resolve();
		}
	}

	/**
	 * Creates the component and lets the verification flow advance until it
	 * blocks on a pending promise supplied by the test.
	 */
	async function createUntilPending(): Promise<void> {
		fixture = TestBed.createComponent(GetSecretMessageComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
		await flushMicrotasks();
	}

	it('should read the message id from the route', async () => {
		await create();

		expect(component.messageId()).toBe(messageId);
	});

	it('should show the no-message state when the message does not exist', async () => {
		apiClient.verifySecretMessage.and.returnValue(of({ id: messageId, exists: false }));

		await create();

		expect(component.componentState()).toBe(component.ComponentState.ReadyNoMessage);
		expect(fixture.nativeElement.textContent).toContain('No message was found on server');
		expect(modalService.openConfirmationModal).not.toHaveBeenCalled();
	});

	it('should explain that the encryption key is missing', async () => {
		route.snapshot.fragment = null;

		await create();

		expect(component.componentState()).toBe(component.ComponentState.MissingEncryptionKey);
		expect(fixture.nativeElement.textContent).toContain('The encryption key is missing');
		expect(modalService.openConfirmationModal).not.toHaveBeenCalled();
	});

	it('should show the error state when verification fails', async () => {
		apiClient.verifySecretMessage.and.returnValue(throwError(() => new Error('failure')));

		await create();

		expect(component.componentState()).toBe(component.ComponentState.Error);
		expect(fixture.nativeElement.textContent).toContain('Could not load message');
	});

	it('should abort when the retrieval confirmation is declined', async () => {
		modalService.openConfirmationModal.and.returnValue(Promise.resolve(false));

		await create();

		expect(component.componentState()).toBe(component.ComponentState.Aborted);
		expect(apiClient.getSecretMessage).not.toHaveBeenCalled();
	});

	it('should wait for the location decision before retrieving the message', async () => {
		const location = deferred<ClientLocation>();
		recipientLocationService.collect.and.callFake((onPermissionPrompt?: () => void) => {
			onPermissionPrompt?.();

			return location.promise;
		});

		await createUntilPending();
		fixture.detectChanges();

		expect(component.componentState()).toBe(component.ComponentState.WaitingForLocationPermission);
		expect(fixture.nativeElement.textContent).toContain('Waiting for your location decision');
		expect(apiClient.getSecretMessage).not.toHaveBeenCalled();

		location.resolve(clientLocation);
		await flushMicrotasks();

		expect(apiClient.getSecretMessage).toHaveBeenCalledWith(messageId, keyHash, undefined, clientLocation);
	});

	it('should abort when the confirmation modal is dismissed', async () => {
		modalService.openConfirmationModal.and.returnValue(Promise.reject('dismissed'));

		await create();

		expect(component.componentState()).toBe(component.ComponentState.Aborted);
		expect(apiClient.getSecretMessage).not.toHaveBeenCalled();
	});

	it('should ask for the OTP and use the returned token', async () => {
		apiClient.verifySecretMessage.and.returnValue(of({
			id: messageId,
			exists: true,
			requiresOtp: true
		}));

		await create();

		expect(modalService.openModal).toHaveBeenCalledWith(
			OtpInputModalComponent,
			{ messageId },
			jasmine.objectContaining({ centered: true, backdrop: 'static' }));
		expect(apiClient.getSecretMessage).toHaveBeenCalledWith(messageId, keyHash, 'auth-token', clientLocation);
		expect(component.componentState()).toBe(component.ComponentState.ReadyWithMessage);
	});

	it('should abort when the OTP modal returns no token', async () => {
		apiClient.verifySecretMessage.and.returnValue(of({
			id: messageId,
			exists: true,
			requiresOtp: true
		}));
		modalService.openModal.and.returnValue(Promise.resolve(''));

		await create();

		expect(component.componentState()).toBe(component.ComponentState.Aborted);
		expect(apiClient.getSecretMessage).not.toHaveBeenCalled();
	});

	it('should retrieve, decrypt and display the message', async () => {
		await create();

		expect(cryptoService.getSha256).toHaveBeenCalledWith(encryptionKey);
		expect(apiClient.getSecretMessage).toHaveBeenCalledWith(messageId, keyHash, undefined, clientLocation);
		expect(cryptoService.decryptMessage).toHaveBeenCalledWith(
			{ iv: 'iv', ct: 'ciphertext' },
			encryptionKey);
		expect(component.componentState()).toBe(component.ComponentState.ReadyWithMessage);
		expect(component.decryptionResult()?.status).toBe(DecryptionStatus.OK);
		expect(component.secretMessageTextAsHtml()).toContain('<a href="https://example.com"');
		expect(toastService.success).toHaveBeenCalledWith(
			'Delivery notification was sent', '', { timeOut: 7_000 });
		expect(FakeWorker.instances.length).toBe(1);

		const element: HTMLElement = fixture.nativeElement;
		const inputValues = Array.from(element.querySelectorAll('input'))
			.map(input => (input as HTMLInputElement).value);

		expect(inputValues).toContain('user');
		expect(inputValues).toContain('pass');
		expect(element.textContent).toContain('secret.txt');
	});

	it('should remove the encryption key from the address bar after a successful retrieval', async () => {
		window.history.replaceState(
			null,
			'',
			`${window.location.pathname}${window.location.search}#${encryptionKey}`);
		const replaceStateSpy = spyOn(window.history, 'replaceState').and.callThrough();

		await create();

		expect(replaceStateSpy).toHaveBeenCalledWith(
			window.history.state,
			'',
			`${window.location.pathname}${window.location.search}`);
	});

	it('should warn when the delivery notification could not be sent', async () => {
		apiClient.getSecretMessage.and.returnValue(of({
			data: { iv: 'iv', ct: 'ciphertext' },
			deliveryNotificationSent: false
		}));

		await create();

		expect(toastService.warning).toHaveBeenCalledWith(
			'Delivery notification could not be sent', '', { timeOut: 7_000 });
	});

	it('should display a decryption error', async () => {
		cryptoService.decryptMessage.and.returnValue(Promise.resolve(<DecryptionResult>{
			status: DecryptionStatus.Error,
			decryptedMsg: emptyMsg,
			errorMsg: 'The key is wrong'
		}));

		await create();

		expect(component.componentState()).toBe(component.ComponentState.ReadyWithMessage);
		expect(component.decryptionResult()?.status).toBe(DecryptionStatus.Error);
		expect(component.secretMessageTextAsHtml()).toBe('');
		expect(FakeWorker.instances.length).toBe(0);
		expect(fixture.nativeElement.textContent).toContain('The key is wrong');
	});

	it('should show the error state when retrieving the message fails', async () => {
		apiClient.getSecretMessage.and.returnValue(throwError(() => new Error('failure')));

		await create();

		expect(component.componentState()).toBe(component.ComponentState.Error);
	});

	it('should retry the verification flow', async () => {
		apiClient.verifySecretMessage.and.returnValue(throwError(() => new Error('failure')));
		await create();
		expect(component.componentState()).toBe(component.ComponentState.Error);

		apiClient.verifySecretMessage.and.returnValue(of({ id: messageId, exists: false }));
		component.retry();
		await fixture.whenStable();
		fixture.detectChanges();

		expect(component.componentState()).toBe(component.ComponentState.ReadyNoMessage);
	});

	it('should download the decrypted attachment', async () => {
		await create();

		component.downloadAttachment();

		expect(fileService.saveBase64BlobAsFile).toHaveBeenCalledWith(
			'data:text/plain;base64,aGVsbG8=',
			'secret.txt');
	});

	it('should not download when there is no attachment', async () => {
		await create();
		component.decryptionResult.set({
			status: DecryptionStatus.OK,
			decryptedMsg: { ...decryptedMsg, containsFile: false },
			errorMsg: ''
		});

		component.downloadAttachment();

		expect(fileService.saveBase64BlobAsFile).not.toHaveBeenCalled();
	});

	it('should not download before a message was decrypted', async () => {
		await create();
		component.decryptionResult.set(null);

		component.downloadAttachment();

		expect(fileService.saveBase64BlobAsFile).not.toHaveBeenCalled();
	});

	it('should not download when the attachment data is missing', async () => {
		await create();
		component.decryptionResult.set({
			status: DecryptionStatus.OK,
			decryptedMsg: { ...decryptedMsg, base64BlobFile: '' },
			errorMsg: ''
		});

		component.downloadAttachment();

		expect(fileService.saveBase64BlobAsFile).not.toHaveBeenCalled();
	});

	it('should copy the username, password and full message', async () => {
		await create();

		component.copyUsernameToClipboard();
		component.copyPasswordToClipboard();
		component.copyMessageContentToClipboard();

		expect(clipboard.copy).toHaveBeenCalledWith('user');
		expect(clipboard.copy).toHaveBeenCalledWith('pass');
		expect(clipboard.copy).toHaveBeenCalledWith(
			'Visit https://example.com now\r\nUsername: user\r\nPassword: pass');
	});

	it('should copy only the text when there are no credentials', async () => {
		cryptoService.decryptMessage.and.returnValue(Promise.resolve(<DecryptionResult>{
			status: DecryptionStatus.OK,
			decryptedMsg: { ...decryptedMsg, containsUsernameAndPassword: false },
			errorMsg: ''
		}));
		await create();

		component.copyMessageContentToClipboard();

		expect(clipboard.copy).toHaveBeenCalledWith('Visit https://example.com now');
	});

	it('should not copy empty credentials or a missing message', async () => {
		cryptoService.decryptMessage.and.returnValue(Promise.resolve(<DecryptionResult>{
			status: DecryptionStatus.OK,
			decryptedMsg: { ...decryptedMsg, username: '', password: '' },
			errorMsg: ''
		}));
		await create();

		component.copyUsernameToClipboard();
		component.copyPasswordToClipboard();
		component.decryptionResult.set(null);
		component.copyMessageContentToClipboard();

		expect(clipboard.copy).not.toHaveBeenCalled();
	});

	it('should not copy anything after the message was auto-cleared', async () => {
		await create();
		component.msgAutoClearTimeoutTriggered.set(true);

		component.copyUsernameToClipboard();
		component.copyPasswordToClipboard();
		component.copyMessageContentToClipboard();

		expect(clipboard.copy).not.toHaveBeenCalled();
	});

	it('should clear the decrypted content when the auto-clear timer fires', async () => {
		await create();
		const worker = FakeWorker.instances[0];

		expect(worker.postMessage).toHaveBeenCalledWith({
			timeOutMs: Constants.GET_SECRET_MESSAGE_AUTOCLEAR_INTERVAL_MINUTES * 60 * 1000
		});

		worker.emit({ clear: true });
		fixture.detectChanges();

		expect(component.msgAutoClearTimeoutTriggered()).toBeTrue();
		expect(component.decryptionResult()?.decryptedMsg.plainText).toBe('');
		expect(component.secretMessageTextAsHtml()).toBe('--- Message auto deleted ---');
		expect(worker.terminate).toHaveBeenCalled();
	});

	it('should ignore unrelated worker messages', async () => {
		await create();
		const worker = FakeWorker.instances[0];

		worker.emit({ clear: false });

		expect(component.msgAutoClearTimeoutTriggered()).toBeFalse();
		expect(component.secretMessageTextAsHtml()).toContain('https://example.com');
	});

	it('should clear the message when the page becomes visible after the interval elapsed', async () => {
		await create();

		// The worker timer does not run while the page is hidden or frozen, so the
		// deadline has to be checked again once the page is running.
		let now = Date.now();
		spyOn(Date, 'now').and.callFake(() => now);
		now += Constants.GET_SECRET_MESSAGE_AUTOCLEAR_INTERVAL_MINUTES * 60 * 1000;

		document.dispatchEvent(new Event('visibilitychange'));
		fixture.detectChanges();

		expect(component.msgAutoClearTimeoutTriggered()).toBeTrue();
		expect(component.secretMessageTextAsHtml()).toBe('--- Message auto deleted ---');
	});

	it('should keep the message when the page becomes visible before the interval elapsed', async () => {
		await create();
		const worker = FakeWorker.instances[0];

		document.dispatchEvent(new Event('visibilitychange'));

		expect(component.msgAutoClearTimeoutTriggered()).toBeFalse();
		expect(worker.postMessage).toHaveBeenCalledTimes(2);
	});

	it('should clear the message before the page is cached in the back/forward cache', async () => {
		await create();

		window.dispatchEvent(new PageTransitionEvent('pagehide', { persisted: true }));
		fixture.detectChanges();

		expect(component.msgAutoClearTimeoutTriggered()).toBeTrue();
		expect(component.secretMessageTextAsHtml()).toBe('--- Message auto deleted ---');
	});

	it('should drop decrypted content and stop the timer when destroyed', async () => {
		await create();
		const worker = FakeWorker.instances[0];

		fixture.destroy();

		expect(worker.terminate).toHaveBeenCalled();
		expect(component.decryptionResult()).toBeNull();
		expect(component.secretMessageTextAsHtml()).toBe('');
	});

	it('should not retrieve the message when destroyed while the confirmation modal is open', async () => {
		const confirmation = deferred<boolean>();
		modalService.openConfirmationModal.and.returnValue(confirmation.promise);

		await createUntilPending();
		expect(modalService.openConfirmationModal).toHaveBeenCalled();

		fixture.destroy();
		confirmation.resolve(true);
		await flushMicrotasks();

		expect(apiClient.getSecretMessage).not.toHaveBeenCalled();
		expect(component.componentState()).toBe(component.ComponentState.LoadingMessage);
	});

	it('should not use the OTP token when destroyed while the OTP modal is open', async () => {
		apiClient.verifySecretMessage.and.returnValue(of({
			id: messageId,
			exists: true,
			requiresOtp: true
		}));
		const otp = deferred<string>();
		modalService.openModal.and.returnValue(otp.promise);

		await createUntilPending();
		expect(modalService.openModal).toHaveBeenCalled();

		fixture.destroy();
		otp.resolve('auth-token');
		await flushMicrotasks();

		expect(apiClient.getSecretMessage).not.toHaveBeenCalled();
		expect(component.componentState()).toBe(component.ComponentState.LoadingMessage);
	});

	it('should not request the message when destroyed while hashing the key', async () => {
		const hash = deferred<string>();
		cryptoService.getSha256.and.returnValue(hash.promise);

		await createUntilPending();
		expect(cryptoService.getSha256).toHaveBeenCalled();

		fixture.destroy();
		hash.resolve(keyHash);
		await flushMicrotasks();

		expect(apiClient.getSecretMessage).not.toHaveBeenCalled();
	});

	it('should not display the message when destroyed while decrypting', async () => {
		const decryption = deferred<DecryptionResult>();
		cryptoService.decryptMessage.and.returnValue(decryption.promise);

		await createUntilPending();
		expect(cryptoService.decryptMessage).toHaveBeenCalled();

		fixture.destroy();
		decryption.resolve({
			status: DecryptionStatus.OK,
			decryptedMsg,
			errorMsg: ''
		});
		await flushMicrotasks();

		expect(component.componentState()).toBe(component.ComponentState.LoadingMessage);
		expect(component.decryptionResult()).toBeNull();
	});

	it('should not show an error when verification fails after being destroyed', async () => {
		apiClient.verifySecretMessage.and.returnValue(throwError(() => new Error('failure')));

		fixture = TestBed.createComponent(GetSecretMessageComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
		fixture.destroy();
		await flushMicrotasks();

		expect(component.componentState()).toBe(component.ComponentState.LoadingMessage);
	});

	it('should not abort when the confirmation is rejected after being destroyed', async () => {
		const confirmation = deferred<boolean>();
		modalService.openConfirmationModal.and.returnValue(confirmation.promise);

		await createUntilPending();
		expect(modalService.openConfirmationModal).toHaveBeenCalled();

		fixture.destroy();
		confirmation.reject('dismissed');
		await flushMicrotasks();

		expect(component.componentState()).toBe(component.ComponentState.LoadingMessage);
	});

	it('should not show an error when retrieval fails after being destroyed', async () => {
		const request = deferred<GetSecretMessageResponse>();
		apiClient.getSecretMessage.and.returnValue(from(request.promise));

		await createUntilPending();
		expect(apiClient.getSecretMessage).toHaveBeenCalled();

		fixture.destroy();
		request.reject(new Error('failure'));
		await flushMicrotasks();

		expect(component.componentState()).toBe(component.ComponentState.LoadingMessage);
	});

	it('should default the message id to an empty string when it is missing', async () => {
		route.snapshot.queryParams = {} as { id: string };
		apiClient.verifySecretMessage.and.returnValue(of({ id: '', exists: false }));

		await create();

		expect(component.messageId()).toBe('');
		expect(apiClient.verifySecretMessage).toHaveBeenCalledWith('');
	});

	it('should treat a missing requiresOtp flag as no OTP required', async () => {
		apiClient.verifySecretMessage.and.returnValue(of({ id: messageId, exists: true }));

		await create();

		expect(modalService.openModal).not.toHaveBeenCalled();
		expect(apiClient.getSecretMessage).toHaveBeenCalledWith(messageId, keyHash, undefined, clientLocation);
	});

	it('should keep a missing decryption result when the auto-clear timer fires', async () => {
		await create();
		const worker = FakeWorker.instances[0];
		component.decryptionResult.set(null);

		worker.emit({ clear: true });

		expect(component.decryptionResult()).toBeNull();
		expect(component.msgAutoClearTimeoutTriggered()).toBeTrue();
	});
});
