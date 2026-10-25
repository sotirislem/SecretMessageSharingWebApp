/// <reference lib="webworker" />

import * as signalR from "@microsoft/signalr";
import { SecretMessageDeliveryNotification } from "../models/api/secret-message-delivery-notification.model";


var hubConnection: signalR.HubConnection | undefined;
var restartTimeoutId: ReturnType<typeof setTimeout> | undefined;
var isStopped = false;
var hasReportedConnectionError = false;
const hubMethodName: string = "message-delivery-notification";
const hubUrl: string = "signalr/secret-message-delivery-notification-hub";
const maxRetryDelayInMilliseconds: number = 30 * 1000;
const restartDelayInMilliseconds: number = 5 * 1000;

addEventListener('message', ({ data }) => {
	switch (data.action) {
		case 'INIT_START':
			createHubConnection(data.apiUrl, data.clientId, data.tabId);
			void startHubConnection();
			break;
		case 'START':
			void startHubConnection();
			break;
		case 'STOP':
			void stopHubConnection();
			break;
		default:
			throw Error('Unknown action');
	}
});

function createHubConnection(apiUrl: string, clientId: string, tabId: string) {
	// The tab id lets the server deliver a notification about the creator's own retrieval to
	// this tab only. It lives in sessionStorage, so it survives a refresh of this tab.
	const url = `${apiUrl}/${hubUrl}?client_id=${encodeURIComponent(clientId)}&tab_id=${encodeURIComponent(tabId)}`;

	hubConnection = new signalR.HubConnectionBuilder()
		.withUrl(url)
		// Retry forever with a capped exponential backoff. A fixed list of delays gives up after
		// about a minute, which leaves an open tab without notifications (e.g. after switching
		// apps on Android) until the page is hidden and shown again.
		.withAutomaticReconnect(new CappedBackoffRetryPolicy())
		.configureLogging(signalR.LogLevel.Warning)
		.build();

	hubConnection.keepAliveIntervalInMilliseconds = 10 * 1000;
	hubConnection.serverTimeoutInMilliseconds = 15 * 1000;

	registerHandlers();
}

class CappedBackoffRetryPolicy implements signalR.IRetryPolicy {
	nextRetryDelayInMilliseconds(retryContext: signalR.RetryContext): number {
		return Math.min(maxRetryDelayInMilliseconds, 1000 * Math.pow(2, retryContext.previousRetryCount));
	}
}

function registerHandlers() {
	if (!hubConnection) return;

	hubConnection.on(hubMethodName, (response: SecretMessageDeliveryNotification) => {
		postMessage({ action: 'NOTIFICATION', secretMessageDeliveryNotification: response });
	});

	hubConnection.onreconnecting(() => {
		postConnectionState('reconnecting');
	});

	hubConnection.onreconnected(() => {
		hasReportedConnectionError = false;
		postConnectionState('connected');
	});

	hubConnection.onclose(() => {
		postConnectionState('disconnected');
		scheduleRestart();
	});
}

async function startHubConnection() {
	if (!hubConnection) return;

	isStopped = false;
	clearRestartTimeout();

	if (hubConnection.state !== signalR.HubConnectionState.Disconnected) {
		return;
	}

	try {
		await hubConnection.start();
		hasReportedConnectionError = false;
		console.log('SignalR: Connection started, ID:', hubConnection.connectionId);
		postConnectionState('connected');
	} catch (error) {
		postConnectionError(error);
		scheduleRestart();
	}
}

async function stopHubConnection() {
	isStopped = true;
	clearRestartTimeout();

	if (!hubConnection) return;

	try {
		await hubConnection.stop();
	} catch (error) {
		postConnectionError(error);
	}
}

function scheduleRestart() {
	if (isStopped) return;

	clearRestartTimeout();
	restartTimeoutId = setTimeout(() => {
		restartTimeoutId = undefined;
		void startHubConnection();
	}, restartDelayInMilliseconds);
}

function clearRestartTimeout() {
	if (restartTimeoutId === undefined) return;

	clearTimeout(restartTimeoutId);
	restartTimeoutId = undefined;
}

function postConnectionState(state: 'connected' | 'reconnecting' | 'disconnected') {
	postMessage({
		action: 'CONNECTION_STATE',
		state,
		connectionId: state === 'connected' ? hubConnection?.connectionId ?? null : null
	});
}

function postConnectionError(error: unknown) {
	// Switching apps on Android can produce a burst of transient failures; report only the
	// first one per connection attempt so the user does not get a toast every few seconds.
	if (hasReportedConnectionError) return;

	hasReportedConnectionError = true;
	postMessage({ action: 'ERROR', error: error instanceof Error ? error.message : String(error) });
}
