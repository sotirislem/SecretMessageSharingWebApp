/***************************************************************************************************
 * Load `$localize` onto the global scope - used if i18n tags appear in Angular templates.
 */
import { enableProdMode, importProvidersFrom } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { AppRootComponent } from './app/components/app-root/app-root.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ToastrModule } from 'ngx-toastr';
import { DigitOnlyDirective } from '@uiowa/digit-only';
import { httpLoaderInterceptor } from './app/interceptors/http-loader-interceptor.service';
import { httpHeadersInterceptor } from './app/interceptors/http-headers-interceptor.service';
import { httpErrorInterceptor } from './app/interceptors/http-error-interceptor.service';
import { environment } from './environments/environment';
import { AppInsightsModule } from './insights/app-insights.module';
import { AppRoutes } from './app/app.routes';

import './extensions/string.extensions';

const appInsightsModule = (environment.production && environment.applicationInsights.enable ? [importProvidersFrom(AppInsightsModule)] : []);

if (environment.production) {
	enableProdMode();

	window.console.log = function () { };
	window.console.warn = function () { };
	window.console.error = function () { };
}

bootstrapApplication(AppRootComponent, {
	providers: [
		{ provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
		{ provide: 'API_URL', useFactory: getApiUrl, deps: [] },
		{ provide: 'CLIENT_ID', useFactory: getClientId, deps: [] },
		provideHttpClient(withInterceptors([
			httpLoaderInterceptor,
			httpHeadersInterceptor,
			httpErrorInterceptor
		])),
		provideAnimations(),
		provideRouter(AppRoutes, withInMemoryScrolling({ scrollPositionRestoration: 'enabled' })),
		importProvidersFrom(
			NgbModule,
			ToastrModule.forRoot()
		),
		DigitOnlyDirective,
		...appInsightsModule
	]
}).catch(err => console.log(err));


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

// Helper functions
function trimUrlEndSlash(url: string): string {
	if (url.slice(-1) == '/') {
		return url.slice(0, -1);
	}
	return url;
}
