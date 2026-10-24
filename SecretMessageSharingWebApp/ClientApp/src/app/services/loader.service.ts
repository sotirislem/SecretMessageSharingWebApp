import { computed, Injectable, signal } from '@angular/core';

@Injectable({
	providedIn: 'root'
})
export class LoaderService {
	private readonly pendingRequests = signal(0);

	readonly loading = computed(() => this.pendingRequests() > 0);

	enableLoading(): void {
		this.pendingRequests.update(count => count + 1);
	}

	disableLoading(): void {
		this.pendingRequests.update(count => Math.max(0, count - 1));
	}
}
