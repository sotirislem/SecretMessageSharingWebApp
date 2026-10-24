import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
	id: number;
	type: ToastType;
	title: string;
	message: string;
	closeButton: boolean;
	timeOut: number;
}

export interface ToastOptions {
	timeOut?: number;
	closeButton?: boolean;
}

const DEFAULT_TIME_OUT = 5_000;

/**
 * Small signal-based replacement for ngx-toastr. It renders through
 * ToastContainerComponent and needs no third-party peer dependency.
 */
@Injectable({
	providedIn: 'root'
})
export class ToastService {
	private nextId = 0;
	private readonly timers = new Map<number, ReturnType<typeof setTimeout>>();
	private readonly _toasts = signal<Toast[]>([]);

	readonly toasts = this._toasts.asReadonly();

	success(message: string, title = '', options?: ToastOptions): void {
		this.show('success', message, title, options);
	}

	error(message: string, title = '', options?: ToastOptions): void {
		this.show('error', message, title, { timeOut: 15_000, closeButton: true, ...options });
	}

	warning(message: string, title = '', options?: ToastOptions): void {
		this.show('warning', message, title, options);
	}

	info(message: string, title = '', options?: ToastOptions): void {
		this.show('info', message, title, options);
	}

	dismiss(id: number): void {
		const timer = this.timers.get(id);
		if (timer !== undefined) {
			clearTimeout(timer);
			this.timers.delete(id);
		}

		this._toasts.update(toasts => toasts.filter(toast => toast.id !== id));
	}

	clear(): void {
		for (const timer of this.timers.values()) {
			clearTimeout(timer);
		}
		this.timers.clear();
		this._toasts.set([]);
	}

	private show(type: ToastType, message: string, title: string, options?: ToastOptions): void {
		const id = ++this.nextId;
		const timeOut = options?.timeOut ?? DEFAULT_TIME_OUT;
		const toast: Toast = {
			id,
			type,
			title,
			message,
			closeButton: options?.closeButton ?? true,
			timeOut
		};

		this._toasts.update(toasts => [...toasts, toast]);

		if (timeOut > 0) {
			this.timers.set(id, setTimeout(() => this.dismiss(id), timeOut));
		}
	}
}
