import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Clipboard } from '@angular/cdk/clipboard';

import { Constants } from '../../../constants';
import { CreatedSecretMessage, SecretMessageCreationService } from '../../services/secret-message-creation.service';
import { NewSecretMessageStateService } from '../../services/new-secret-message-state.service';
import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';
import { ToastService } from '../../services/toast.service';
import { UrlHelperService } from '../../services/url-helper.service';
import { ModalService } from '../../services/modal.service';
import { SecretMessageDeliveryNotification } from '../../models/api/secret-message-delivery-notification.model';
import { MessageDeliveryDetailsModalComponent } from '../modals/message-delivery-details-modal/message-delivery-details-modal.component';
import { NewSecretMessageComponent } from './new-secret-message.component';

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

describe('NewSecretMessageComponent', () => {
	const url = 'https://example.com/get-secret-message?id=message-id#encryption-key';

	let stateService: NewSecretMessageStateService;
	let clipboard: jasmine.SpyObj<Clipboard>;
	let creationService: jasmine.SpyObj<SecretMessageCreationService>;
	let toastService: jasmine.SpyObj<ToastService>;
	let modalService: jasmine.SpyObj<ModalService>;
	let deliveredMessageIds: ReturnType<typeof signal<readonly string[]>>;
	let deliveryNotifications: ReturnType<typeof signal<readonly SecretMessageDeliveryNotification[]>>;
	let originalWorker: typeof Worker;
	let fixture: ReturnType<typeof TestBed.createComponent<NewSecretMessageComponent>>;
	let component: NewSecretMessageComponent;

	beforeEach(async () => {
		FakeWorker.instances = [];
		originalWorker = window.Worker;
		window.Worker = FakeWorker as unknown as typeof Worker;

		stateService = new NewSecretMessageStateService();
		stateService.set({
			secretMsgObj: {
				plainText: 'A secret message',
				containsFile: true,
				fileName: 'greeting.txt',
				base64BlobFile: 'data:text/plain;base64,aGVsbG8=',
				containsUsernameAndPassword: true,
				username: 'user',
				password: 'pass'
			},
			secretMsgId: 'message-id',
			secretMsgKey: 'encryption-key',
			otp: { required: false, recipientsEmail: '' }
		});

		clipboard = jasmine.createSpyObj<Clipboard>('Clipboard', ['copy']);
		deliveredMessageIds = signal<readonly string[]>([]);
		deliveryNotifications = signal<readonly SecretMessageDeliveryNotification[]>([]);

		modalService = jasmine.createSpyObj<ModalService>('ModalService', ['openModal', 'openConfirmationModal']);
		modalService.openModal.and.returnValue(new Promise(() => { }));

		creationService = jasmine.createSpyObj<SecretMessageCreationService>('SecretMessageCreationService', ['create']);
		creationService.create.and.returnValue(Promise.resolve({
			secretMsgId: 'new-message-id',
			secretMsgKey: 'new-encryption-key'
		}));

		toastService = jasmine.createSpyObj<ToastService>(
			'ToastService',
			['success', 'error', 'warning', 'info', 'dismiss', 'clear']);

		const urlHelperService = jasmine.createSpyObj<UrlHelperService>('UrlHelperService', ['createLocalUrlWithParams']);
		urlHelperService.createLocalUrlWithParams.and.callFake((routeName, params, fragment) =>
			`https://example.com/${routeName}?id=${params['id']}#${fragment}`);

		await TestBed.configureTestingModule({
			imports: [NewSecretMessageComponent],
			providers: [
				{ provide: NewSecretMessageStateService, useValue: stateService },
				{ provide: UrlHelperService, useValue: urlHelperService },
				{ provide: Clipboard, useValue: clipboard },
				{ provide: SecretMessageCreationService, useValue: creationService },
				{ provide: ToastService, useValue: toastService },
				{ provide: ModalService, useValue: modalService },
				{
					provide: SecretMessageDeliveryNotificationHubService,
					useValue: {
						deliveredMessageIds: deliveredMessageIds.asReadonly(),
						deliveryNotifications: deliveryNotifications.asReadonly()
					}
				}
			]
		}).compileComponents();

		fixture = TestBed.createComponent(NewSecretMessageComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
		await fixture.whenStable();
	});

	afterEach(() => {
		window.Worker = originalWorker;
	});

	it('should expose the message, id, key and share URL', () => {
		expect(component.secretMsgObj().plainText).toBe('A secret message');
		expect(component.secretMsgId()).toBe('message-id');
		expect(component.secretMsgKey()).toBe('encryption-key');
		expect(component.secretMsgUrl()).toBe(url);
		expect(component.secretMsgDelivered()).toBeFalse();
		expect(component.secretMsgContentCleared()).toBeFalse();
	});

	it('should render the preview, credentials and share details', () => {
		const element: HTMLElement = fixture.nativeElement;
		const inputValues = Array.from(element.querySelectorAll('input'))
			.map(input => (input as HTMLInputElement).value);

		expect((element.querySelector('#secretTextArea') as HTMLTextAreaElement).value).toBe('A secret message');
		expect(inputValues).toContain('user');
		expect(inputValues).toContain('pass');
		expect(inputValues).toContain('greeting.txt');
		expect(inputValues).toContain('message-id');
		expect(inputValues).toContain('encryption-key');
		expect(inputValues).toContain(url);
		expect(element.textContent).toContain('Waiting for recipient');
	});

	it('should switch to the opened state when the hub reports this message', async () => {
		deliveredMessageIds.set(['message-id']);
		fixture.detectChanges();
		await fixture.whenStable();

		expect(component.secretMsgDelivered()).toBeTrue();
		expect(fixture.nativeElement.textContent).toContain('Opened');
		expect(fixture.nativeElement.textContent).not.toContain('Waiting for recipient');
		expect(fixture.nativeElement.querySelector('.qr-card')).toBeNull();
	});

	it('should ignore delivery notifications for another message', async () => {
		deliveredMessageIds.set(['other-message-id']);
		fixture.detectChanges();
		await fixture.whenStable();

		expect(component.secretMsgDelivered()).toBeFalse();
	});

	it('should show the opened button in the ribbon and reopen the delivery details', async () => {
		const notification: SecretMessageDeliveryNotification = {
			messageId: 'message-id',
			messageCreatedOn: new Date('2026-01-01T10:00:00Z'),
			messageDeliveredOn: new Date('2026-01-01T10:05:00Z'),
			recipientIP: '127.0.0.1',
			recipientClientInfo: 'browser'
		};

		deliveredMessageIds.set(['message-id']);
		deliveryNotifications.set([notification]);
		fixture.detectChanges();
		await fixture.whenStable();

		const detailsButton = fixture.nativeElement
			.querySelector('.delivery-details-button') as HTMLButtonElement | null;

		expect(fixture.nativeElement.textContent).toContain('Message Status: Opened');
		expect(detailsButton).not.toBeNull();
		expect(detailsButton!.textContent).toContain('Delivery details');

		detailsButton!.click();

		expect(modalService.openModal).toHaveBeenCalledWith(
			MessageDeliveryDetailsModalComponent,
			{ isNotification: false, messageDeliveryDetails: notification });
	});

	it('should not open delivery details when no notification is known', async () => {
		deliveredMessageIds.set(['message-id']);
		fixture.detectChanges();
		await fixture.whenStable();

		component.showDeliveryDetails();

		expect(modalService.openModal).not.toHaveBeenCalled();
	});

	it('should not show the delivery details button while the message is pending', () => {
		expect(fixture.nativeElement.querySelector('.delivery-details-button')).toBeNull();
		expect(fixture.nativeElement.textContent).toContain('Message Status: Undelivered');
	});

	it('should create another message with the same content and show the new share details', async () => {
		expect(fixture.nativeElement.textContent).toContain('Create another');

		await component.createAnotherSecretMessage();
		fixture.detectChanges();
		await fixture.whenStable();

		expect(creationService.create).toHaveBeenCalledWith(
			jasmine.objectContaining({
				plainText: 'A secret message',
				fileName: 'greeting.txt',
				base64BlobFile: 'data:text/plain;base64,aGVsbG8='
			}),
			{ required: false, recipientsEmail: '' });
		expect(component.secretMsgId()).toBe('new-message-id');
		expect(component.secretMsgKey()).toBe('new-encryption-key');
		expect(component.secretMsgUrl())
			.toBe('https://example.com/get-secret-message?id=new-message-id#new-encryption-key');
		expect(stateService.get()?.secretMsgId).toBe('new-message-id');
		expect(stateService.get()?.secretMsgKey).toBe('new-encryption-key');
		expect(toastService.success).toHaveBeenCalled();
		expect(component.isCreatingAnotherSecretMessage()).toBeFalse();

		// The new message gets a fresh auto-clear window of its own.
		expect(FakeWorker.instances.length).toBe(2);
		expect(FakeWorker.instances[0].terminate).toHaveBeenCalled();
		expect(FakeWorker.instances[1].postMessage).toHaveBeenCalledWith({
			timeOutMs: Constants.NEW_SECRET_MESSAGE_AUTOCLEAR_INTERVAL_MINUTES * 60 * 1000
		});
	});

	it('should go back to the pending state when another message is created after the first was opened', async () => {
		deliveredMessageIds.set(['message-id']);
		fixture.detectChanges();
		await fixture.whenStable();
		expect(component.secretMsgDelivered()).toBeTrue();

		await component.createAnotherSecretMessage();
		fixture.detectChanges();
		await fixture.whenStable();

		expect(component.secretMsgDelivered()).toBeFalse();
		expect(fixture.nativeElement.querySelector('.qr-card')).not.toBeNull();
		expect(fixture.nativeElement.textContent).toContain('Waiting for recipient');
	});

	it('should not create another message after the content was cleared', async () => {
		FakeWorker.instances[0].emit({ clear: true });
		fixture.detectChanges();

		await component.createAnotherSecretMessage();

		expect(creationService.create).not.toHaveBeenCalled();
		expect(fixture.nativeElement.querySelector('.duplicate-action')).toBeNull();
	});

	it('should keep the current message when creating another one fails', async () => {
		creationService.create.and.returnValue(Promise.resolve(null));

		await component.createAnotherSecretMessage();
		fixture.detectChanges();

		expect(component.secretMsgId()).toBe('message-id');
		expect(component.secretMsgKey()).toBe('encryption-key');
		expect(stateService.get()?.secretMsgId).toBe('message-id');
		expect(toastService.success).not.toHaveBeenCalled();
		expect(component.isCreatingAnotherSecretMessage()).toBeFalse();
	});

	it('should disable the button and ignore repeated clicks while creating another message', async () => {
		let resolveCreate!: (value: CreatedSecretMessage | null) => void;
		creationService.create.and.returnValue(new Promise(resolve => resolveCreate = resolve));

		const firstCall = component.createAnotherSecretMessage();
		const secondCall = component.createAnotherSecretMessage();
		fixture.detectChanges();

		expect(creationService.create).toHaveBeenCalledTimes(1);
		expect(component.isCreatingAnotherSecretMessage()).toBeTrue();
		expect((fixture.nativeElement.querySelector('.duplicate-action button') as HTMLButtonElement).disabled)
			.toBeTrue();

		resolveCreate(null);
		await firstCall;
		await secondCall;

		expect(component.isCreatingAnotherSecretMessage()).toBeFalse();
	});

	it('should copy the share URL to the clipboard', () => {
		component.copySecretMsgUrlToClipboard();

		expect(clipboard.copy).toHaveBeenCalledWith(url);
	});

	it('should save the QR code as a file', () => {
		const link = document.createElement('a');
		const clickSpy = spyOn(link, 'click');
		spyOn(document, 'createElement').and.returnValue(link);
		spyOn(HTMLCanvasElement.prototype, 'toDataURL').and.returnValue('data:image/png;base64,qr');

		component.saveQrCodeToFile();

		expect(link.href).toBe('data:image/png;base64,qr');
		expect(link.download).toBe('QRCode.png');
		expect(clickSpy).toHaveBeenCalled();
	});

	it('should give the auto-clear worker the sender page interval', () => {
		expect(FakeWorker.instances[0].postMessage).toHaveBeenCalledWith({
			timeOutMs: Constants.NEW_SECRET_MESSAGE_AUTOCLEAR_INTERVAL_MINUTES * 60 * 1000
		});
	});

	it('should clear the message content when the auto-clear timer fires', () => {
		const worker = FakeWorker.instances[0];

		worker.emit({ clear: true });
		fixture.detectChanges();

		expect(component.secretMsgContentCleared()).toBeTrue();
		expect(component.secretMsgObj().plainText).toBe('');
		expect(component.secretMsgObj().containsUsernameAndPassword).toBeFalse();
		expect(component.secretMsgObj().containsFile).toBeFalse();
		expect(worker.terminate).toHaveBeenCalled();

		const state = stateService.get();
		expect(state?.secretMsgObj.plainText).toBe('');
		expect(state?.secretMsgId).toBe('message-id');
		expect(state?.secretMsgKey).toBe('encryption-key');
		expect(fixture.nativeElement.textContent).toContain('automatically cleared');
	});

	it('should ignore unrelated worker messages', () => {
		const worker = FakeWorker.instances[0];

		worker.emit({ clear: false });

		expect(component.secretMsgContentCleared()).toBeFalse();
		expect(component.secretMsgObj().plainText).toBe('A secret message');
	});

	it('should drop the in-memory message and clear the QR code when destroyed', () => {
		const worker = FakeWorker.instances[0];
		const canvas = fixture.nativeElement.querySelector('#qrCodeCanvas') as HTMLCanvasElement;

		fixture.destroy();

		expect(worker.terminate).toHaveBeenCalled();
		expect(stateService.get()).toBeNull();
		expect(component.secretMsgObj().plainText).toBe('');
		expect(component.secretMsgId()).toBe('');
		expect(component.secretMsgKey()).toBe('');
		expect(component.secretMsgUrl()).toBe('');
		expect(canvas.width).toBe(0);
		expect(canvas.height).toBe(0);
	});

	it('should not reopen an open tooltip and close it after five seconds', () => {
		const tooltip = (component as unknown as {
			tooltip: () => {
				isOpen: () => boolean;
				open: () => void;
				close: () => void;
			} | undefined;
		}).tooltip();

		expect(tooltip).toBeDefined();

		const openSpy = spyOn(tooltip!, 'open');
		const closeSpy = spyOn(tooltip!, 'close');
		spyOn(tooltip!, 'isOpen').and.returnValue(true);

		jasmine.clock().install();
		try {
			component.copySecretMsgUrlToClipboard();

			expect(openSpy).not.toHaveBeenCalled();

			jasmine.clock().tick(4_999);
			expect(closeSpy).not.toHaveBeenCalled();

			jasmine.clock().tick(1);
			expect(closeSpy).toHaveBeenCalled();
		} finally {
			jasmine.clock().uninstall();
		}
	});

	it('should skip QR code operations when there is no canvas', () => {
		spyOn(component as unknown as { qrCodeCanvas: () => unknown }, 'qrCodeCanvas')
			.and.returnValue(undefined);
		const createElementSpy = spyOn(document, 'createElement').and.callThrough();

		(component as unknown as { renderQrCode: () => void }).renderQrCode();
		component.saveQrCodeToFile();
		(component as unknown as { clearQrCode: () => void }).clearQrCode();

		expect(createElementSpy).not.toHaveBeenCalledWith('a');
	});
});
