import { HttpHeaders, HttpRequest, HttpResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { CLIENT_ID } from '../tokens';
import { httpHeadersInterceptor } from './http-headers-interceptor.service';

describe('httpHeadersInterceptor', () => {
	beforeEach(() => {
		TestBed.configureTestingModule({
			providers: [{ provide: CLIENT_ID, useValue: 'client-1' }]
		});
	});

	it('should add the client id header to the request', () => {
		const request = new HttpRequest('GET', '/api/secret-messages');
		const next = jasmine.createSpy('next').and.returnValue(of(new HttpResponse({ status: 200 })));

		TestBed.runInInjectionContext(() => httpHeadersInterceptor(request, next)).subscribe();

		expect(next).toHaveBeenCalledTimes(1);
		const forwardedRequest = next.calls.mostRecent().args[0] as HttpRequest<unknown>;
		expect(forwardedRequest.headers.get('Client-Id')).toBe('client-1');
	});

	it('should not mutate the original request', () => {
		const request = new HttpRequest('GET', '/api/secret-messages');
		const next = jasmine.createSpy('next').and.returnValue(of(new HttpResponse({ status: 200 })));

		TestBed.runInInjectionContext(() => httpHeadersInterceptor(request, next)).subscribe();

		expect(request.headers.has('Client-Id')).toBeFalse();
	});

	it('should preserve existing headers', () => {
		const request = new HttpRequest('GET', '/api/secret-messages', {
			headers: new HttpHeaders().set('X-Key-Hash', 'hash')
		});
		const next = jasmine.createSpy('next').and.returnValue(of(new HttpResponse({ status: 200 })));

		TestBed.runInInjectionContext(() => httpHeadersInterceptor(request, next)).subscribe();

		const forwardedRequest = next.calls.mostRecent().args[0] as HttpRequest<unknown>;
		expect(forwardedRequest.headers.get('Client-Id')).toBe('client-1');
		expect(forwardedRequest.headers.get('X-Key-Hash')).toBe('hash');
	});
});
