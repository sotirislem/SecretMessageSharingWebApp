/***************************************************************************************************
 * Load `$localize` onto the global scope - used if i18n tags appear in Angular templates.
 */
import '@angular/localize/init';
import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

import './extensions/string.extensions';

const providers = [
	{ provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
	{ provide: 'API_URL', useFactory: getApiUrl, deps: [] },
	{ provide: 'CLIENT_ID', useFactory: getClientId, deps: [] }
];

if (environment.production) {
	enableProdMode();

	window.console.log = function () { };
	window.console.warn = function () { };
	window.console.error = function () { };
}

platformBrowserDynamic(providers).bootstrapModule(AppModule)
	.catch(err => console.log(err));


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
	return crypto.randomUUID();
}

// Helper functions
function trimUrlEndSlash(url: string): string {
	if (url.slice(-1) == '/') {
		return url.slice(0, -1);
	}
	return url;
}
