import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { HomeComponent } from './home.component';

describe('HomeComponent', () => {
	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [HomeComponent],
			providers: [provideRouter([])]
		}).compileComponents();
	});

	it('should render the hero and call to action', () => {
		const fixture = TestBed.createComponent(HomeComponent);
		fixture.detectChanges();

		const element: HTMLElement = fixture.nativeElement;
		expect(element.querySelector('.hero-title')?.textContent).toContain('Share secrets');
		expect(element.querySelector('.hero-badge')?.textContent).toContain('End-to-end encrypted');
		expect(element.querySelectorAll('.trust-list li').length).toBe(3);
		expect(element.querySelectorAll('.step').length).toBe(3);
	});

	it('should link the call to action to the create page', () => {
		const fixture = TestBed.createComponent(HomeComponent);
		fixture.detectChanges();

		const link = fixture.nativeElement.querySelector('.cta-button') as HTMLAnchorElement;
		expect(link.getAttribute('href')).toBe('/create-secret-message');
		expect(link.textContent).toContain('Create a Message');
	});
});
