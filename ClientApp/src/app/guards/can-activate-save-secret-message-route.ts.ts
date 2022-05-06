import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { RouterHelperService } from '../services/router-helper.service';

@Injectable({
	providedIn: 'root'
})
export class CanActivateSaveSecretMessageRouteGuard implements CanActivate {

	constructor(private routerHelperService: RouterHelperService)
	{ }

	canActivate(
		route: ActivatedRouteSnapshot,
		state: RouterStateSnapshot
	): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
		const secretMsgId = this.routerHelperService.getCurrentNavigationStateData('secretMsgId');
		const secretMsgKey = this.routerHelperService.getCurrentNavigationStateData('secretMsgKey');

		if (secretMsgId && secretMsgKey) {
			return true;
		}
		return false;
	}
}
