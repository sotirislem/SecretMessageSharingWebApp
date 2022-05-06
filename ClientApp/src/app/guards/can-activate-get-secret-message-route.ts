import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { RouterHelperService } from '../services/router-helper.service';

@Injectable({
	providedIn: 'root'
})
export class CanActivateGetSecretMessageRouteGuard implements CanActivate {

	constructor(private routerHelperService: RouterHelperService)
	{ }

	canActivate(
		route: ActivatedRouteSnapshot,
		state: RouterStateSnapshot
	): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
		const id = route.queryParams.id;
		const encryptionKey = route.queryParams.encryptionKey;

		if (id && encryptionKey) {
			return true;
		}
		return false;
	}
}
