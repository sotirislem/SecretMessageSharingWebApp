import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ApiErrorResponse } from '../models/api/api-error-response.model';
import { ApiInternalErrorResponse } from '../models/api/api-internal-error-response.model';

export function httpErrorInterceptor(req: HttpRequest<any>, next: HttpHandlerFn): Observable<HttpEvent<any>> {
	const toastrService = inject(ToastrService);

	return next(req).pipe(
		catchError((response: HttpErrorResponse) => {
			let toastrTitle = 'API Error';
			let toastrMessage = `Status Code: ${response.status}`;

			// Messages are rendered as text only. Server or proxy responses must never
			// be interpreted as HTML, otherwise an error body could inject markup.
			if (typeof response.error === 'string' && response.error.length > 0) {
				toastrMessage += `\n\n${response.error}`;
			} else if (response.status === 400) {
				const badRequestError = response.error as ApiErrorResponse;

				if (badRequestError) {
					toastrMessage = badRequestError.message ?? toastrMessage;

					if (badRequestError.errors) {
						const validationErrors = Object.entries(badRequestError.errors)
							.flatMap(([errorKey, errorMessages]) =>
								(errorMessages ?? []).map(errorMessage => `${errorKey}: ${errorMessage}`));

						if (validationErrors.length > 0) {
							toastrMessage += `\n\n${validationErrors.join('\n')}`;
						}
					}
				}
			} else if (response.status === 500) {
				const internalError = response.error as ApiInternalErrorResponse;

				if (internalError) {
					toastrTitle = internalError.status ?? toastrTitle;
					toastrMessage = internalError.reason ?? toastrMessage;
				}
			}

			toastrService.error(toastrMessage, toastrTitle, {
				timeOut: 15_000,
				extendedTimeOut: 0,
				closeButton: true,
				progressBar: true,
				enableHtml: false
			});

			return throwError(() => response);
		})
	);
}
