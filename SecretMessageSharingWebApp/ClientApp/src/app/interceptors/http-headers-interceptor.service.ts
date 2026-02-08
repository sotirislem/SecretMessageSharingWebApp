import { HttpEvent, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';

export function httpHeadersInterceptor(req: HttpRequest<any>, next: HttpHandlerFn): Observable<HttpEvent<any>> {
	const clientId = inject<string>('CLIENT_ID' as any);

	req = req.clone({
		headers: req.headers.set('Client-Id', clientId)
	});

	return next(req);
}
