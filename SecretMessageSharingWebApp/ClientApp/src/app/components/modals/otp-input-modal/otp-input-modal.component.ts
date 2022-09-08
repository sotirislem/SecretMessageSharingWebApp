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

	timer: NodeJS.Timer;
	timerExpirationTime: string;
	timerRemainingSeconds: number = NaN;

	otpValidationForm = this.formBuilder.group({
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

	get timerActive(): boolean { return !isNaN(this.timerRemainingSeconds) }
	get timerFinished(): boolean { return this.timerRemainingSeconds == 0 }
	get timerRunning(): boolean { return this.timerActive && !this.timerFinished }

	getOtp() {
		if (this.otpSent && this.timerRunning) return;

		this.apiClientService.acquireSecretMessageOtp(this.messageId).subscribe((response) => {
			this.otpSent = response;

			if (this.otpSent) {
				this.startExpirationTimer();

				setTimeout(() => {
					this.form?.resetForm();
					this.otpCodeFormControlRef?.nativeElement.focus();
				});
			}
		});
	}

	submitOtpValidationForm() {
		if (this.otpValidationForm.invalid)
			return;

		if (this.otpCode.length === 0) {
			this.otpCodeFormControl.setErrors({ required: true });
			return;
		}

		if (this.otpCode.length !== 8) {
			this.otpCodeFormControl.setErrors({ minlength: true });
			return;
		}

		this.apiClientService.validateSecretMessageOtp(this.messageId, this.otpCode).subscribe((response: ValidateSecretMessageOtpResponse) => {
			if (response.isValid) {
				this.modal.close(response.token);
			}
			else if (!response.canRetry) {
				this.stopExpirationTimer();
				this.otpCodeFormControl.setErrors({ otpInvalid: true, otpMaxAttemptsLimitReached: true });
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
		if (this.timerRunning) return;

		this.timerRemainingSeconds = Constants.OTP_EXPIRATION_MINUTES * 60;

		const timerFunc = () => {
			--this.timerRemainingSeconds;

			const m = Math.floor(this.timerRemainingSeconds / 60);
			const s = this.timerRemainingSeconds - (m * 60);

			const minutes = m < 10 ? "0" + m : m;
			const seconds = s < 10 ? "0" + s : s;

			this.timerExpirationTime = minutes + ":" + seconds;

			if (this.timerRemainingSeconds == 0) {
				clearInterval(this.timer);
				!this.otpCodeFormControl.hasError('otpMaxAttemptsLimitReached') && this.otpCodeFormControl.setErrors({ otpHasExpired: true });
			}
		};

		timerFunc();
		this.timer = setInterval(timerFunc, 1000);
	}

	private stopExpirationTimer() {
		if (!this.timerRunning) return;

		clearInterval(this.timer);
		this.timerRemainingSeconds = 0;
	}

	private otpHasExpiredValidator(): ValidationErrors | null {
		const otpHasExpired = this.timerFinished;
		return otpHasExpired ? { otpHasExpired: true } : null;
	}
}
