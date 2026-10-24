import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { Routes } from '../../constants';
import { NewSecretMessageStateService } from '../services/new-secret-message-state.service';

@Injectable({
	providedIn: 'root'
})
export class NewSecretMessageRouteGuard implements CanActivate {

	constructor(
		private router: Router,
		private newSecretMessageStateService: NewSecretMessageStateService
	) { }

	canActivate(
		route: ActivatedRouteSnapshot,
		state: RouterStateSnapshot
	): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
		if (this.newSecretMessageStateService.get()) {
			return true;
		}

		this.router.navigate([Routes.Root]);
		return false;
	}
}
