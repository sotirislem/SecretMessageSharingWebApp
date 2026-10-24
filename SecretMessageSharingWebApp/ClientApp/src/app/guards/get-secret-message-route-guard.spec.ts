import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree } from '@angular/router';

import { getSecretMessageRouteGuard } from './get-secret-message-route-guard';

describe('getSecretMessageRouteGuard', () => {
	let router: jasmine.SpyObj<Router>;
	const routerState = {} as RouterStateSnapshot;

	beforeEach(() => {
		router = jasmine.createSpyObj<Router>('Router', ['createUrlTree']);
		router.createUrlTree.and.returnValue({} as UrlTree);

		TestBed.configureTestingModule({
			providers: [{ provide: Router, useValue: router }]
		});
	});

	function runGuard(route: Partial<ActivatedRouteSnapshot>) {
		return TestBed.runInInjectionContext(() =>
			getSecretMessageRouteGuard(route as ActivatedRouteSnapshot, routerState));
	}

	it('allows a URL that has an id and an encryption key', () => {
		const result = runGuard({ queryParams: { id: 'message-id' }, fragment: 'encryption-key' });

		expect(result).toBeTrue();
	});

	it('allows a URL that has an id but no encryption key, so a used link can show the missing-message state', () => {
		const result = runGuard({ queryParams: { id: 'message-id' }, fragment: null });

		expect(result).toBeTrue();
	});

	it('redirects to the home page when the id is missing', () => {
		const result = runGuard({ queryParams: {}, fragment: 'encryption-key' });

		expect(result).not.toBeTrue();
		expect(router.createUrlTree).toHaveBeenCalledWith(['/']);
	});
});
