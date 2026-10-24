import { TestBed } from '@angular/core/testing';

import { ToastService } from '../../services/toast.service';
import { ToastContainerComponent } from './toast-container.component';

describe('ToastContainerComponent', () => {
	let toastService: ToastService;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [ToastContainerComponent]
		}).compileComponents();

		toastService = TestBed.inject(ToastService);
	});

	afterEach(() => {
		toastService.clear();
	});

	it('should render every toast with its title, message and type class', () => {
		const fixture = TestBed.createComponent(ToastContainerComponent);
		fixture.detectChanges();

		toastService.success('Saved', 'Success');
		toastService.error('Failed', 'Error');
		toastService.warning('Careful');
		toastService.info('Info');
		fixture.detectChanges();

		const toasts = fixture.nativeElement.querySelectorAll('.toast-item');
		expect(toasts.length).toBe(4);
		expect(toasts[0].classList).toContain('alert-success');
		expect(toasts[1].classList).toContain('alert-danger');
		expect(toasts[2].classList).toContain('alert-warning');
		expect(toasts[3].classList).toContain('alert-info');
		expect(toasts[0].querySelector('.toast-title')?.textContent).toContain('Success');
		expect(toasts[0].querySelector('.toast-message')?.textContent).toContain('Saved');
	});

	it('should render a progress bar for toasts with a timeout', () => {
		const fixture = TestBed.createComponent(ToastContainerComponent);

		toastService.success('Temporary');
		fixture.detectChanges();

		const progress = fixture.nativeElement.querySelector('.toast-progress') as HTMLElement;
		expect(progress).not.toBeNull();
		expect(progress.style.animationDuration).toBe('5000ms');
	});

	it('should not render a progress bar for sticky toasts', () => {
		const fixture = TestBed.createComponent(ToastContainerComponent);

		toastService.success('Sticky', '', { timeOut: 0 });
		fixture.detectChanges();

		expect(fixture.nativeElement.querySelector('.toast-progress')).toBeNull();
	});

	it('should dismiss a toast when its close button is clicked', () => {
		const fixture = TestBed.createComponent(ToastContainerComponent);

		toastService.success('Dismiss me');
		fixture.detectChanges();

		const closeButton = fixture.nativeElement.querySelector('.toast-close') as HTMLButtonElement;
		closeButton.click();
		fixture.detectChanges();

		expect(toastService.toasts()).toEqual([]);
		expect(fixture.nativeElement.querySelectorAll('.toast-item').length).toBe(0);
	});

	it('should hide the close button when the toast asks for it', () => {
		const fixture = TestBed.createComponent(ToastContainerComponent);

		toastService.success('No close', '', { closeButton: false });
		fixture.detectChanges();

		expect(fixture.nativeElement.querySelector('.toast-close')).toBeNull();
	});
});
