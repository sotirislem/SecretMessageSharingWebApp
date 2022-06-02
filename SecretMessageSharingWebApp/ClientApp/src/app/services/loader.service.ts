import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
	providedIn: 'root'
})
export class LoaderService {
	private loading = new BehaviorSubject(false);

	constructor() { }

	enableLoading() {
		this.loading.next(true);
	}

	disableLoading() {
		this.loading.next(false);
	}

	getLoadingObservable(): Observable<boolean> {
		return this.loading.asObservable();
	}
}
