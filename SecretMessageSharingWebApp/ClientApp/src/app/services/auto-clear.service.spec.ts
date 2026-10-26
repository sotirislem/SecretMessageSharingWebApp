import { TestBed } from '@angular/core/testing';

import { AutoClearService } from './auto-clear.service';

class FakeWorker {
	static instances: FakeWorker[] = [];

	onmessage: ((event: MessageEvent) => void) | null = null;
	postMessage = jasmine.createSpy('postMessage');
	terminate = jasmine.createSpy('terminate');

	constructor() {
		FakeWorker.instances.push(this);
	}

	emit(data: unknown): void {
		this.onmessage?.({ data } as MessageEvent);
	}
}

describe('AutoClearService', () => {
	const intervalMinutes = 5;
	const intervalMs = intervalMinutes * 60_000;

	let service: AutoClearService;
	let originalWorker: typeof Worker;
	let clearContent: jasmine.Spy;
	let now: number;

	beforeEach(() => {
		FakeWorker.instances = [];
		originalWorker = window.Worker;
		window.Worker = FakeWorker as unknown as typeof Worker;

		now = 1_700_000_000_000;
		spyOn(Date, 'now').and.callFake(() => now);

		clearContent = jasmine.createSpy('clearContent');

		TestBed.configureTestingModule({});
		service = TestBed.inject(AutoClearService);
	});

	afterEach(() => {
		service.stop();
		window.Worker = originalWorker;
	});

	function showPage(): void {
		document.dispatchEvent(new Event('visibilitychange'));
	}

	it('should arm the worker with the requested interval and clear the content when it fires', () => {
		service.start(intervalMinutes, clearContent);

		expect(FakeWorker.instances.length).toBe(1);
		expect(FakeWorker.instances[0].postMessage).toHaveBeenCalledWith({ timeOutMs: intervalMs });

		FakeWorker.instances[0].emit({ clear: true });

		expect(clearContent).toHaveBeenCalledTimes(1);
		expect(FakeWorker.instances[0].terminate).toHaveBeenCalled();
	});

	it('should ignore unrelated worker messages', () => {
		service.start(intervalMinutes, clearContent);

		FakeWorker.instances[0].emit({ clear: false });

		expect(clearContent).not.toHaveBeenCalled();
	});

	it('should clear immediately when the interval passed while the page was hidden', () => {
		service.start(intervalMinutes, clearContent);

		now += intervalMs + 1;
		showPage();

		expect(clearContent).toHaveBeenCalledTimes(1);
		expect(FakeWorker.instances[0].terminate).toHaveBeenCalled();
	});

	it('should re-arm the worker with the remaining time when the page is shown again', () => {
		service.start(intervalMinutes, clearContent);
		const worker = FakeWorker.instances[0];

		now += 60_000;
		showPage();

		expect(clearContent).not.toHaveBeenCalled();
		expect(worker.postMessage).toHaveBeenCalledWith({ timeOutMs: intervalMs - 60_000 });
	});

	it('should clear the content before the page is cached in the back/forward cache', () => {
		service.start(intervalMinutes, clearContent);

		window.dispatchEvent(new PageTransitionEvent('pagehide', { persisted: true }));

		expect(clearContent).toHaveBeenCalledTimes(1);
		expect(FakeWorker.instances[0].terminate).toHaveBeenCalled();
	});

	it('should not clear the content when the page is unloaded without being cached', () => {
		service.start(intervalMinutes, clearContent);

		window.dispatchEvent(new PageTransitionEvent('pagehide', { persisted: false }));

		expect(clearContent).not.toHaveBeenCalled();
	});

	it('should restart the countdown and stop the previous worker on start', () => {
		service.start(intervalMinutes, clearContent);
		const firstWorker = FakeWorker.instances[0];

		service.start(intervalMinutes, clearContent);

		expect(firstWorker.terminate).toHaveBeenCalled();
		expect(FakeWorker.instances.length).toBe(2);
		expect(FakeWorker.instances[1].postMessage).toHaveBeenCalledWith({ timeOutMs: intervalMs });
	});

	it('should ignore the timer and page events after being stopped', () => {
		service.start(intervalMinutes, clearContent);
		const worker = FakeWorker.instances[0];

		service.stop();

		worker.emit({ clear: true });
		now += intervalMs + 1;
		showPage();

		expect(clearContent).not.toHaveBeenCalled();
		expect(worker.terminate).toHaveBeenCalled();
	});

	it('should detach its listeners when stopped', () => {
		const removeDocumentListener = spyOn(document, 'removeEventListener').and.callThrough();
		const removeWindowListener = spyOn(window, 'removeEventListener').and.callThrough();

		service.start(intervalMinutes, clearContent);
		service.stop();

		expect(removeDocumentListener).toHaveBeenCalledWith('visibilitychange', jasmine.any(Function));
		expect(removeWindowListener).toHaveBeenCalledWith('pageshow', jasmine.any(Function));
		expect(removeWindowListener).toHaveBeenCalledWith('focus', jasmine.any(Function));
		expect(removeWindowListener).toHaveBeenCalledWith('pagehide', jasmine.any(Function));
	});
});
