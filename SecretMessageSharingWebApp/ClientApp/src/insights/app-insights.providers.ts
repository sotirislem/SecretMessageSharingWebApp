import { EnvironmentProviders, ErrorHandler, inject, provideAppInitializer, Provider } from '@angular/core';
import { ApplicationinsightsAngularpluginErrorService } from '@microsoft/applicationinsights-angularplugin-js';

import { AppInsightsService } from './app-insights.service';

/**
 * Instantiates Application Insights eagerly (like the old NgModule did) and
 * routes uncaught Angular errors to it.
 */
export function provideAppInsights(): Array<Provider | EnvironmentProviders> {
	return [
		provideAppInitializer(() => {
			inject(AppInsightsService);
		}),
		{
			provide: ErrorHandler,
			useClass: ApplicationinsightsAngularpluginErrorService
		}
	];
}
