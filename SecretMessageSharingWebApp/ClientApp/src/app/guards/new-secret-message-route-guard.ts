import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { Routes } from '../../constants';
import { NewSecretMessageStateService } from '../services/new-secret-message-state.service';

export const newSecretMessageRouteGuard: CanActivateFn = () => {
	const hasInMemoryState = inject(NewSecretMessageStateService).get() !== null;

	return hasInMemoryState ? true : inject(Router).createUrlTree([Routes.Root]);
};
