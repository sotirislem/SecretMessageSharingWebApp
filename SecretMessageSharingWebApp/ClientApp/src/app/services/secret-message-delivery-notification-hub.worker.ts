/// <reference lib="webworker" />

import * as signalR from "@microsoft/signalr";
import { SecretMessageDeliveryNotification } from "../models/api/secret-message-delivery-notification.model";


var hubConnection: signalR.HubConnection;
const hubMethodName: string = "message-delivery-notification";
const hubUrl: string = "signalr/secret-message-delivery-notification-hub";

addEventListener('message', ({ data }) => {
	switch (data.action) {
		case 'INIT_START':
			createHubConnection(data.apiUrl, data.clientId);
			startHubConnection();
			break;
		case 'START':
			startHubConnection();
			break;
		case 'STOP':
			stopHubConnection();
			break;
		default:
			throw Error('Unknown action');
	}
});

function createHubConnection(apiUrl: string, clientId: string) {
	const url = `${apiUrl}/${hubUrl}?client_id=${clientId}`;

	const retryDelays: number[] = [0, 5, 10, 15, 30].map(d => d * 1000);

	hubConnection = new signalR.HubConnectionBuilder()
		.withUrl(url)
		.withAutomaticReconnect(retryDelays)
		.configureLogging(signalR.LogLevel.Warning)
		.build();

	hubConnection.keepAliveIntervalInMilliseconds = 7 * 1000;
	hubConnection.serverTimeoutInMilliseconds = 15 * 1000;

	registerHandlers();
}

function registerHandlers() {
	hubConnection.on(hubMethodName, (response: SecretMessageDeliveryNotification) => {
		postMessage({ action: 'NOTIFICATION', secretMessageDeliveryNotification: response });
	});

	hubConnection.onclose((error) => {
		console.warn('SignalR: Connection terminated');
		postMessage({ action: 'CONNECTION_ID', connectionId: hubConnection.connectionId });

		if (error) {
			postErrorMessage(error);
		}
	});
}

async function startHubConnection() {
	await hubConnection
		.start()
		.then(() => {
			console.log('SignalR: Connection started, ID:', hubConnection.connectionId);
		})
		.catch((error) => { postErrorMessage(error) });

	postMessage({ action: 'CONNECTION_ID', connectionId: hubConnection.connectionId });
}

async function stopHubConnection() {
	await hubConnection
		.stop()
		.catch((error) => { postErrorMessage(error) });
}

function postErrorMessage(error: any) {
	postMessage({ action: 'ERROR', error });
}
