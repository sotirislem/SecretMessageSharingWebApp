import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import {
	HttpRequest,
	HttpHandler,
	HttpEvent,
	HttpInterceptor,
	HttpErrorResponse
} from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { ApiErrorResponse } from '../models/api/api-error-response.model';
import { ApiInternalErrorResponse } from '../models/api/api-internal-error-response.model';

@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {

	constructor(private toastrService: ToastrService) { }

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		return next.handle(req)
			.pipe(
				catchError((response: HttpErrorResponse) => {
					let toastrTitle: string = `${response.statusText} (${response.status})`;
					let toastrMessage: string = 'API error';

					if (response.status === 400) {
						const badRequestError = response.error as ApiErrorResponse;

						if (badRequestError) {
							toastrMessage = `${badRequestError.message}</br>`

							if (badRequestError.errors) {
								toastrMessage += "<ul>";
								for (let errorKey in badRequestError.errors) {
									const errorMessages = badRequestError.errors[errorKey];
									for (let errorMessage of errorMessages) {
										toastrMessage += '<li><b>' + errorKey + '</b>: ' + errorMessage + '</li>';
									}
								}
								toastrMessage += "</ul>";
							}
						}
					}
					else if (response.status === 500) {
						const internalError = response.error as ApiInternalErrorResponse;

						if (internalError) {
							toastrTitle = internalError.status;
							toastrMessage = internalError.reason;
						}
					}

					this.toastrService.error(toastrMessage, toastrTitle, {
						timeOut: 15_000,
						extendedTimeOut: 0,
						closeButton: true,
						progressBar: true,
						enableHtml: true
					});

					return throwError(response);
				})
			);
	}
}
