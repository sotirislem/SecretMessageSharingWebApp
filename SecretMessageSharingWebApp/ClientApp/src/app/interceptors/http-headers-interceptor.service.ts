import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { CLIENT_ID, TAB_ID } from '../tokens';

export const httpHeadersInterceptor: HttpInterceptorFn = (req, next) => {
	const clientId = inject(CLIENT_ID);
	const tabId = inject(TAB_ID);

	return next(req.clone({
		setHeaders: {
			'Client-Id': clientId,
			'Tab-Id': tabId
		}
	}));
};
