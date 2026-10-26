import { TestBed } from '@angular/core/testing';

import { AppInfoComponent } from './app-info.component';

describe('AppInfoComponent', () => {
	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [AppInfoComponent]
		}).compileComponents();
	});

	it('should render the security overview', () => {
		const fixture = TestBed.createComponent(AppInfoComponent);
		fixture.detectChanges();

		const element: HTMLElement = fixture.nativeElement;
		expect(element.querySelector('.info-hero h1')?.textContent).toContain('Share a secret');
		expect(element.querySelectorAll('.info-stat').length).toBe(4);
		expect(element.querySelectorAll('.info-card').length).toBeGreaterThanOrEqual(6);
		expect(element.textContent).toContain('Never stored on the server');
	});

	it('should render the frequently asked questions as an accordion', () => {
		const fixture = TestBed.createComponent(AppInfoComponent);
		fixture.detectChanges();

		const buttons = fixture.nativeElement.querySelectorAll('[ngbAccordionButton]') as NodeListOf<HTMLButtonElement>;
		expect(buttons.length).toBe(11);
		expect(buttons[0].textContent).toContain('How is the message encrypted?');
		expect(buttons[6].textContent).toContain('Can I get the delivery details by email?');
		expect(buttons[10].textContent).toContain('What is it built with?');
	});

	it('should expand a question when it is clicked', () => {
		const fixture = TestBed.createComponent(AppInfoComponent);
		fixture.detectChanges();

		const button = fixture.nativeElement.querySelector('[ngbAccordionButton]') as HTMLButtonElement;
		expect(button.getAttribute('aria-expanded')).toBe('false');

		button.click();
		fixture.detectChanges();

		expect(button.getAttribute('aria-expanded')).toBe('true');
	});
});
