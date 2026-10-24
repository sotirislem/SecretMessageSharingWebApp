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

	readonly modal = inject(NgbActiveModal);

	dismiss(): void {
		this.modal.dismiss();
	}
}
