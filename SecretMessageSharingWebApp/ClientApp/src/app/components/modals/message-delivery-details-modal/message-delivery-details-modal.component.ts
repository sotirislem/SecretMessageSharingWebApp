import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { SecretMessageDeliveryNotification } from '../../../models/api/secret-message-delivery-notification.model';

@Component({
	templateUrl: './message-delivery-details-modal.component.html',
	styleUrls: ['./message-delivery-details-modal.component.css']
})
export class MessageDeliveryDetailsModalComponent {

	@Input() messageDeliveryDetails: SecretMessageDeliveryNotification;
	@Input() isNotification: boolean;

	constructor(public modal: NgbActiveModal) { }

	dismiss() {
		this.modal.dismiss();
	}

}
