import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
	providedIn: 'root'
})
export class LoaderService {
	private loading$ = new BehaviorSubject(false);
	private pendingRequestsCount = 0;

	readonly loadingObservable$ = this.loading$.asObservable();

	constructor()
	{ }

	enableLoading() {
		this.pendingRequestsCount++;
		this.loading$.next(true);
	}

	disableLoading() {
		this.pendingRequestsCount--;
		if (this.pendingRequestsCount <= 0) {
			this.pendingRequestsCount = 0;
			this.loading$.next(false);
		}
	}
}
