import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';

import { LoaderService } from '../services/loader.service';

export const httpLoaderInterceptor: HttpInterceptorFn = (req, next) => {
	const loaderService = inject(LoaderService);

	// Skip loader for specific requests if needed (e.g. SignalR, background sync)
	if (req.url.includes('signalr') || req.headers.has('X-Skip-Loader')) {
		return next(req);
	}

	loaderService.enableLoading();

	return next(req).pipe(
		finalize(() => loaderService.disableLoading())
	);
};
