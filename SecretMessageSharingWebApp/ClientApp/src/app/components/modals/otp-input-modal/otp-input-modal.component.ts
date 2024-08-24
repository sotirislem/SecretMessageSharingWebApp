import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { FormBuilder, NgForm, ValidationErrors, Validators } from '@angular/forms';

import { Constants } from '../../../../constants';
import { ApiClientService } from '../../../services/api-client.service';

import { ValidateSecretMessageOtpResponse } from '../../../models/api/validate-secret-message-otp-response.model';

@Component({
	templateUrl: './otp-input-modal.component.html',
	styleUrls: ['./otp-input-modal.component.css']
})
export class OtpInputModalComponent {
	@Input() messageId: string;

	@ViewChild('form') form: NgForm;
	@ViewChild('otpCodeFormControlRef') otpCodeFormControlRef: ElementRef;

	otpSent: boolean;

	expirationTimerWorker: Worker | null = null;
	timerExpirationTime: string = '';

	otpValidationForm = this.formBuilder.nonNullable.group({
		otpCodeFormControl: ['',
			[this.otpHasExpiredValidator.bind(this)]
		]
	});

	constructor(
		public modal: NgbActiveModal,
		private formBuilder: FormBuilder,
		private apiClientService: ApiClientService)
	{ }

	get otpCodeFormControl() { return this.otpValidationForm.controls.otpCodeFormControl }
	get otpCode(): string { return this.otpCodeFormControl.value }

	get timerActive(): boolean { return this.expirationTimerWorker !== null }

	getOtp() {
		if (this.otpSent && this.timerActive) return;

		this.apiClientService.acquireSecretMessageOtp(this.messageId).subscribe(() => {
			this.otpSent = true;
			this.startExpirationTimer();

			setTimeout(() => {
				this.form?.resetForm();
				this.otpCodeFormControlRef?.nativeElement.focus();
			});
		});
	}

	submitOtpValidationForm() {
		if (this.otpValidationForm.invalid || !this.timerActive)
			return;

		if (!this.otpCode) {
			this.otpCodeFormControl.setErrors({ required: true });
			return;
		}

		if (this.otpCode.length !== 8) {
			this.otpCodeFormControl.setErrors({ minlength: true });
			return;
		}

		this.apiClientService.validateSecretMessageOtp(this.messageId, this.otpCode).subscribe((response: ValidateSecretMessageOtpResponse) => {
			if (response.isValid) {
				this.modal.close(response.authToken);
			}
			else if (response.hasExpired) {
				this.stopExpirationTimer();
				this.otpCodeFormControl.setErrors({ otpHasExpired: true });
			}
			else {
				this.otpCodeFormControl.setErrors({ otpInvalid: true });
			}
		});
	}

	private startExpirationTimer() {
		if (this.timerActive) return;

		this.timerExpirationTime = `0${Constants.OTP_EXPIRATION_MINUTES}:00`;

		this.expirationTimerWorker = new Worker(new URL('./expiration-timer.worker', import.meta.url));
		
		this.expirationTimerWorker.onmessage = ({ data }) => {
			const { timerRemainingSeconds, timerExpirationTime }: { timerRemainingSeconds: number; timerExpirationTime: string } = data;

			this.timerExpirationTime = timerExpirationTime;

			if (timerRemainingSeconds == 0) {
				this.stopExpirationTimer();
			}
		};
	}

	private stopExpirationTimer() {
		this.expirationTimerWorker?.terminate();
		this.expirationTimerWorker = null;
	}

	private otpHasExpiredValidator(): ValidationErrors | null {
		const otpHasExpired = !this.timerActive;
		return otpHasExpired ? { otpHasExpired: true } : null;
	}
}
