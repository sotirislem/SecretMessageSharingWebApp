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
		const shouldReuseRoute = this.router.routeReuseStrategy.shouldReuseRoute;
		const onSameUrlNavigation = this.router.onSameUrlNavigation;

		this.router.routeReuseStrategy.shouldReuseRoute = () => false;
		this.router.onSameUrlNavigation = 'reload';
		this.router.navigateByUrl(this.router.url).finally(() => {
			this.router.routeReuseStrategy.shouldReuseRoute = shouldReuseRoute;
			this.router.onSameUrlNavigation = onSameUrlNavigation;
		});
	}
}
