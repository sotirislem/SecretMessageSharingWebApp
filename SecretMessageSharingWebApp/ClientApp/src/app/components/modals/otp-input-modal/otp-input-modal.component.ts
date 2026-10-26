import { ChangeDetectionStrategy, Component, DestroyRef, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormGroupDirective, NonNullableFormBuilder, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { firstValueFrom } from 'rxjs';
import { DigitOnlyDirective } from '@uiowa/digit-only';

import { Constants } from '../../../../constants';
import { ApiClientService } from '../../../services/api-client.service';
import { RecipientLocationService } from '../../../services/recipient-location.service';
import { ValidateSecretMessageOtpResponse } from '../../../models/api/validate-secret-message-otp-response.model';
import { MODAL_DATA } from '../../../tokens';

export interface OtpInputModalData {
	messageId: string;
}

@Component({
	templateUrl: './otp-input-modal.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [ReactiveFormsModule, DigitOnlyDirective]
})
export class OtpInputModalComponent {
	readonly data = inject(MODAL_DATA) as OtpInputModalData;

	readonly modal = inject(NgbActiveModal);

	private readonly formBuilder = inject(NonNullableFormBuilder);
	private readonly apiClientService = inject(ApiClientService);
	private readonly recipientLocationService = inject(RecipientLocationService);
	private readonly destroyRef = inject(DestroyRef);

	private readonly form = viewChild<FormGroupDirective>('form');
	private readonly otpCodeFormControlRef = viewChild<ElementRef<HTMLInputElement>>('otpCodeFormControlRef');

	readonly otpSent = signal(false);
	readonly timerActive = signal(false);
	readonly timerExpirationTime = signal('');

	readonly otpValidationForm = this.formBuilder.group({
		otpCodeFormControl: ['', [this.otpHasExpiredValidator.bind(this)]]
	});

	private expirationTimerWorker: Worker | null = null;

	constructor() {
		this.destroyRef.onDestroy(() => this.stopExpirationTimer());
	}

	get otpCodeFormControl() { return this.otpValidationForm.controls.otpCodeFormControl; }
	get otpCode(): string { return this.otpCodeFormControl.value; }

	async getOtp(): Promise<void> {
		if (this.otpSent() && this.timerActive()) return;

		try {
			await firstValueFrom(this.apiClientService.acquireSecretMessageOtp(
				this.data.messageId,
				this.recipientLocationService.collectRequestContext()));

			this.otpSent.set(true);
			this.startExpirationTimer();

			setTimeout(() => {
				this.form()?.resetForm();
				this.otpCodeFormControlRef()?.nativeElement.focus();
			});
		} catch {
			// The error interceptor already reported the failure.
		}
	}

	async submitOtpValidationForm(): Promise<void> {
		if (this.otpValidationForm.invalid || !this.timerActive()) return;

		if (!this.otpCode) {
			this.otpCodeFormControl.setErrors({ required: true });
			return;
		}

		if (this.otpCode.length !== 8) {
			this.otpCodeFormControl.setErrors({ minlength: true });
			return;
		}

		try {
			const response: ValidateSecretMessageOtpResponse = await firstValueFrom(
				this.apiClientService.validateSecretMessageOtp(this.data.messageId, this.otpCode));

			if (response.isValid) {
				this.modal.close(response.authToken);
			} else if (response.hasExpired) {
				this.stopExpirationTimer();
				this.otpCodeFormControl.setErrors({ otpHasExpired: true });
			} else if (!response.canRetry) {
				this.stopExpirationTimer();
				this.otpCodeFormControl.setErrors({ otpInvalid: true, otpMaxAttemptsLimitReached: true });
			} else {
				this.otpCodeFormControl.setErrors({ otpInvalid: true });
			}
		} catch {
			// The error interceptor already reported the failure.
		}
	}

	private startExpirationTimer(): void {
		if (this.timerActive()) return;

		this.timerActive.set(true);
		this.timerExpirationTime.set(`0${Constants.OTP_EXPIRATION_MINUTES}:00`);

		this.expirationTimerWorker = new Worker(new URL('./expiration-timer.worker', import.meta.url));

		this.expirationTimerWorker.onmessage = ({ data }) => {
			const { timerRemainingSeconds, timerExpirationTime }: { timerRemainingSeconds: number; timerExpirationTime: string } = data;

			this.timerExpirationTime.set(timerExpirationTime);

			if (timerRemainingSeconds == 0) {
				this.stopExpirationTimer();
			}
		};
	}

	private stopExpirationTimer(): void {
		this.expirationTimerWorker?.terminate();
		this.expirationTimerWorker = null;
		this.timerActive.set(false);
	}

	private otpHasExpiredValidator(): ValidationErrors | null {
		return this.timerActive() ? null : { otpHasExpired: true };
	}
}
