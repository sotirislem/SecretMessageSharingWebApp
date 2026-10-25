import { TestBed } from '@angular/core/testing';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

import { SecretMessageDeliveryNotification } from '../../../models/api/secret-message-delivery-notification.model';
import { MODAL_DATA } from '../../../tokens';
import {
	MessageDeliveryDetailsModalComponent,
	MessageDeliveryDetailsModalData
} from './message-delivery-details-modal.component';

describe('MessageDeliveryDetailsModalComponent', () => {
	let modal: jasmine.SpyObj<NgbActiveModal>;

	const details = <SecretMessageDeliveryNotification>{
		messageId: 'message-id',
		messageCreatedOn: new Date(2026, 9, 25, 10, 30, 0),
		messageDeliveredOn: new Date(2026, 9, 25, 10, 35, 0),
		recipientIP: '192.168.1.10',
		recipientClientInfo: 'Chrome on Windows'
	};

	function createFixture(isNotification: boolean) {
		const data: MessageDeliveryDetailsModalData = {
			messageDeliveryDetails: details,
			isNotification
		};

		TestBed.configureTestingModule({
			imports: [MessageDeliveryDetailsModalComponent],
			providers: [
				{ provide: MODAL_DATA, useValue: data },
				{ provide: NgbActiveModal, useValue: modal }
			]
		});

		const fixture = TestBed.createComponent(MessageDeliveryDetailsModalComponent);
		fixture.detectChanges();

		return fixture;
	}

	beforeEach(() => {
		modal = jasmine.createSpyObj<NgbActiveModal>('NgbActiveModal', ['close', 'dismiss']);
	});

	it('should render the delivery details', () => {
		const fixture = createFixture(false);
		const element: HTMLElement = fixture.nativeElement;

		expect(element.querySelector('.modal-title')?.textContent).toContain('Message details');
		expect(element.textContent).toContain('message-id');
		expect(element.textContent).toContain('25/10/2026 10:30:00');
		expect(element.textContent).toContain('25/10/2026 10:35:00');
		expect(element.textContent).toContain('192.168.1.10');
		expect(element.textContent).toContain('Chrome on Windows');
		expect(element.textContent).not.toContain('Your message has been opened');
	});

	it('should render the notification explanation for incoming notifications', () => {
		const fixture = createFixture(true);

		expect(fixture.nativeElement.querySelector('.modal-title')?.textContent)
			.toContain('Message notification');
		expect(fixture.nativeElement.textContent).toContain('Your message has been opened');
	});

	it('should dismiss the modal when the close button is clicked', () => {
		const fixture = createFixture(false);

		(fixture.nativeElement.querySelector('.btn-close') as HTMLButtonElement).click();

		expect(modal.dismiss).toHaveBeenCalled();
	});
});
