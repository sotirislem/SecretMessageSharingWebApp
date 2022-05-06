/***************************************************************************************************
 * Load `$localize` onto the global scope - used if i18n tags appear in Angular templates.
 */
import '@angular/localize/init';
import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

export function getBaseUrl():string {
	let baseUrl = document.getElementsByTagName('base')[0].href;
	if (baseUrl.slice(-1) == '/') {
		baseUrl = baseUrl.slice(0, -1);
	}
	return baseUrl;
}

const providers = [
	{ provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }
];

if (environment.production) {
	enableProdMode();
}

platformBrowserDynamic(providers).bootstrapModule(AppModule)
	.catch(err => console.log(err));
