import { Inject, Injectable } from '@angular/core';
import { NgbModalOptions } from '@ng-bootstrap/ng-bootstrap';
import { Subject } from 'rxjs';

import { ModalService } from './modal.service';
import { MessageDeliveryDetailsModalComponent } from '../components/modals/message-delivery-details-modal/message-delivery-details-modal.component';
import { SecretMessageDeliveryNotification } from '../models/api/secret-message-delivery-notification.model';


@Injectable({
	providedIn: 'root'
})
export class SecretMessageDeliveryNotificationHubService {
	private readonly hubWorker: Worker;

	private _hubConnectionId: string | null;
	private receivedDeliveryNotifications: Subject<string> = new Subject();

	constructor(
		@Inject('API_URL') apiUrl: string,
		@Inject('CLIENT_ID') clientId: string,
		private modalService: ModalService)
	{
		this.hubWorker = new Worker(new URL('./secret-message-delivery-notification-hub.worker', import.meta.url));

		this.hubWorker.onmessage = ({ data }) => {
			switch (data.action) {
				case 'CONNECTION_ID':
					this._hubConnectionId = data.connectionId;
					break;
				case 'NOTIFICATION':
					this.handleIncomingNotification(data.secretMessageDeliveryNotification);
					break;
				case 'ERROR':
					alert('SignalR\r\n' + data.error);
					break;
				default:
					throw Error('Unknown action');
			}
		};

		this.hubWorker.postMessage({ action: 'INIT_START', apiUrl, clientId });
		this.addVisibilityChangeEventListener();
	}

	receivedDeliveryNotificationsObservable = this.receivedDeliveryNotifications.asObservable();

	get hubConnectionId(): string | null {
		return this._hubConnectionId;
	}

	private addVisibilityChangeEventListener() {
		const handleVisibilityChange = () => {
			if (document.visibilityState === "visible" && !this._hubConnectionId) {
				this.hubWorker.postMessage({ action: 'START' });
			}
		}

		document.addEventListener("visibilitychange", handleVisibilityChange, false);
	}

	private handleIncomingNotification(secretMessageDeliveryNotification: SecretMessageDeliveryNotification) {
		this.receivedDeliveryNotifications.next(secretMessageDeliveryNotification.messageId);

		this.modalService.openModal(
			MessageDeliveryDetailsModalComponent,
			{ isNotification: true, messageDeliveryDetails: secretMessageDeliveryNotification },
			<NgbModalOptions>{ centered: true }
		);
	}
}
