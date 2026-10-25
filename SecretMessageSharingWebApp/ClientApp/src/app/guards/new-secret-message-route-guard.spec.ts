import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree } from '@angular/router';

import { NewSecretMessageStateService } from '../services/new-secret-message-state.service';
import { newSecretMessageRouteGuard } from './new-secret-message-route-guard';

describe('newSecretMessageRouteGuard', () => {
	let router: jasmine.SpyObj<Router>;
	let stateService: NewSecretMessageStateService;
	const route = {} as ActivatedRouteSnapshot;
	const routerState = {} as RouterStateSnapshot;

	beforeEach(() => {
		router = jasmine.createSpyObj<Router>('Router', ['createUrlTree']);
		router.createUrlTree.and.returnValue({} as UrlTree);

		TestBed.configureTestingModule({
			providers: [
				NewSecretMessageStateService,
				{ provide: Router, useValue: router }
			]
		});

		stateService = TestBed.inject(NewSecretMessageStateService);
	});

	function runGuard() {
		return TestBed.runInInjectionContext(() => newSecretMessageRouteGuard(route, routerState));
	}

	it('should allow navigation when a message is held in memory', () => {
		stateService.set({
			secretMsgObj: {
				plainText: 'secret',
				containsFile: false,
				fileName: null,
				base64BlobFile: null,
				containsUsernameAndPassword: false,
				username: '',
				password: ''
			},
			secretMsgId: 'message-id',
			secretMsgKey: 'message-key',
			otp: { required: false, recipientsEmail: '' }
		});

		expect(runGuard()).toBeTrue();
		expect(router.createUrlTree).not.toHaveBeenCalled();
	});

	it('should redirect to the home page when no message is held in memory', () => {
		const result = runGuard();

		expect(result).not.toBeTrue();
		expect(router.createUrlTree).toHaveBeenCalledWith(['/']);
	});

	it('should redirect again after the in-memory message is cleared', () => {
		stateService.set({
			secretMsgObj: {
				plainText: 'secret',
				containsFile: false,
				fileName: null,
				base64BlobFile: null,
				containsUsernameAndPassword: false,
				username: '',
				password: ''
			},
			secretMsgId: 'message-id',
			secretMsgKey: 'message-key',
			otp: { required: false, recipientsEmail: '' }
		});
		expect(runGuard()).toBeTrue();

		stateService.clear();

		expect(runGuard()).not.toBeTrue();
		expect(router.createUrlTree).toHaveBeenCalledWith(['/']);
	});
});
