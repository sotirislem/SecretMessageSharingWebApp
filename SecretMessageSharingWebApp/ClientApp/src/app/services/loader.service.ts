import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
	providedIn: 'root'
})
export class LoaderService {
	private loading$ = new BehaviorSubject(false);

	readonly loadingObservable$ = this.loading$.asObservable();

	constructor()
	{ }

	enableLoading() {
		this.loading$.next(true);
	}

	disableLoading() {
		this.loading$.next(false);
	}
}
