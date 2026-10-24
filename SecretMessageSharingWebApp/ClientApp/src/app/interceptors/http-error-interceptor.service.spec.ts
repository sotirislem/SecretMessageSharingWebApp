import { HttpErrorResponse, HttpRequest, HttpResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { ToastService } from '../services/toast.service';
import { httpErrorInterceptor } from './http-error-interceptor.service';

describe('httpErrorInterceptor', () => {
	let toastService: jasmine.SpyObj<ToastService>;

	beforeEach(() => {
		toastService = jasmine.createSpyObj<ToastService>('ToastService', ['success', 'error', 'warning', 'info', 'dismiss', 'clear']);

		TestBed.configureTestingModule({
			providers: [{ provide: ToastService, useValue: toastService }]
		});
	});

	function run(errorResponse: HttpErrorResponse): { error: HttpErrorResponse | undefined } {
		const request = new HttpRequest('GET', '/api/secret-messages');
		const next = jasmine.createSpy('next').and.returnValue(throwError(() => errorResponse));
		const result: { error: HttpErrorResponse | undefined } = { error: undefined };

		TestBed.runInInjectionContext(() => httpErrorInterceptor(request, next))
			.subscribe({ error: error => result.error = error });

		return result;
	}

	it('should pass successful responses through', () => {
		const request = new HttpRequest('GET', '/api/secret-messages');
		const next = jasmine.createSpy('next').and.returnValue(of(new HttpResponse({ status: 200 })));
		let response: HttpResponse<unknown> | undefined;

		TestBed.runInInjectionContext(() => httpErrorInterceptor(request, next))
			.subscribe(value => response = value as HttpResponse<unknown>);

		expect(response?.status).toBe(200);
		expect(toastService.error).not.toHaveBeenCalled();
	});

	it('should show a generic error for a plain string body', () => {
		const errorResponse = new HttpErrorResponse({
			status: 503,
			statusText: 'Service Unavailable',
			error: 'The service is down',
			url: '/api/secret-messages'
		});

		const result = run(errorResponse);

		expect(result.error).toBe(errorResponse);
		expect(toastService.error).toHaveBeenCalledWith(
			'Status Code: 503\n\nThe service is down',
			'API Error',
			{ timeOut: 15_000, closeButton: true });
	});

	it('should use the bad-request message and validation errors', () => {
		const errorResponse = new HttpErrorResponse({
			status: 400,
			error: {
				statusCode: 400,
				message: 'Validation failed',
				errors: {
					Message: ['Message is required', 'Message is too long'],
					Email: ['Email is invalid']
				}
			}
		});

		run(errorResponse);

		expect(toastService.error).toHaveBeenCalledWith(
			'Validation failed\n\nMessage: Message is required\nMessage: Message is too long\nEmail: Email is invalid',
			'API Error',
			{ timeOut: 15_000, closeButton: true });
	});

	it('should fall back to the status code for a bad request without a message', () => {
		run(new HttpErrorResponse({ status: 400, error: { statusCode: 400, errors: {} } }));

		expect(toastService.error).toHaveBeenCalledWith(
			'Status Code: 400',
			'API Error',
			{ timeOut: 15_000, closeButton: true });
	});

	it('should use the internal error status and reason', () => {
		run(new HttpErrorResponse({
			status: 500,
			error: {
				status: 'Internal Server Error',
				code: 500,
				reason: 'An unexpected error occurred',
				note: 'Contact support'
			}
		}));

		expect(toastService.error).toHaveBeenCalledWith(
			'An unexpected error occurred',
			'Internal Server Error',
			{ timeOut: 15_000, closeButton: true });
	});

	it('should fall back to the default title and message for an empty internal error', () => {
		run(new HttpErrorResponse({ status: 500, error: null }));

		expect(toastService.error).toHaveBeenCalledWith(
			'Status Code: 500',
			'API Error',
			{ timeOut: 15_000, closeButton: true });
	});

	it('should fall back to the status code for any other status', () => {
		run(new HttpErrorResponse({ status: 404, error: { unexpected: true } }));

		expect(toastService.error).toHaveBeenCalledWith(
			'Status Code: 404',
			'API Error',
			{ timeOut: 15_000, closeButton: true });
	});

	it('should ignore validation error entries without messages', () => {
		run(new HttpErrorResponse({
			status: 400,
			error: {
				statusCode: 400,
				message: 'Validation failed',
				errors: { Message: null }
			}
		}));

		expect(toastService.error).toHaveBeenCalledWith(
			'Validation failed',
			'API Error',
			{ timeOut: 15_000, closeButton: true });
	});

	it('should fall back to the default title and message for an internal error without details', () => {
		run(new HttpErrorResponse({ status: 500, error: { code: 500 } }));

		expect(toastService.error).toHaveBeenCalledWith(
			'Status Code: 500',
			'API Error',
			{ timeOut: 15_000, closeButton: true });
	});
});
