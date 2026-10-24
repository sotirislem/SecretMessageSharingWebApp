import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { GetSecretMessageRouteGuard } from './get-secret-message-route-guard';
import { RouterHelperService } from '../services/router-helper.service';

describe('GetSecretMessageRouteGuard', () => {
	let router: jasmine.SpyObj<Router>;
	let guard: GetSecretMessageRouteGuard;

	beforeEach(() => {
		router = jasmine.createSpyObj<Router>('Router', ['navigate']);
		guard = new GetSecretMessageRouteGuard(router, {} as RouterHelperService);
	});

	it('allows a URL that has an id and an encryption key', () => {
		const result = guard.canActivate(
			{ queryParams: { id: 'message-id' }, fragment: 'encryption-key' } as unknown as ActivatedRouteSnapshot,
			{} as unknown as RouterStateSnapshot);

		expect(result).toBeTrue();
	});

	it('allows a URL that has an id but no encryption key, so a used link can show the missing-message state', () => {
		const result = guard.canActivate(
			{ queryParams: { id: 'message-id' }, fragment: null } as unknown as ActivatedRouteSnapshot,
			{} as unknown as RouterStateSnapshot);

		expect(result).toBeTrue();
	});

	it('redirects to the home page when the id is missing', () => {
		const result = guard.canActivate(
			{ queryParams: {}, fragment: 'encryption-key' } as unknown as ActivatedRouteSnapshot,
			{} as unknown as RouterStateSnapshot);

		expect(result).toBeFalse();
		expect(router.navigate).toHaveBeenCalledWith(['/']);
	});
});
