import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { ToastService } from '../services/toast.service';
import { ApiErrorResponse } from '../models/api/api-error-response.model';
import { ApiInternalErrorResponse } from '../models/api/api-internal-error-response.model';

export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
	const toastService = inject(ToastService);

	return next(req).pipe(
		catchError((response: HttpErrorResponse) => {
			let title = 'API Error';
			let message = `Status Code: ${response.status}`;

			// Messages are rendered as text only. Server or proxy responses must never
			// be interpreted as HTML, otherwise an error body could inject markup.
			if (typeof response.error === 'string' && response.error.length > 0) {
				message += `\n\n${response.error}`;
			} else if (response.status === 400) {
				const badRequestError = response.error as ApiErrorResponse;

				if (badRequestError) {
					message = badRequestError.message ?? message;

					if (badRequestError.errors) {
						const validationErrors = Object.entries(badRequestError.errors)
							.flatMap(([errorKey, errorMessages]) =>
								(errorMessages ?? []).map(errorMessage => `${errorKey}: ${errorMessage}`));

						if (validationErrors.length > 0) {
							message += `\n\n${validationErrors.join('\n')}`;
						}
					}
				}
			} else if (response.status === 500) {
				const internalError = response.error as ApiInternalErrorResponse;

				if (internalError) {
					title = internalError.status ?? title;
					message = internalError.reason ?? message;
				}
			}

			toastService.error(message, title, { timeOut: 15_000, closeButton: true });

			return throwError(() => response);
		})
	);
};
