import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { NavMenuComponent } from './nav-menu.component';

describe('NavMenuComponent', () => {
	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [NavMenuComponent],
			providers: [provideRouter([])]
		}).compileComponents();
	});

	it('should render the brand and every navigation link', () => {
		const fixture = TestBed.createComponent(NavMenuComponent);
		fixture.detectChanges();

		const element: HTMLElement = fixture.nativeElement;
		expect(element.querySelector('.app-brand-name')?.textContent).toContain('SecretMessageSharingWebApp');

		const links = Array.from(element.querySelectorAll('.app-nav-link')) as HTMLAnchorElement[];
		expect(links.map(link => link.getAttribute('href')))
			.toEqual(['/create-secret-message', '/recently-stored-messages', '/app-info']);
	});

	it('should start collapsed', () => {
		const fixture = TestBed.createComponent(NavMenuComponent);
		fixture.detectChanges();

		const toggle = fixture.nativeElement.querySelector('.app-nav-toggle') as HTMLButtonElement;
		expect(fixture.componentInstance.isExpanded()).toBeFalse();
		expect(toggle.getAttribute('aria-expanded')).toBe('false');
		expect(fixture.nativeElement.querySelector('.app-nav-links').classList).not.toContain('is-open');
	});

	it('should toggle the navigation menu', () => {
		const fixture = TestBed.createComponent(NavMenuComponent);
		fixture.detectChanges();

		const toggle = fixture.nativeElement.querySelector('.app-nav-toggle') as HTMLButtonElement;
		toggle.click();
		fixture.detectChanges();

		expect(fixture.componentInstance.isExpanded()).toBeTrue();
		expect(toggle.getAttribute('aria-expanded')).toBe('true');
		expect(toggle.classList).toContain('is-open');
		expect(fixture.nativeElement.querySelector('.app-nav-links').classList).toContain('is-open');

		toggle.click();
		fixture.detectChanges();

		expect(fixture.componentInstance.isExpanded()).toBeFalse();
	});

	it('should collapse the menu when a link or the brand is clicked', () => {
		const fixture = TestBed.createComponent(NavMenuComponent);
		fixture.detectChanges();

		fixture.componentInstance.toggle();
		fixture.detectChanges();

		(fixture.nativeElement.querySelector('.app-nav-link') as HTMLAnchorElement).click();
		fixture.detectChanges();
		expect(fixture.componentInstance.isExpanded()).toBeFalse();

		fixture.componentInstance.toggle();
		fixture.detectChanges();

		(fixture.nativeElement.querySelector('.app-brand') as HTMLAnchorElement).click();
		fixture.detectChanges();
		expect(fixture.componentInstance.isExpanded()).toBeFalse();
	});
});
