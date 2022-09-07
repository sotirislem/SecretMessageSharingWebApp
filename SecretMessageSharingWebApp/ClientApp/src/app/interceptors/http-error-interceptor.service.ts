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
import { InternalApiError } from '../models/api/internal-api-error.model'
import { BadRequestApiError } from '../models/api/bad-request-api-error.model';

@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {

	constructor(private toastrService: ToastrService) { }

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		return next.handle(req)
			.pipe(
				catchError((response: HttpErrorResponse) => {
					let toastrMessage: string = '';
					let toastrTitle: string = '';

					if (response.status === 500) {
						const apiError = response.error as InternalApiError;

						toastrTitle = 'API Error';
						toastrMessage = (apiError?.message ?? 'An unexpected internal API error occurred');
					}
					else if (response.status === 400) {
						const badRequestError = response.error as BadRequestApiError;

						toastrTitle = `${response.statusText} (${badRequestError.message})`;

						if (badRequestError.errors) {
							toastrMessage += "<ul>";
							for (let errorKey in badRequestError.errors) {
								const errorMessages = badRequestError.errors[errorKey];
								for (let errorMessage of errorMessages) {
									toastrMessage += '<li>' + errorMessage + '</li>';
								}
							}
							toastrMessage += "</ul>";
						}
					}
					else {
						toastrTitle = `${response.statusText} (${response.status})`;
						toastrMessage = response.message;
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

	private tryJsonParse<T>(response: any): T | null {
		try {
			return JSON.parse(response.error) as T;
		}
		catch {
			return null;
		}
	}
}
