import { HttpEvent, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { LoaderService } from '../services/loader.service';

export function httpLoaderInterceptor(req: HttpRequest<any>, next: HttpHandlerFn): Observable<HttpEvent<any>> {
	const loaderService = inject(LoaderService);

	// Skip loader for specific requests if needed (e.g. SignalR, background sync)
	if (req.url.includes('signalr') || req.headers.has('X-Skip-Loader')) {
		return next(req);
	}

	loaderService.enableLoading();

	return next(req).pipe(
		finalize(() => {
			loaderService.disableLoading();
		})
	);
}
