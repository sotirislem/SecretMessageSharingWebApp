import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { CLIENT_ID } from '../tokens';

export const httpHeadersInterceptor: HttpInterceptorFn = (req, next) => {
	const clientId = inject(CLIENT_ID);

	return next(req.clone({
		setHeaders: { 'Client-Id': clientId }
	}));
};
