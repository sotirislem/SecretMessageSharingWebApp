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

			if (typeof response.error === 'string' && response.error.length > 0) {
				toastrMessage += `<br><br>${response.error}`;
			} else if (response.status === 400) {
				const badRequestError = response.error as ApiErrorResponse;

				if (badRequestError) {
					toastrMessage = `${badRequestError.message}<br>`;

					if (badRequestError.errors) {
						toastrMessage += '<ul>';
						for (const errorKey in badRequestError.errors) {
							const errorMessages = badRequestError.errors[errorKey];
							for (const errorMessage of errorMessages) {
								toastrMessage += '<li><b>' + errorKey + '</b>: ' + errorMessage + '</li>';
							}
						}
						toastrMessage += '</ul>';
					}
				}
			} else if (response.status === 500) {
				const internalError = response.error as ApiInternalErrorResponse;

				if (internalError) {
					toastrTitle = internalError.status;
					toastrMessage = internalError.reason;
				}
			}

			toastrService.error(toastrMessage, toastrTitle, {
				timeOut: 15_000,
				extendedTimeOut: 0,
				closeButton: true,
				progressBar: true,
				enableHtml: true
			});

			return throwError(() => response);
		})
	);
}
