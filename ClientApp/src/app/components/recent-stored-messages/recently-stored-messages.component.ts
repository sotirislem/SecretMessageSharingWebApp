import { Component, OnInit } from '@angular/core';
import { RecentlyStoredMessage } from '../../models/recently-stored-message.model';
import { MessageDeliveryDetails } from '../../models/message-delivery-details.model';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ApiClientService } from '../../services/api-client.service';
import { MessageDeliveryDetailsModalComponent } from '../message-delivery-details-modal/message-delivery-details-modal.component';

@Component({
	selector: 'app-recently-stored-messages',
	templateUrl: './recently-stored-messages.component.html',
	styleUrls: ['./recently-stored-messages.component.css']
})
export class RecentlyStoredMessagesComponent {
	recentlyStoredMessages: RecentlyStoredMessage[];

	constructor(private apiClient: ApiClientService, private modalService: NgbModal) {
		this.apiClient.getRecentlyStoredMessages().subscribe((response) => {
			this.recentlyStoredMessages = response;
		});
	}

	showDeliveryDetails(msg: RecentlyStoredMessage) {
		const messageDeliveryDetails = <MessageDeliveryDetails>{
			messageId: msg.id,
			messageCreatedOn: msg.createdDateTime,
			messageDeliveredOn: msg.deliveryDetails!.deliveredAt,
			recipientIp: msg.deliveryDetails?.recipientIP,
			recipientClientInfo: msg.deliveryDetails?.recipientClientInfo
		};

		const modal = this.modalService.open(MessageDeliveryDetailsModalComponent);
		modal.componentInstance.messageDeliveryDetails = messageDeliveryDetails;
	}

	deleteStoredMessage(msg: RecentlyStoredMessage) {
		var deleteConfirmation = confirm(`Are you sure you want delete stored SecretMessage with Id: '${msg.id}' ?`);
		if (!deleteConfirmation) return;

		this.apiClient.deleteRecentlyStoredMessage(msg.id).subscribe((deleted) => {
			if (deleted) {
				this.recentlyStoredMessages = this.recentlyStoredMessages.filter(o => o !== msg);
			}
		});
	}
}
