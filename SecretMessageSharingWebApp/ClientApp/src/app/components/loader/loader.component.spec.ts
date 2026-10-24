import { TestBed } from '@angular/core/testing';

import { LoaderService } from '../../services/loader.service';
import { LoaderComponent } from './loader.component';

describe('LoaderComponent', () => {
	let loaderService: LoaderService;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [LoaderComponent]
		}).compileComponents();

		loaderService = TestBed.inject(LoaderService);
	});

	it('should render nothing while no request is running', () => {
		const fixture = TestBed.createComponent(LoaderComponent);
		fixture.detectChanges();

		expect(fixture.nativeElement.querySelector('.loader-wrapper')).toBeNull();
	});

	it('should show the spinner while a request is running', () => {
		const fixture = TestBed.createComponent(LoaderComponent);

		loaderService.enableLoading();
		fixture.detectChanges();

		const element: HTMLElement = fixture.nativeElement;
		expect(element.querySelector('.loader-wrapper')).not.toBeNull();
		expect(element.querySelector('.spinner-text')?.textContent).toContain('Please wait');
		expect(element.querySelector('[role="status"]')).not.toBeNull();
	});

	it('should hide the spinner when all requests finished', () => {
		const fixture = TestBed.createComponent(LoaderComponent);

		loaderService.enableLoading();
		fixture.detectChanges();
		expect(fixture.nativeElement.querySelector('.loader-wrapper')).not.toBeNull();

		loaderService.disableLoading();
		fixture.detectChanges();

		expect(fixture.nativeElement.querySelector('.loader-wrapper')).toBeNull();
	});
});
