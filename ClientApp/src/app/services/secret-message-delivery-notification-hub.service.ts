import { Inject, Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr"
import { MessageDeliveryNotification } from '../models/message-delivery-notification.model';

@Injectable({
	providedIn: 'root'
})
export class SecretMessageDeliveryNotificationHubService {
	private readonly hubConnection: signalR.HubConnection;
	private readonly hubUrl: string = "signalr/secret-message-delivery-notification-hub";
	private readonly hubMethodName: string = "message-delivery-notification";

	constructor(@Inject('API_URL') apiUrl: string) {
		const url = `${apiUrl}/${this.hubUrl}`;

		this.hubConnection = new signalR.HubConnectionBuilder()
			.withUrl(url)
			.build();
	}

	get hubConnectionInitialized(): boolean {
		return (this.hubConnectionId ? true : false);
	}

	get hubConnectionId(): string | null {
		return this.hubConnection?.connectionId;
	}

	async initHubConnection() {
		if (this.hubConnectionInitialized) return;

		await this.hubConnection
			.start()
			.then(() => console.log('SecretMessageDeliveryNotificationHubService: Connection started, ID:', this.hubConnectionId))
			.catch(err => console.error('SecretMessageDeliveryNotificationHubService: Error while starting connection: ' + err));
	}

	async terminateHubConnection() {
		if (!this.hubConnectionInitialized) return;

		await this.hubConnection
			.stop()
			.then(() => console.log('SecretMessageDeliveryNotificationHubService: Connection terminated'));
	}

	registerNewSecretMessageDeliveryNotificationHandler(invokeMethod: (response: MessageDeliveryNotification) => void) {
		if (!this.hubConnectionInitialized) return;

		this.unregisterExistingHandlers();

		this.hubConnection.on(this.hubMethodName, invokeMethod);
		this.hubConnection.on(this.hubMethodName, () => {
			this.unregisterExistingHandlers();
			this.terminateHubConnection();
		});
	}

	private unregisterExistingHandlers() {
		this.hubConnection.off(this.hubMethodName);
	}
}
