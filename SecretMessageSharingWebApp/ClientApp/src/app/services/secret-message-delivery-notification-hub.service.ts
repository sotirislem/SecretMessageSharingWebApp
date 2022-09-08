import { Inject, Injectable } from '@angular/core';
import { SecretMessageDeliveryNotification } from '../models/api/secret-message-delivery-notification.model';
import * as signalR from "@microsoft/signalr";
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { MessageDeliveryDetailsModalComponent } from '../components/modals/message-delivery-details-modal/message-delivery-details-modal.component';
import { Subject } from 'rxjs';

@Injectable({
	providedIn: 'root'
})
export class SecretMessageDeliveryNotificationHubService {
	private readonly hubConnection: signalR.HubConnection;
	private readonly hubUrl: string = "signalr/secret-message-delivery-notification-hub";
	private readonly hubMethodName: string = "message-delivery-notification";

	private pendingNotifications: number = 0;
	private receivedDeliveryNotifications: Subject<string> = new Subject();

	receivedDeliveryNotificationsObservable = this.receivedDeliveryNotifications.asObservable();

	constructor(@Inject('API_URL') apiUrl: string, private modalService: NgbModal) {
		const url = `${apiUrl}/${this.hubUrl}`;

		this.hubConnection = new signalR.HubConnectionBuilder()
			.withUrl(url)
			.build();

		this.registerHandler();
	}

	get hubConnectionInitialized(): boolean {
		return (this.hubConnectionId ? true : false);
	}

	get hubConnectionId(): string | null {
		return this.hubConnection?.connectionId;
	}

	async initHubConnection() {
		this.pendingNotifications++;

		if (this.hubConnectionInitialized) return;

		await this.hubConnection
			.start()
			.then(() => console.log('SecretMessageDeliveryNotificationHubService: Connection started, ID:', this.hubConnectionId))
			.catch(err => console.error('SecretMessageDeliveryNotificationHubService: Error while starting connection: ' + err));
	}

	private registerHandler() {
		this.hubConnection.on(this.hubMethodName, (response: SecretMessageDeliveryNotification) => {
			this.receivedDeliveryNotifications.next(response.messageId);

			const modal = this.modalService.open(MessageDeliveryDetailsModalComponent, { centered: true });
			modal.componentInstance.messageDeliveryDetails = response;
			modal.componentInstance.isNotification = true;

			if (--this.pendingNotifications == 0) {
				this.terminateHubConnection();
			}
		});
	}

	private async terminateHubConnection() {
		if (!this.hubConnectionInitialized) return;

		await this.hubConnection
			.stop()
			.then(() => console.log('SecretMessageDeliveryNotificationHubService: Connection terminated'));
	}
}
