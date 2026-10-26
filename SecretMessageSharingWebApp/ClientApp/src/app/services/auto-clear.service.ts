import { Injectable } from '@angular/core';

/**
 * Clears locally displayed sensitive content after a fixed interval.
 *
 * A plain setTimeout (even inside a Worker) is only a best-effort timer: browsers
 * throttle timers of hidden pages and freeze background tabs, and on Android the
 * whole page may be suspended. The callback can therefore run minutes late and the
 * worker's message is queued until the page is running again. To keep the interval
 * meaningful, the deadline is tracked in wall-clock time and checked again whenever
 * the page becomes visible. BFCache restores are handled explicitly, because they
 * would otherwise bring the plaintext back with the remaining timer still paused.
 */
@Injectable({
	providedIn: 'root'
})
export class AutoClearService {
	private worker: Worker | null = null;
	private deadline = 0;
	private clearContent: (() => void) | null = null;
	private isListening = false;

	private readonly handlePageStateChange = (): void => this.syncWithDeadline();
	private readonly handlePageHide = (event: PageTransitionEvent): void => {
		// A page put into the back/forward cache keeps its DOM, and therefore the
		// plaintext, while its timers are paused. Clear it before it is cached.
		if (event.persisted) {
			this.clearNow();
		}
	};

	/**
	 * Starts a new countdown, replacing any running one. `clearContent` is called
	 * once, either when the countdown expires or when the page is cached.
	 */
	start(intervalMinutes: number, clearContent: () => void): void {
		this.stop();

		const timeOutMs = intervalMinutes * 60_000;

		this.clearContent = clearContent;
		this.deadline = Date.now() + timeOutMs;
		this.armWorker(timeOutMs);

		document.addEventListener('visibilitychange', this.handlePageStateChange);
		window.addEventListener('pageshow', this.handlePageStateChange);
		window.addEventListener('focus', this.handlePageStateChange);
		window.addEventListener('pagehide', this.handlePageHide);
		this.isListening = true;
	}

	/**
	 * Stops the countdown without clearing the content. Must be called when the
	 * owner is destroyed, because the service itself lives for the whole session.
	 */
	stop(): void {
		this.worker?.terminate();
		this.worker = null;
		this.deadline = 0;
		this.clearContent = null;

		if (!this.isListening) return;

		document.removeEventListener('visibilitychange', this.handlePageStateChange);
		window.removeEventListener('pageshow', this.handlePageStateChange);
		window.removeEventListener('focus', this.handlePageStateChange);
		window.removeEventListener('pagehide', this.handlePageHide);
		this.isListening = false;
	}

	private clearNow(): void {
		const clearContent = this.clearContent;

		// Drop the callback first so a nested call cannot clear the content twice.
		this.stop();
		clearContent?.();
	}

	private syncWithDeadline(): void {
		if (!this.clearContent) return;

		const remainingMs = this.deadline - Date.now();

		if (remainingMs <= 0) {
			this.clearNow();
			return;
		}

		// The pending worker timer may have been throttled while the page was hidden.
		// Re-arm it with the time that is actually left instead of starting over.
		this.armWorker(remainingMs);
	}

	private armWorker(timeOutMs: number): void {
		this.worker ??= new Worker(new URL('../workers/auto-clear-timeout.worker', import.meta.url));

		this.worker.onmessage = ({ data }) => {
			if (data.clear) {
				this.clearNow();
			}
		};

		this.worker.postMessage({ timeOutMs });
	}
}
