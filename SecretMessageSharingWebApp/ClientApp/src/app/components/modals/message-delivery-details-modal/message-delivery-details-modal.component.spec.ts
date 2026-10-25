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

	const enrichedDetails = <SecretMessageDeliveryNotification>{
		...details,
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
	};

	function createFixture(
		isNotification: boolean,
		messageDeliveryDetails: SecretMessageDeliveryNotification = details
	) {
		const data: MessageDeliveryDetailsModalData = {
			messageDeliveryDetails,
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

	it('should render location, network and request context when available', () => {
		const fixture = createFixture(false, enrichedDetails);
		const text = fixture.nativeElement.textContent as string;

		expect(text).toContain('5 minutes');
		expect(text).toContain('Bucharest, Bucuresti, Romania');
		expect(text).toContain('Precise (device location)');
		expect(text).toContain('DIGI Romania');
		expect(text).toContain('Europe/Bucharest (UTC+3:00)');
		expect(text).toContain('(ro-RO)');
		expect(text).toContain('OTP verified');
		expect(text).not.toContain('Opened by sender');
		expect(text).not.toContain('Same network as sender');
	});

	it('should still show the location row for records without location data', () => {
		const fixture = createFixture(false);
		const text = fixture.nativeElement.textContent as string;

		expect(text).toContain('Opened from');
		expect(text).toContain('Not available');
	});

	it('should omit optional rows for records that predate the extra details', () => {
		const fixture = createFixture(false);
		const labels = Array.from(fixture.nativeElement.querySelectorAll('th') as NodeListOf<HTMLElement>)
			.map(header => header.textContent?.trim());

		expect(labels).toContain('Opened from');
		expect(labels).not.toContain('Network provider');
		expect(labels).not.toContain('Recipient time zone');
		expect(labels).not.toContain('Browser language');
		expect(labels).not.toContain('OTP verified');
		expect(labels).not.toContain('Opened by sender');
		expect(labels).not.toContain('Same network as sender');
	});

	it('should dismiss the modal when the close button is clicked', () => {
		const fixture = createFixture(false);

		(fixture.nativeElement.querySelector('.btn-close') as HTMLButtonElement).click();

		expect(modal.dismiss).toHaveBeenCalled();
	});
});
