import { TestBed } from '@angular/core/testing';

import { ToastService } from './toast.service';

describe('ToastService', () => {
	let service: ToastService;

	beforeEach(() => {
		jasmine.clock().install();
		TestBed.configureTestingModule({});
		service = TestBed.inject(ToastService);
	});

	afterEach(() => {
		service.clear();
		jasmine.clock().uninstall();
	});

	it('should start with no toasts', () => {
		expect(service.toasts()).toEqual([]);
	});

	it('should add a success toast with the default options', () => {
		service.success('Message copied.');

		expect(service.toasts()).toEqual([{
			id: 1,
			type: 'success',
			title: '',
			message: 'Message copied.',
			closeButton: true,
			timeOut: 5_000
		}]);
	});

	it('should add each toast type', () => {
		service.success('success');
		service.error('error');
		service.warning('warning');
		service.info('info');

		expect(service.toasts().map(toast => toast.type))
			.toEqual(['success', 'error', 'warning', 'info']);
	});

	it('should keep the error toast open longer by default', () => {
		service.error('Something failed', 'API Error');

		const [toast] = service.toasts();
		expect(toast.title).toBe('API Error');
		expect(toast.timeOut).toBe(15_000);
		expect(toast.closeButton).toBeTrue();
	});

	it('should allow the caller to override the default options', () => {
		service.error('Something failed', '', { timeOut: 1_000, closeButton: false });

		const [toast] = service.toasts();
		expect(toast.timeOut).toBe(1_000);
		expect(toast.closeButton).toBeFalse();
	});

	it('should assign an increasing id to every toast', () => {
		service.success('first');
		service.success('second');
		service.success('third');

		expect(service.toasts().map(toast => toast.id)).toEqual([1, 2, 3]);
	});

	it('should automatically dismiss a toast after its timeout', () => {
		service.success('temporary');

		jasmine.clock().tick(4_999);
		expect(service.toasts().length).toBe(1);

		jasmine.clock().tick(1);
		expect(service.toasts()).toEqual([]);
	});

	it('should not schedule a timer when the timeout is zero', () => {
		service.success('sticky', '', { timeOut: 0 });

		jasmine.clock().tick(60_000);

		expect(service.toasts().length).toBe(1);
	});

	it('should dismiss a toast by id', () => {
		service.success('first');
		service.success('second');

		service.dismiss(1);

		expect(service.toasts().map(toast => toast.message)).toEqual(['second']);
	});

	it('should ignore an unknown id when dismissing', () => {
		service.success('first');

		service.dismiss(999);

		expect(service.toasts().length).toBe(1);
	});

	it('should clear every toast and cancel its timer', () => {
		service.success('first');
		service.error('second');

		service.clear();

		expect(service.toasts()).toEqual([]);

		jasmine.clock().tick(60_000);
		expect(service.toasts()).toEqual([]);
	});
});
