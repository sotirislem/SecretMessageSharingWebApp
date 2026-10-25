import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

import { SecretMessageDeliveryNotification } from '../../../models/api/secret-message-delivery-notification.model';
import { DateTimeFullPipe } from '../../../pipes/date-time-full.pipe';
import { MODAL_DATA } from '../../../tokens';

export interface MessageDeliveryDetailsModalData {
	messageDeliveryDetails: SecretMessageDeliveryNotification;
	isNotification: boolean;
}

@Component({
	templateUrl: './message-delivery-details-modal.component.html',
	styleUrl: './message-delivery-details-modal.component.css',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [DateTimeFullPipe]
})
export class MessageDeliveryDetailsModalComponent {
	readonly data = inject(MODAL_DATA) as MessageDeliveryDetailsModalData;

	readonly details = this.data.messageDeliveryDetails;

	readonly modal = inject(NgbActiveModal);

	get locationText(): string | null {
		const details = this.data.messageDeliveryDetails;
		const place = [details.recipientCity, details.recipientRegion, details.recipientCountry]
			.filter((part): part is string => !!part?.trim())
			.join(', ');

		if (!place) {
			return null;
		}

		const flag = this.getCountryFlag(details.recipientCountryCode);

		return flag ? `${flag} ${place}` : place;
	}

	get locationSourceText(): string | null {
		switch (this.data.messageDeliveryDetails.recipientLocationSource) {
			case 'device':
				return 'Precise (device location)';
			case 'ip':
				return 'Approximate (IP address)';
			default:
				return null;
		}
	}

	/**
	 * The row is always rendered so the sender knows whether the city was precise
	 * or approximate. Records created before location tracking simply say so.
	 */
	get locationDisplayText(): string {
		return this.locationText ?? (this.locationSourceText ? 'Unknown' : 'Not available');
	}

	get ispText(): string | null {
		return this.data.messageDeliveryDetails.recipientIsp?.trim() || null;
	}

	get timezoneText(): string | null {
		const details = this.data.messageDeliveryDetails;
		const timezone = details.recipientTimezone?.trim();

		if (details.recipientUtcOffsetMinutes === undefined || details.recipientUtcOffsetMinutes === null) {
			return timezone || null;
		}

		const offset = this.formatUtcOffset(details.recipientUtcOffsetMinutes);

		return timezone ? `${timezone} (${offset})` : offset;
	}

	get languageText(): string | null {
		const language = this.data.messageDeliveryDetails.recipientLanguage?.trim();
		if (!language) {
			return null;
		}

		try {
			const displayName = new Intl.DisplayNames([language], { type: 'language' }).of(language);

			return displayName && displayName !== language
				? `${displayName} (${language})`
				: language;
		} catch {
			return language;
		}
	}

	get timeToOpenText(): string | null {
		const details = this.data.messageDeliveryDetails;
		const createdOn = new Date(details.messageCreatedOn).getTime();
		const deliveredOn = new Date(details.messageDeliveredOn).getTime();

		if (!Number.isFinite(createdOn) || !Number.isFinite(deliveredOn) || deliveredOn < createdOn) {
			return null;
		}

		return this.formatDuration(deliveredOn - createdOn);
	}

	private formatUtcOffset(offsetMinutes: number): string {
		const sign = offsetMinutes < 0 ? '-' : '+';
		const absolute = Math.abs(Math.round(offsetMinutes));
		const hours = Math.floor(absolute / 60);
		const minutes = absolute % 60;

		return `UTC${sign}${hours}:${minutes.toString().padStart(2, '0')}`;
	}

	private formatDuration(durationMs: number): string {
		const totalSeconds = Math.round(durationMs / 1000);

		if (totalSeconds < 60) {
			return 'Less than a minute';
		}

		const totalMinutes = Math.floor(totalSeconds / 60);
		if (totalMinutes < 60) {
			return this.pluralize(totalMinutes, 'minute');
		}

		const totalHours = Math.floor(totalMinutes / 60);
		if (totalHours < 24) {
			const minutes = totalMinutes % 60;

			return this.joinParts([
				this.pluralize(totalHours, 'hour'),
				minutes > 0 ? this.pluralize(minutes, 'minute') : null
			]);
		}

		const days = Math.floor(totalHours / 24);
		const hours = totalHours % 24;

		return this.joinParts([
			this.pluralize(days, 'day'),
			hours > 0 ? this.pluralize(hours, 'hour') : null
		]);
	}

	private pluralize(value: number, unit: string): string {
		return `${value} ${unit}${value === 1 ? '' : 's'}`;
	}

	private joinParts(parts: (string | null)[]): string {
		return parts.filter((part): part is string => !!part).join(' ');
	}

	private getCountryFlag(countryCode?: string | null): string | null {
		if (!countryCode || /^[a-z]{2}$/i.test(countryCode) === false) {
			return null;
		}

		const codePoints = [...countryCode.toUpperCase()]
			.map(character => 0x1F1E6 + character.charCodeAt(0) - 'A'.charCodeAt(0));

		return String.fromCodePoint(...codePoints);
	}

	dismiss(): void {
		this.modal.dismiss();
	}
}
