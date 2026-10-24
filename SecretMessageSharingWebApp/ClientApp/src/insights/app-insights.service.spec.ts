import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { ApplicationInsights, ITelemetryItem } from '@microsoft/applicationinsights-web';
import { EMPTY } from 'rxjs';

import { AppInsightsService } from './app-insights.service';

describe('AppInsightsService', () => {
	let service: AppInsightsService;
	let appInsights: ApplicationInsights;
	let trackEventSpy: jasmine.Spy;
	let trackTraceSpy: jasmine.Spy;
	let trackExceptionSpy: jasmine.Spy;

	beforeEach(() => {
		TestBed.configureTestingModule({
			providers: [{ provide: Router, useValue: { events: EMPTY } }]
		});

		service = TestBed.inject(AppInsightsService);
		appInsights = (service as unknown as { appInsights: ApplicationInsights }).appInsights;

		trackEventSpy = spyOn(appInsights, 'trackEvent');
		trackTraceSpy = spyOn(appInsights, 'trackTrace');
		trackExceptionSpy = spyOn(appInsights, 'trackException');
	});

	afterEach(() => {
		// The SDK starts background senders and timers; leaving them running makes
		// Karma hang in afterAll after the browser context is torn down.
		appInsights.unload(false);
	});

	function sanitize(data: Record<string, unknown>): Record<string, unknown> {
		const envelope = { data: { baseData: data } } as unknown as ITelemetryItem;
		const sanitizer = (service as unknown as {
			sanitizeTelemetryEnvelope: (envelope: ITelemetryItem) => void;
		}).sanitizeTelemetryEnvelope;

		sanitizer(envelope);

		return data;
	}

	it('should create the Application Insights client', () => {
		expect(appInsights).toBeInstanceOf(ApplicationInsights);
	});

	it('should remove the fragment and sensitive query parameters from a page URI', () => {
		const baseData = sanitize({
			uri: 'https://example.com/get-secret-message?id=message-id&keyHash=hash&key=key&page=1#fragment'
		});

		expect(baseData['uri']).toBe('https://example.com/get-secret-message?page=1');
	});

	it('should sanitize a page name that is an absolute URL', () => {
		const baseData = sanitize({
			name: 'https://example.com/get-secret-message?id=message-id&page=1#fragment'
		});

		expect(baseData['name']).toBe('https://example.com/get-secret-message?page=1');
	});

	it('should leave a non-URL page name untouched', () => {
		const baseData = sanitize({ name: 'Secret Message' });

		expect(baseData['name']).toBe('Secret Message');
	});

	it('should leave unrelated base data untouched', () => {
		const baseData = sanitize({ uri: 42, name: 'Secret Message', other: 'value' });

		expect(baseData).toEqual({ uri: 42, name: 'Secret Message', other: 'value' });
	});

	it('should ignore envelopes without base data', () => {
		const sanitizer = (service as unknown as {
			sanitizeTelemetryEnvelope: (envelope: ITelemetryItem) => void;
		}).sanitizeTelemetryEnvelope;

		expect(() => sanitizer({} as ITelemetryItem)).not.toThrow();
		expect(() => sanitizer({ data: {} } as ITelemetryItem)).not.toThrow();
	});

	it('should return the original value when the URL cannot be parsed', () => {
		const baseData = sanitize({ uri: 'http://[' });

		expect(baseData['uri']).toBe('http://[');
	});

	it('should forward telemetry calls to the SDK', () => {
		const event = { name: 'event' };
		const trace = { message: 'trace' };
		const exception = { exception: new Error('failure') };

		service.trackEvent(event);
		service.trackTrace(trace);
		service.trackException(exception);

		expect(trackEventSpy).toHaveBeenCalledWith(event);
		expect(trackTraceSpy).toHaveBeenCalledWith(trace);
		expect(trackExceptionSpy).toHaveBeenCalledWith(exception);
	});
});
