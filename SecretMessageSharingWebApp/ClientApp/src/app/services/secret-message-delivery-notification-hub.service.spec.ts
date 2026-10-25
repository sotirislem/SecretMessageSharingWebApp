import { createEnvironmentInjector, EnvironmentInjector, runInInjectionContext } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { NgbModalOptions } from '@ng-bootstrap/ng-bootstrap';

import { MessageDeliveryDetailsModalComponent } from '../components/modals/message-delivery-details-modal/message-delivery-details-modal.component';
import { SecretMessageDeliveryNotification } from '../models/api/secret-message-delivery-notification.model';
import { API_URL, CLIENT_ID, TAB_ID } from '../tokens';
import { ModalService } from './modal.service';
import { SecretMessageDeliveryNotificationHubService } from './secret-message-delivery-notification-hub.service';
import { ToastService } from './toast.service';

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

describe('SecretMessageDeliveryNotificationHubService', () => {
	const apiUrl = 'https://api.example.com';
	const clientId = 'client-1';
	const tabId = 'tab-1';

	let originalWorker: typeof Worker;
	let modalService: jasmine.SpyObj<ModalService>;
	let toastService: jasmine.SpyObj<ToastService>;
	let injector: EnvironmentInjector;

	beforeEach(() => {
		FakeWorker.instances = [];
		originalWorker = window.Worker;
		window.Worker = FakeWorker as unknown as typeof Worker;

		modalService = jasmine.createSpyObj<ModalService>('ModalService', ['openModal', 'openConfirmationModal']);
		modalService.openModal.and.returnValue(new Promise(() => { }));

		toastService = jasmine.createSpyObj<ToastService>('ToastService', ['success', 'error', 'warning', 'info', 'dismiss', 'clear']);

		injector = createEnvironmentInjector([
			{ provide: API_URL, useValue: apiUrl },
			{ provide: CLIENT_ID, useValue: clientId },
			{ provide: TAB_ID, useValue: tabId },
			{ provide: ModalService, useValue: modalService },
			{ provide: ToastService, useValue: toastService }
		], TestBed.inject(EnvironmentInjector));
	});

	afterEach(() => {
		if (!injector.destroyed) {
			injector.destroy();
		}
		window.Worker = originalWorker;
	});

	function createService(): { service: SecretMessageDeliveryNotificationHubService; worker: FakeWorker } {
		const service = runInInjectionContext(injector, () => new SecretMessageDeliveryNotificationHubService());
		const worker = FakeWorker.instances[FakeWorker.instances.length - 1];

		return { service, worker };
	}

	function notification(messageId = 'message-id'): SecretMessageDeliveryNotification {
		return <SecretMessageDeliveryNotification>{
			messageId,
			messageCreatedOn: new Date(),
			messageDeliveredOn: new Date(),
			recipientIP: '127.0.0.1',
			recipientClientInfo: 'browser'
		};
	}

	it('should create a worker and ask it to start the hub connection', () => {
		const { worker } = createService();

		expect(FakeWorker.instances.length).toBe(1);
		expect(worker.postMessage).toHaveBeenCalledWith({
			action: 'INIT_START',
			apiUrl,
			clientId,
			tabId
		});
	});

	it('should start with no delivered messages', () => {
		const { service } = createService();

		expect(service.deliveredMessageIds()).toEqual([]);
	});

	it('should record a delivered message and open the delivery details modal', () => {
		const { service, worker } = createService();
		const details = notification();

		worker.emit({ action: 'NOTIFICATION', secretMessageDeliveryNotification: details });

		expect(service.deliveredMessageIds()).toEqual(['message-id']);
		expect(modalService.openModal).toHaveBeenCalledWith(
			MessageDeliveryDetailsModalComponent,
			{ isNotification: true, messageDeliveryDetails: details },
			<NgbModalOptions>{ centered: true });
	});

	it('should record every delivery notification', () => {
		const { service, worker } = createService();

		worker.emit({ action: 'NOTIFICATION', secretMessageDeliveryNotification: notification('first') });
		worker.emit({ action: 'NOTIFICATION', secretMessageDeliveryNotification: notification('second') });

		expect(service.deliveredMessageIds()).toEqual(['first', 'second']);
	});

	it('should report worker errors through the toast service', () => {
		const { worker } = createService();

		worker.emit({ action: 'ERROR', error: 'Connection failed' });

		expect(toastService.error).toHaveBeenCalledWith('Connection failed', 'SignalR');
	});

	it('should throw for an unknown worker action', () => {
		const { worker } = createService();

		expect(() => worker.emit({ action: 'UNKNOWN' })).toThrowError('Unknown action');
	});

	it('should ask the worker to reconnect when the page becomes visible without a connection', () => {
		const { worker } = createService();
		worker.postMessage.calls.reset();

		spyOnProperty(Document.prototype, 'visibilityState', 'get').and.returnValue('visible');
		document.dispatchEvent(new Event('visibilitychange'));

		expect(worker.postMessage).toHaveBeenCalledWith({ action: 'START' });
	});

	it('should not reconnect while the page is hidden or a connection exists', () => {
		const { worker } = createService();
		worker.postMessage.calls.reset();

		const visibilityStateSpy = spyOnProperty(Document.prototype, 'visibilityState', 'get').and.returnValue('hidden');
		document.dispatchEvent(new Event('visibilitychange'));
		expect(worker.postMessage).not.toHaveBeenCalled();

		worker.emit({ action: 'CONNECTION_STATE', state: 'connected' });

		visibilityStateSpy.and.returnValue('visible');
		document.dispatchEvent(new Event('visibilitychange'));
		expect(worker.postMessage).not.toHaveBeenCalled();
	});

	it('should ask the worker to reconnect while the connection is reconnecting', () => {
		const { worker } = createService();
		worker.emit({ action: 'CONNECTION_STATE', state: 'reconnecting' });
		worker.postMessage.calls.reset();

		spyOnProperty(Document.prototype, 'visibilityState', 'get').and.returnValue('visible');
		document.dispatchEvent(new Event('visibilitychange'));

		expect(worker.postMessage).toHaveBeenCalledWith({ action: 'START' });
	});

	it('should ask the worker to reconnect after the connection was closed', () => {
		const { worker } = createService();
		worker.emit({ action: 'CONNECTION_STATE', state: 'connected' });
		worker.emit({ action: 'CONNECTION_STATE', state: 'disconnected' });
		worker.postMessage.calls.reset();

		spyOnProperty(Document.prototype, 'visibilityState', 'get').and.returnValue('visible');
		document.dispatchEvent(new Event('visibilitychange'));

		expect(worker.postMessage).toHaveBeenCalledWith({ action: 'START' });
	});

	it('should terminate the worker and stop listening when destroyed', () => {
		const { worker } = createService();
		const removeEventListenerSpy = spyOn(document, 'removeEventListener');

		injector.destroy();

		expect(worker.terminate).toHaveBeenCalled();
		expect(removeEventListenerSpy).toHaveBeenCalledWith('visibilitychange', jasmine.any(Function));
	});

	it('should ignore a dismissed delivery notification modal', async () => {
		modalService.openModal.and.returnValue(Promise.reject('dismissed'));
		const { worker } = createService();

		worker.emit({ action: 'NOTIFICATION', secretMessageDeliveryNotification: notification() });
		await Promise.resolve();

		expect(modalService.openModal).toHaveBeenCalled();
	});
});
