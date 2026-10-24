import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { Routes } from '../../constants';

export const getSecretMessageRouteGuard: CanActivateFn = (route) => {
	const id = route.queryParams['id'];

	// Allow a URL without an encryption key: an already-used link must still be
	// able to show the "no message found" / "key missing" state.
	return id ? true : inject(Router).createUrlTree([Routes.Root]);
};
