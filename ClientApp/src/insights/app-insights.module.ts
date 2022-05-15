import { ErrorHandler, NgModule } from '@angular/core';
import { ApplicationinsightsAngularpluginErrorService } from '@microsoft/applicationinsights-angularplugin-js';
import { AppInsightsService } from './app-insights.service';

@NgModule({
	providers: [
		AppInsightsService,
		{
			provide: ErrorHandler,
			useClass: ApplicationinsightsAngularpluginErrorService,
		}
	]
})
export class AppInsightsModule {
	constructor(appInsights: AppInsightsService) { }
}
