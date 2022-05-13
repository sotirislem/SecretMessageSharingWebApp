import { Inject, Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr"
import { MessageDeliveryNotification } from '../models/message-delivery-notification.model';

@Injectable({
	providedIn: 'root'
})
export class SignalRService {
	private readonly hubConnection: signalR.HubConnection;
	private readonly hubConnectionMethodName: string = "secret-message-delivery-notification";
	private readonly signalRUrl: string;

	constructor(@Inject('API_URL') apiUrl: string,) {
		this.signalRUrl = `${apiUrl}/signalR/secret-message-delivery-notification-hub`;

		this.hubConnection = new signalR.HubConnectionBuilder()
			.withUrl(this.signalRUrl)
			.build();
	}

	get hubConnectionInitialized(): boolean {
		return (this.getHubConnectionId() ? true : false);
	}

	async initHubConnection() {
		if (this.hubConnectionInitialized) return;

		await this.hubConnection
			.start()
			.then(() => console.log('SignalR: Connection started, ID:', this.hubConnection.connectionId))
			.catch(err => console.error('SignalR: Error while starting connection: ' + err));
	}

	registerNewSecretMessageDeliveryNotificationHandler(invokeMethod: (response: MessageDeliveryNotification) => void) {
		if (!this.hubConnectionInitialized) return;

		this.unregisterExistingHandlers();
		this.hubConnection.on(this.hubConnectionMethodName, invokeMethod);
	}

	unregisterExistingHandlers() {
		if (!this.hubConnectionInitialized) return;

		this.hubConnection.off(this.hubConnectionMethodName);
	}

	getHubConnectionId(): string | null {
		return this.hubConnection?.connectionId;
	}
}
