/// <reference lib="webworker" />

import * as signalR from "@microsoft/signalr";
import { SecretMessageDeliveryNotification } from "../models/api/secret-message-delivery-notification.model";


var hubConnection: signalR.HubConnection;
const hubMethodName: string = "message-delivery-notification";
const hubUrl: string = "signalr/secret-message-delivery-notification-hub";

addEventListener('message', ({ data }) => {
	switch (data.action) {
		case 'START':
			createHubConnection(data.apiUrl);
			startHubConnection();
			break;
		case 'STOP':
			stopHubConnection();
			break;
		default:
			throw Error('Unknown action');
	}
});

function createHubConnection(apiUrl: string) {
	const url = `${apiUrl}/${hubUrl}`;

	hubConnection = new signalR.HubConnectionBuilder()
		.withUrl(url)
		.build();

	registerHandler();
}

function registerHandler() {
	hubConnection.on(hubMethodName, (response: SecretMessageDeliveryNotification) => {
		postMessage({ action: 'NOTIFICATION', secretMessageDeliveryNotification: response });
	});
}

async function startHubConnection() {
	await hubConnection
		.start()
		.then(() => console.log('SignalR: Connection started, ID:', hubConnection?.connectionId))
		.catch(err => console.error('SignalR: Error while starting connection: ' + err));

	postMessage({ action: 'CONNECTED', connectionId: hubConnection.connectionId });
}

async function stopHubConnection() {
	await hubConnection
		.stop()
		.then(() => console.log('SignalR: Connection terminated'));
}
