import { TestBed } from '@angular/core/testing';

import { LoaderService } from './loader.service';

describe('LoaderService', () => {
	let service: LoaderService;

	beforeEach(() => {
		TestBed.configureTestingModule({});
		service = TestBed.inject(LoaderService);
	});

	it('should not be loading initially', () => {
		expect(service.loading()).toBeFalse();
	});

	it('should stay loading until every pending request has finished', () => {
		service.enableLoading();
		service.enableLoading();

		expect(service.loading()).toBeTrue();

		service.disableLoading();
		expect(service.loading()).toBeTrue();

		service.disableLoading();
		expect(service.loading()).toBeFalse();
	});

	it('should never drop below zero pending requests', () => {
		service.disableLoading();
		service.disableLoading();

		expect(service.loading()).toBeFalse();

		service.enableLoading();
		expect(service.loading()).toBeTrue();
	});
});
