import { ErrorHandler, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AngularPlugin } from '@microsoft/applicationinsights-angularplugin-js';
import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import { environment } from '../environments/environment';

@Injectable({
	providedIn: 'root'
})
export class AppInsightsService {
	private angularPlugin = new AngularPlugin();
	private appInsights!: ApplicationInsights;

	constructor(private router: Router) {
		this.appInsights = new ApplicationInsights({
			config: {
				instrumentationKey: environment.applicationInsights.instrumentationKey,
				extensions: [this.angularPlugin],
				extensionConfig: {
					[this.angularPlugin.identifier]: {
						router: this.router,
						errorServices: [new ErrorHandler()]
					}
				}
			}
		});
		this.appInsights.loadAppInsights();
	}

	// expose methods that can be used in components and services
	trackEvent(arg: any) { this.appInsights.trackEvent(arg); }
	trackTrace(arg: any) { this.appInsights.trackTrace(arg); }
	trackException(arg: any) { this.appInsights.trackException(arg); }
}
