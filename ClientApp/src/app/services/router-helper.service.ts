import { HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Router, UrlSerializer } from '@angular/router';

@Injectable({
	providedIn: 'root'
})
export class RouterHelperService {

	constructor(private router: Router) { }

	getCurrentNavigationStateData(key: string) {
		return this.router.getCurrentNavigation()?.extras.state?.[key] as string;
	}
}
