import { HttpHeaders, HttpRequest, HttpResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { LoaderService } from '../services/loader.service';
import { httpLoaderInterceptor } from './http-loader-interceptor.service';

describe('httpLoaderInterceptor', () => {
	let loaderService: jasmine.SpyObj<LoaderService>;

	beforeEach(() => {
		loaderService = jasmine.createSpyObj<LoaderService>('LoaderService', ['enableLoading', 'disableLoading']);

		TestBed.configureTestingModule({
			providers: [{ provide: LoaderService, useValue: loaderService }]
		});
	});

	function run(request: HttpRequest<unknown>, next: jasmine.Spy) {
		return TestBed.runInInjectionContext(() => httpLoaderInterceptor(request, next));
	}

	it('should show the loader while the request is running and hide it afterwards', () => {
		const request = new HttpRequest('GET', '/api/secret-messages');
		const next = jasmine.createSpy('next').and.returnValue(of(new HttpResponse({ status: 200 })));

		run(request, next).subscribe();

		expect(loaderService.enableLoading).toHaveBeenCalledTimes(1);
		expect(loaderService.disableLoading).toHaveBeenCalledTimes(1);
	});

	it('should hide the loader when the request fails', () => {
		const request = new HttpRequest('GET', '/api/secret-messages');
		const next = jasmine.createSpy('next').and.returnValue(throwError(() => new Error('failure')));

		run(request, next).subscribe({ error: () => { } });

		expect(loaderService.enableLoading).toHaveBeenCalledTimes(1);
		expect(loaderService.disableLoading).toHaveBeenCalledTimes(1);
	});

	it('should skip the loader for SignalR requests', () => {
		const request = new HttpRequest('POST', '/signalr/negotiate', null);
		const next = jasmine.createSpy('next').and.returnValue(of(new HttpResponse({ status: 200 })));

		run(request, next).subscribe();

		expect(next).toHaveBeenCalledWith(request);
		expect(loaderService.enableLoading).not.toHaveBeenCalled();
		expect(loaderService.disableLoading).not.toHaveBeenCalled();
	});

	it('should skip the loader when the request asks for it', () => {
		const request = new HttpRequest('GET', '/api/secret-messages', {
			headers: new HttpHeaders().set('X-Skip-Loader', 'true')
		});
		const next = jasmine.createSpy('next').and.returnValue(of(new HttpResponse({ status: 200 })));

		run(request, next).subscribe();

		expect(next).toHaveBeenCalledWith(request);
		expect(loaderService.enableLoading).not.toHaveBeenCalled();
	});
});
