import { Component } from '@angular/core';
import { RecentlyStoredSecretMessage } from '../../models/api/recently-stored-secret-messages-response.model';
import { SecretMessageDeliveryNotification } from '../../models/api/secret-message-delivery-notification.model';
import { ApiClientService } from '../../services/api-client.service';
import { MessageDeliveryDetailsModalComponent } from '../modals/message-delivery-details-modal/message-delivery-details-modal.component';
import { ModalService } from '../../services/modal.service';

@Component({
	templateUrl: './recently-stored-messages.component.html',
	styleUrls: ['./recently-stored-messages.component.css']
})
export class RecentlyStoredMessagesComponent {
	recentlyStoredSecretMessages: RecentlyStoredSecretMessage[];

	constructor(
		private apiClient: ApiClientService,
		private modalService: ModalService
	) {
		this.apiClient.getRecentlyStoredSecretMessages().subscribe((response) => {
			this.recentlyStoredSecretMessages = response.recentlyStoredSecretMessages;
		});
	}

	showDeliveryDetails(msg: RecentlyStoredSecretMessage) {
		const messageDeliveryDetails = <SecretMessageDeliveryNotification>{
			messageId: msg.id,
			messageCreatedOn: msg.createdDateTime,
			messageDeliveredOn: msg.deliveryDetails!.deliveredAt,
			recipientIP: msg.deliveryDetails!.recipientIP,
			recipientClientInfo: msg.deliveryDetails!.recipientClientInfo
		};

		this.modalService.openModal(MessageDeliveryDetailsModalComponent, { isNotification: false, messageDeliveryDetails });
	}

	deleteStoredMessage(msg: RecentlyStoredSecretMessage) {
		var deleteConfirmation = confirm(`Are you sure you want delete stored SecretMessage with Id: '${msg.id}' ?`);
		if (!deleteConfirmation) return;

		this.apiClient.deleteRecentlyStoredSecretMessage(msg.id).subscribe(() => {
			this.recentlyStoredSecretMessages = this.recentlyStoredSecretMessages.filter(o => o !== msg);
		});
	}
}
