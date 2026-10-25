import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Clipboard } from '@angular/cdk/clipboard';

import { NewSecretMessageStateService } from '../../services/new-secret-message-state.service';
import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';
import { UrlHelperService } from '../../services/url-helper.service';
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
	let deliveredMessageIds: ReturnType<typeof signal<readonly string[]>>;
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
			secretMsgKey: 'encryption-key'
		});

		clipboard = jasmine.createSpyObj<Clipboard>('Clipboard', ['copy']);
		deliveredMessageIds = signal<readonly string[]>([]);

		await TestBed.configureTestingModule({
			imports: [NewSecretMessageComponent],
			providers: [
				{ provide: NewSecretMessageStateService, useValue: stateService },
				{ provide: UrlHelperService, useValue: { createLocalUrlWithParams: () => url } },
				{ provide: Clipboard, useValue: clipboard },
				{
					provide: SecretMessageDeliveryNotificationHubService,
					useValue: {
						deliveredMessageIds: deliveredMessageIds.asReadonly()
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
