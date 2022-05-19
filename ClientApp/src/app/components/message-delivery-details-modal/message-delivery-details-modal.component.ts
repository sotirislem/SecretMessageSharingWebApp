import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { MessageDeliveryDetails } from '../../models/message-delivery-details.model';

@Component({
	selector: 'app-message-delivery-details-modal',
	templateUrl: './message-delivery-details-modal.component.html',
	styleUrls: ['./message-delivery-details-modal.component.css']
})
export class MessageDeliveryDetailsModalComponent {

	@Input() messageDeliveryDetails: MessageDeliveryDetails;
	@Input() isNotification: boolean;

	constructor(public modal: NgbActiveModal) { }

	dismiss() {
		this.modal.dismiss();
	}

}
