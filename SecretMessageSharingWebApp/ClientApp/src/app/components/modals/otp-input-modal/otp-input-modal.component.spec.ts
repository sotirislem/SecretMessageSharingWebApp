import { TestBed } from '@angular/core/testing';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError } from 'rxjs';

import { ApiClientService } from '../../../services/api-client.service';
import { RecipientLocationService } from '../../../services/recipient-location.service';
import { MODAL_DATA } from '../../../tokens';
import { OtpInputModalComponent } from './otp-input-modal.component';

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

describe('OtpInputModalComponent', () => {
	const messageId = 'message-id';
	const requestContext = {
		timezone: 'Europe/Bucharest',
		utcOffsetMinutes: 180,
		language: 'ro-RO'
	};

	let apiClient: jasmine.SpyObj<ApiClientService>;
	let recipientLocation: jasmine.SpyObj<RecipientLocationService>;
	let modal: jasmine.SpyObj<NgbActiveModal>;
	let fixture: ReturnType<typeof TestBed.createComponent<OtpInputModalComponent>>;
	let component: OtpInputModalComponent;
	let originalWorker: typeof Worker;

	beforeEach(async () => {
		FakeWorker.instances = [];
		originalWorker = window.Worker;
		window.Worker = FakeWorker as unknown as typeof Worker;
		jasmine.clock().install();

		apiClient = jasmine.createSpyObj<ApiClientService>(
			'ApiClientService',
			['acquireSecretMessageOtp', 'validateSecretMessageOtp']);
		recipientLocation = jasmine.createSpyObj<RecipientLocationService>(
			'RecipientLocationService',
			['collect', 'collectRequestContext']);
		recipientLocation.collectRequestContext.and.returnValue(requestContext);
		modal = jasmine.createSpyObj<NgbActiveModal>('NgbActiveModal', ['close', 'dismiss']);

		await TestBed.configureTestingModule({
			imports: [OtpInputModalComponent],
			providers: [
				{ provide: MODAL_DATA, useValue: { messageId } },
				{ provide: NgbActiveModal, useValue: modal },
				{ provide: ApiClientService, useValue: apiClient },
				{ provide: RecipientLocationService, useValue: recipientLocation }
			]
		}).compileComponents();

		fixture = TestBed.createComponent(OtpInputModalComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	afterEach(() => {
		fixture.destroy();
		jasmine.clock().uninstall();
		window.Worker = originalWorker;
	});

	async function sendOtp(): Promise<void> {
		apiClient.acquireSecretMessageOtp.and.returnValue(of(undefined));
		await component.getOtp();
		fixture.detectChanges();
	}

	it('should start with the OTP not sent and the form invalid', () => {
		expect(component.data.messageId).toBe(messageId);
		expect(component.otpSent()).toBeFalse();
		expect(component.timerActive()).toBeFalse();
		expect(component.otpValidationForm.invalid).toBeTrue();
		expect(component.otpCodeFormControl.errors).toEqual({ otpHasExpired: true });
	});

	it('should send the OTP, start the expiration timer and reset the form', async () => {
		await sendOtp();

		expect(apiClient.acquireSecretMessageOtp).toHaveBeenCalledWith(messageId, requestContext);
		expect(component.otpSent()).toBeTrue();
		expect(component.timerActive()).toBeTrue();
		expect(component.timerExpirationTime()).toBe('03:00');
		expect(FakeWorker.instances.length).toBe(1);

		component.otpCodeFormControl.setValue('12345678');
		jasmine.clock().tick(0);

		expect(component.otpCodeFormControl.value).toBe('');
	});

	it('should not request another OTP while the current one is still active', async () => {
		await sendOtp();
		apiClient.acquireSecretMessageOtp.calls.reset();

		await component.getOtp();

		expect(apiClient.acquireSecretMessageOtp).not.toHaveBeenCalled();
	});

	it('should keep the OTP state unchanged when the request fails', async () => {
		apiClient.acquireSecretMessageOtp.and.returnValue(throwError(() => new Error('failure')));

		await component.getOtp();

		expect(component.otpSent()).toBeFalse();
		expect(component.timerActive()).toBeFalse();
		expect(FakeWorker.instances.length).toBe(0);
	});

	it('should update the countdown from the timer worker', async () => {
		await sendOtp();
		const worker = FakeWorker.instances[0];

		worker.emit({ timerRemainingSeconds: 125, timerExpirationTime: '02:05' });
		fixture.detectChanges();

		expect(component.timerExpirationTime()).toBe('02:05');
		expect(component.timerActive()).toBeTrue();
	});

	it('should stop the timer when the countdown reaches zero', async () => {
		await sendOtp();
		const worker = FakeWorker.instances[0];

		worker.emit({ timerRemainingSeconds: 0, timerExpirationTime: '00:00' });
		fixture.detectChanges();

		expect(component.timerExpirationTime()).toBe('00:00');
		expect(component.timerActive()).toBeFalse();
		expect(worker.terminate).toHaveBeenCalled();
	});

	it('should ignore submissions before the OTP was sent', async () => {
		component.otpCodeFormControl.setValue('12345678');

		await component.submitOtpValidationForm();

		expect(apiClient.validateSecretMessageOtp).not.toHaveBeenCalled();
	});

	it('should require a code', async () => {
		await sendOtp();

		await component.submitOtpValidationForm();

		expect(component.otpCodeFormControl.errors).toEqual({ required: true });
		expect(apiClient.validateSecretMessageOtp).not.toHaveBeenCalled();
	});

	it('should require a code of eight digits', async () => {
		await sendOtp();
		component.otpCodeFormControl.setValue('123');

		await component.submitOtpValidationForm();

		expect(component.otpCodeFormControl.errors).toEqual({ minlength: true });
		expect(apiClient.validateSecretMessageOtp).not.toHaveBeenCalled();
	});

	it('should close the modal with the auth token when the OTP is valid', async () => {
		await sendOtp();
		component.otpCodeFormControl.setValue('12345678');
		apiClient.validateSecretMessageOtp.and.returnValue(of({
			isValid: true,
			canRetry: false,
			hasExpired: false,
			authToken: 'auth-token'
		}));

		await component.submitOtpValidationForm();

		expect(apiClient.validateSecretMessageOtp).toHaveBeenCalledWith(messageId, '12345678');
		expect(modal.close).toHaveBeenCalledWith('auth-token');
	});

	it('should stop the timer when the OTP has expired', async () => {
		await sendOtp();
		component.otpCodeFormControl.setValue('12345678');
		apiClient.validateSecretMessageOtp.and.returnValue(of({
			isValid: false,
			canRetry: false,
			hasExpired: true
		}));

		await component.submitOtpValidationForm();

		expect(component.timerActive()).toBeFalse();
		expect(component.otpCodeFormControl.errors).toEqual({ otpHasExpired: true });
		expect(modal.close).not.toHaveBeenCalled();
	});

	it('should stop the timer when the maximum number of attempts is reached', async () => {
		await sendOtp();
		component.otpCodeFormControl.setValue('12345678');
		apiClient.validateSecretMessageOtp.and.returnValue(of({
			isValid: false,
			canRetry: false,
			hasExpired: false
		}));

		await component.submitOtpValidationForm();

		expect(component.timerActive()).toBeFalse();
		expect(component.otpCodeFormControl.errors).toEqual({
			otpInvalid: true,
			otpMaxAttemptsLimitReached: true
		});
		expect(modal.close).not.toHaveBeenCalled();
	});

	it('should allow another attempt for an invalid OTP when retries remain', async () => {
		await sendOtp();
		component.otpCodeFormControl.setValue('12345678');
		apiClient.validateSecretMessageOtp.and.returnValue(of({
			isValid: false,
			canRetry: true,
			hasExpired: false
		}));

		await component.submitOtpValidationForm();

		expect(component.timerActive()).toBeTrue();
		expect(component.otpCodeFormControl.errors).toEqual({ otpInvalid: true });
		expect(modal.close).not.toHaveBeenCalled();
	});

	it('should keep the modal open when the validation request fails', async () => {
		await sendOtp();
		component.otpCodeFormControl.setValue('12345678');
		apiClient.validateSecretMessageOtp.and.returnValue(throwError(() => new Error('failure')));

		await component.submitOtpValidationForm();

		expect(component.timerActive()).toBeTrue();
		expect(modal.close).not.toHaveBeenCalled();
	});

	it('should terminate the timer worker when destroyed', async () => {
		await sendOtp();
		const worker = FakeWorker.instances[0];

		fixture.destroy();

		expect(worker.terminate).toHaveBeenCalled();
		expect(component.timerActive()).toBeFalse();
	});

	it('should request a new OTP after the previous timer expired', async () => {
		await sendOtp();
		FakeWorker.instances[0].emit({ timerRemainingSeconds: 0, timerExpirationTime: '00:00' });
		fixture.detectChanges();
		expect(component.timerActive()).toBeFalse();
		apiClient.acquireSecretMessageOtp.calls.reset();

		await component.getOtp();
		fixture.detectChanges();

		expect(apiClient.acquireSecretMessageOtp).toHaveBeenCalledWith(messageId, requestContext);
		expect(component.timerActive()).toBeTrue();
		expect(FakeWorker.instances.length).toBe(2);
	});

	it('should not start a second expiration timer when one is already active', async () => {
		await sendOtp();

		(component as unknown as { startExpirationTimer: () => void }).startExpirationTimer();

		expect(FakeWorker.instances.length).toBe(1);
		expect(component.timerActive()).toBeTrue();
	});
});
