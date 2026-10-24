import { ErrorHandler, inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AngularPlugin } from '@microsoft/applicationinsights-angularplugin-js';
import { ApplicationInsights, ITelemetryItem } from '@microsoft/applicationinsights-web';
import { environment } from '../environments/environment';

@Injectable({
	providedIn: 'root'
})
export class AppInsightsService {
	private static readonly sensitiveQueryParams = ['id', 'keyHash', 'key'];

	private readonly router = inject(Router);
	private readonly angularPlugin = new AngularPlugin();
	private appInsights!: ApplicationInsights;

	constructor() {
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

		// Page-view telemetry includes the URL, which contains the message id.
		// Strip the fragment and identifying query parameters before upload.
		this.appInsights.addTelemetryInitializer((envelope: ITelemetryItem) => {
			const baseData = envelope.data?.['baseData'] as Record<string, unknown> | undefined;
			if (!baseData) return;

			if (typeof baseData['uri'] === 'string') {
				baseData['uri'] = this.sanitizeUrl(baseData['uri']);
			}

			if (typeof baseData['name'] === 'string' && /^https?:\/\//i.test(baseData['name'])) {
				baseData['name'] = this.sanitizeUrl(baseData['name']);
			}
		});

		this.appInsights.loadAppInsights();
	}

	private sanitizeUrl(url: string): string {
		try {
			const parsed = new URL(url, window.location.origin);
			parsed.hash = '';

			for (const param of AppInsightsService.sensitiveQueryParams) {
				parsed.searchParams.delete(param);
			}

			return parsed.toString();
		} catch {
			return url;
		}
	}

	// expose methods that can be used in components and services
	trackEvent(arg: any) { this.appInsights.trackEvent(arg); }
	trackTrace(arg: any) { this.appInsights.trackTrace(arg); }
	trackException(arg: any) { this.appInsights.trackException(arg); }
}
