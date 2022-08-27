import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { Routes } from '../../constants';
import { RouterHelperService } from '../services/router-helper.service';

@Injectable({
	providedIn: 'root'
})
export class NewSecretMessageRouteGuard implements CanActivate {

	constructor(private router: Router, private routerHelperService: RouterHelperService) { }

	canActivate(
		route: ActivatedRouteSnapshot,
		state: RouterStateSnapshot
	): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
		const secretMsgObj = this.routerHelperService.getCurrentNavigationStateData('secretMsgObj');
		const secretMsgId = this.routerHelperService.getCurrentNavigationStateData('secretMsgId');
		const secretMsgKey = this.routerHelperService.getCurrentNavigationStateData('secretMsgKey');

		if (secretMsgObj && secretMsgId && secretMsgKey) {
			return true;
		}

		this.router.navigate([Routes.Root]);
		return false;
	}
}
