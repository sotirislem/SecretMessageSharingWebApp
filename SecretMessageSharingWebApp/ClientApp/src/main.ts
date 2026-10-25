import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, withInMemoryScrolling } from '@angular/router';

import { AppRootComponent } from './app/components/app-root/app-root.component';
import { httpLoaderInterceptor } from './app/interceptors/http-loader-interceptor.service';
import { httpHeadersInterceptor } from './app/interceptors/http-headers-interceptor.service';
import { httpErrorInterceptor } from './app/interceptors/http-error-interceptor.service';
import { AppRoutes } from './app/app.routes';
import { API_URL, BASE_URL, CLIENT_ID, TAB_ID } from './app/tokens';
import { environment } from './environments/environment';
import { provideAppInsights } from './insights/app-insights.providers';

import './extensions/string.extensions';

if (environment.production) {
	window.console.log = function () { };
	window.console.warn = function () { };
	window.console.error = function () { };
}

const appConfig: ApplicationConfig = {
	providers: [
		provideBrowserGlobalErrorListeners(),
		provideZonelessChangeDetection(),
		{ provide: BASE_URL, useFactory: getBaseUrl },
		{ provide: API_URL, useFactory: getApiUrl },
		{ provide: CLIENT_ID, useFactory: getClientId },
		{ provide: TAB_ID, useFactory: getTabId },
		provideHttpClient(withInterceptors([
			httpLoaderInterceptor,
			httpHeadersInterceptor,
			httpErrorInterceptor
		])),
		provideRouter(AppRoutes, withInMemoryScrolling({ scrollPositionRestoration: 'enabled' })),
		...(environment.production && environment.applicationInsights.enable ? provideAppInsights() : [])
	]
};

bootstrapApplication(AppRootComponent, appConfig).catch(err => console.error(err));

// providers => Factory functions
function getBaseUrl(): string {
	const baseUrl = document.getElementsByTagName('base')[0].href;
	return trimUrlEndSlash(baseUrl);
}

function getApiUrl(): string {
	const apiUrl = environment.apiUrl || getBaseUrl();
	return trimUrlEndSlash(apiUrl);
}

function getClientId(): string {
	let clientId = localStorage.getItem('CLIENT_ID');

	if (!clientId) {
		clientId = crypto.randomUUID();
		localStorage.setItem('CLIENT_ID', clientId);
	}

	return clientId;
}

/**
 * Identifies this browser tab. Unlike the client id it is stored in sessionStorage, so it
 * survives a refresh but is not shared with other tabs. The server uses it to deliver a
 * notification about a self-read only to the tab that created the message.
 */
function getTabId(): string {
	let tabId = sessionStorage.getItem('TAB_ID');

	if (!tabId) {
		tabId = crypto.randomUUID();
		sessionStorage.setItem('TAB_ID', tabId);
	}

	return tabId;
}

// Helper functions
function trimUrlEndSlash(url: string): string {
	if (url.slice(-1) == '/') {
		return url.slice(0, -1);
	}
	return url;
}
