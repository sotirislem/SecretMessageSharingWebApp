import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
	providedIn: 'root'
})
export class RouterHelperService {

	constructor(private router: Router) { }

	getCurrentNavigationStateData(key: string) {
		return this.router.getCurrentNavigation()?.extras.state?.[key];
	}

	reloadCurrentPage() {
		this.router.routeReuseStrategy.shouldReuseRoute = () => false;
		this.router.navigateByUrl(this.router.url);
	}
}
