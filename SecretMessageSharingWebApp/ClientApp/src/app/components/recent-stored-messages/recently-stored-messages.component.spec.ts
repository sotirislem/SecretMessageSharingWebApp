import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import {
	RecentlyStoredSecretMessage,
	RecentlyStoredSecretMessagesResponse
} from '../../models/api/recently-stored-secret-messages-response.model';
import { ApiClientService } from '../../services/api-client.service';
import { ModalService } from '../../services/modal.service';
import { ToastService } from '../../services/toast.service';
import { MessageDeliveryDetailsModalComponent } from '../modals/message-delivery-details-modal/message-delivery-details-modal.component';
import { RecentlyStoredMessagesComponent } from './recently-stored-messages.component';

describe('RecentlyStoredMessagesComponent', () => {
	const apiUrl = '/api/secret-messages';
	const createdAt = new Date(2026, 9, 25, 9, 0, 0);

	const deliveredMessage: RecentlyStoredSecretMessage = {
		id: 'delivered-id',
		createdDateTime: createdAt,
		deliveryDetails: {
			deliveredAt: new Date(2026, 9, 25, 9, 5, 0),
			recipientIP: '192.168.1.10',
			recipientClientInfo: 'Chrome on Windows',
			recipientCity: 'Bucharest',
			recipientRegion: 'Bucuresti',
			recipientCountry: 'Romania',
			recipientCountryCode: 'RO',
			recipientLocationSource: 'device',
			recipientIsp: 'DIGI Romania',
			recipientTimezone: 'Europe/Bucharest',
			recipientUtcOffsetMinutes: 180,
			recipientLanguage: 'ro-RO',
			otpVerified: true
		}
	};
	const pendingMessage: RecentlyStoredSecretMessage = {
		id: 'pending-id',
		createdDateTime: createdAt
	};

	let apiClient: jasmine.SpyObj<ApiClientService>;
	let modalService: jasmine.SpyObj<ModalService>;
	let toastService: jasmine.SpyObj<ToastService>;
	let httpTesting: HttpTestingController;
	let writeTextSpy: jasmine.Spy;
	let fixture: ReturnType<typeof TestBed.createComponent<RecentlyStoredMessagesComponent>>;
	let component: RecentlyStoredMessagesComponent;

	beforeEach(async () => {
		apiClient = jasmine.createSpyObj<ApiClientService>(
			'ApiClientService',
			['getRecentlyStoredSecretMessagesUrl', 'deleteRecentlyStoredSecretMessage']);
		apiClient.getRecentlyStoredSecretMessagesUrl.and.returnValue(apiUrl);
		apiClient.deleteRecentlyStoredSecretMessage.and.returnValue(of(undefined));

		modalService = jasmine.createSpyObj<ModalService>('ModalService', ['openModal', 'openConfirmationModal']);
		modalService.openModal.and.returnValue(Promise.resolve());
		modalService.openConfirmationModal.and.returnValue(Promise.resolve(true));
		toastService = jasmine.createSpyObj<ToastService>(
			'ToastService',
			['success', 'error', 'warning', 'info', 'dismiss', 'clear']);

		writeTextSpy = jasmine.createSpy('writeText').and.returnValue(Promise.resolve());
		Object.defineProperty(navigator, 'clipboard', {
			configurable: true,
			value: { writeText: writeTextSpy }
		});

		await TestBed.configureTestingModule({
			imports: [RecentlyStoredMessagesComponent],
			providers: [
				provideHttpClient(),
				provideHttpClientTesting(),
				{ provide: ApiClientService, useValue: apiClient },
				{ provide: ModalService, useValue: modalService },
				{ provide: ToastService, useValue: toastService }
			]
		}).compileComponents();

		httpTesting = TestBed.inject(HttpTestingController);
		fixture = TestBed.createComponent(RecentlyStoredMessagesComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	afterEach(() => {
		httpTesting.verify();
		delete (navigator as unknown as { clipboard?: unknown }).clipboard;
	});

	async function flushMessages(messages: RecentlyStoredSecretMessage[]): Promise<void> {
		const response: RecentlyStoredSecretMessagesResponse = { recentlyStoredSecretMessages: messages };
		httpTesting.expectOne(apiUrl).flush(response);
		await fixture.whenStable();
		fixture.detectChanges();
	}

	it('should show a loading state while the messages are requested', async () => {
		expect(component.loading()).toBeTrue();
		expect(component.recentlyStoredSecretMessages()).toEqual([]);
		expect(component.deliveredCount()).toBe(0);
		expect(component.pendingCount()).toBe(0);
		expect(fixture.nativeElement.textContent).toContain('Loading messages');

		await flushMessages([]);

		expect(component.loading()).toBeFalse();
	});

	it('should render delivered and pending messages with their counts', async () => {
		await flushMessages([deliveredMessage, pendingMessage]);

		const element: HTMLElement = fixture.nativeElement;
		expect(component.deliveredCount()).toBe(1);
		expect(component.pendingCount()).toBe(1);
		expect(element.querySelectorAll('.message-item').length).toBe(2);
		expect(element.textContent).toContain('1 delivered');
		expect(element.textContent).toContain('1 pending');
		expect(element.textContent).toContain('delivered-id');
		expect(element.textContent).toContain('pending-id');
		expect(element.querySelectorAll('.status-pill--success').length).toBe(1);
		expect(element.querySelectorAll('.status-pill--warning').length).toBe(1);
		expect(element.querySelector('.message-time')?.textContent).toContain('25/10/2026 09:00:00');
	});

	it('should show an empty state when there are no messages', async () => {
		await flushMessages([]);

		const element: HTMLElement = fixture.nativeElement;
		expect(element.querySelector('.empty-state')?.textContent).toContain('No messages yet');
		expect(element.querySelectorAll('.message-item').length).toBe(0);
	});

	it('should show an error state when the request fails', async () => {
		httpTesting.expectOne(apiUrl).flush('failure', { status: 500, statusText: 'Server Error' });
		await fixture.whenStable();
		fixture.detectChanges();

		expect(component.loadError()).toBeTruthy();
		expect(fixture.nativeElement.textContent).toContain('Could not load recently stored messages');
	});

	it('should copy a message id and report success', async () => {
		await flushMessages([deliveredMessage]);

		await component.copyMessageId('delivered-id');

		expect(writeTextSpy).toHaveBeenCalledWith('delivered-id');
		expect(toastService.success).toHaveBeenCalledWith('Message ID copied to clipboard.', 'Copied');
	});

	it('should report a failed copy', async () => {
		await flushMessages([deliveredMessage]);
		writeTextSpy.and.returnValue(Promise.reject(new Error('denied')));

		await component.copyMessageId('delivered-id');

		expect(toastService.error).toHaveBeenCalledWith(
			'Could not copy the message ID. Select and copy it manually.',
			'Copy failed');
	});

	it('should open the delivery details for a delivered message', async () => {
		await flushMessages([deliveredMessage]);

		component.showDeliveryDetails(deliveredMessage);

		expect(modalService.openModal).toHaveBeenCalledWith(
			MessageDeliveryDetailsModalComponent,
			{
				isNotification: false,
				messageDeliveryDetails: {
					messageId: 'delivered-id',
					messageCreatedOn: createdAt,
					messageDeliveredOn: deliveredMessage.deliveryDetails!.deliveredAt,
					recipientIP: '192.168.1.10',
					recipientClientInfo: 'Chrome on Windows',
					recipientCity: 'Bucharest',
					recipientRegion: 'Bucuresti',
					recipientCountry: 'Romania',
					recipientCountryCode: 'RO',
					recipientLocationSource: 'device',
					recipientIsp: 'DIGI Romania',
					recipientTimezone: 'Europe/Bucharest',
					recipientUtcOffsetMinutes: 180,
					recipientLanguage: 'ro-RO',
					otpVerified: true
				}
			});
	});

	it('should delete a message after confirmation and reload the list', async () => {
		await flushMessages([pendingMessage]);
		modalService.openConfirmationModal.and.returnValue(Promise.resolve(true));

		await component.deleteStoredMessage(pendingMessage);
		await fixture.whenStable();
		fixture.detectChanges();

		expect(modalService.openConfirmationModal).toHaveBeenCalledWith(jasmine.anything(), 'Delete Message');
		expect(apiClient.deleteRecentlyStoredSecretMessage).toHaveBeenCalledWith('pending-id');
		expect(component.messagePendingDeletion()).toBeNull();

		httpTesting.expectOne(apiUrl).flush({ recentlyStoredSecretMessages: [] });
		await fixture.whenStable();
		fixture.detectChanges();
		expect(fixture.nativeElement.textContent).toContain('No messages yet');
	});

	it('should not delete a message when the confirmation is cancelled', async () => {
		await flushMessages([pendingMessage]);
		modalService.openConfirmationModal.and.returnValue(Promise.resolve(false));

		await component.deleteStoredMessage(pendingMessage);

		expect(apiClient.deleteRecentlyStoredSecretMessage).not.toHaveBeenCalled();
		expect(component.messagePendingDeletion()).toBeNull();
	});

	it('should not delete a message when the confirmation modal is dismissed', async () => {
		await flushMessages([pendingMessage]);
		modalService.openConfirmationModal.and.returnValue(Promise.reject('dismissed'));

		await component.deleteStoredMessage(pendingMessage);

		expect(apiClient.deleteRecentlyStoredSecretMessage).not.toHaveBeenCalled();
		expect(component.messagePendingDeletion()).toBeNull();
	});

	it('should reset the pending deletion when the delete request fails', async () => {
		await flushMessages([pendingMessage]);
		modalService.openConfirmationModal.and.returnValue(Promise.resolve(true));
		apiClient.deleteRecentlyStoredSecretMessage.and.returnValue(throwError(() => new Error('failure')));

		await component.deleteStoredMessage(pendingMessage);

		expect(component.messagePendingDeletion()).toBeNull();
	});

	it('should ignore a dismissed delivery details modal', async () => {
		await flushMessages([deliveredMessage]);
		modalService.openModal.and.returnValue(Promise.reject('dismissed'));

		component.showDeliveryDetails(deliveredMessage);
		await Promise.resolve();

		expect(modalService.openModal).toHaveBeenCalledWith(
			MessageDeliveryDetailsModalComponent,
			jasmine.objectContaining({ isNotification: false }));
	});
});
