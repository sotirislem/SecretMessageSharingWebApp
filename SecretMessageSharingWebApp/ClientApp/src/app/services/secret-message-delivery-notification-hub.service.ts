import { DestroyRef, inject, Injectable, signal } from '@angular/core';
import { NgbModalOptions } from '@ng-bootstrap/ng-bootstrap';

import { ModalService } from './modal.service';
import { ToastService } from './toast.service';
import { MessageDeliveryDetailsModalComponent } from '../components/modals/message-delivery-details-modal/message-delivery-details-modal.component';
import { SecretMessageDeliveryNotification } from '../models/api/secret-message-delivery-notification.model';
import { API_URL, CLIENT_ID } from '../tokens';

@Injectable({
	providedIn: 'root'
})
export class SecretMessageDeliveryNotificationHubService {
	private readonly apiUrl = inject(API_URL);
	private readonly clientId = inject(CLIENT_ID);
	private readonly modalService = inject(ModalService);
	private readonly toastService = inject(ToastService);
	private readonly destroyRef = inject(DestroyRef);

	private readonly hubWorker = new Worker(new URL('./secret-message-delivery-notification-hub.worker', import.meta.url));

	private readonly _hubConnectionId = signal<string | null>(null);
	private readonly _deliveredMessageIds = signal<readonly string[]>([]);

	readonly deliveredMessageIds = this._deliveredMessageIds.asReadonly();

	constructor() {
		this.hubWorker.onmessage = ({ data }) => {
			switch (data.action) {
				case 'CONNECTION_ID':
					this._hubConnectionId.set(data.connectionId);
					break;
				case 'NOTIFICATION':
					this.handleIncomingNotification(data.secretMessageDeliveryNotification);
					break;
				case 'ERROR':
					this.toastService.error(String(data.error), 'SignalR');
					break;
				default:
					throw Error('Unknown action');
			}
		};

		this.hubWorker.postMessage({ action: 'INIT_START', apiUrl: this.apiUrl, clientId: this.clientId });
		document.addEventListener('visibilitychange', this.handleVisibilityChange);

		this.destroyRef.onDestroy(() => {
			document.removeEventListener('visibilitychange', this.handleVisibilityChange);
			this.hubWorker.terminate();
		});
	}

	private readonly handleVisibilityChange = () => {
		if (document.visibilityState === 'visible' && !this._hubConnectionId()) {
			this.hubWorker.postMessage({ action: 'START' });
		}
	};

	private handleIncomingNotification(secretMessageDeliveryNotification: SecretMessageDeliveryNotification): void {
		this._deliveredMessageIds.update(ids => [...ids, secretMessageDeliveryNotification.messageId]);

		this.modalService.openModal(
			MessageDeliveryDetailsModalComponent,
			{ isNotification: true, messageDeliveryDetails: secretMessageDeliveryNotification },
			<NgbModalOptions>{ centered: true }
		).catch(() => {
			// The notification can be dismissed with Escape or a backdrop click.
		});
	}
}
