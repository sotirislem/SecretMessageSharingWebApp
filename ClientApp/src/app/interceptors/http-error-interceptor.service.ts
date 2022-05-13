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
import { ApiError } from '../models/api-error.model'
import { ToastrService } from 'ngx-toastr';

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
						const apiError = JSON.parse(response.error) as ApiError;

						toastrTitle = 'API Error';
						toastrMessage = apiError.message;
					}
					else {
						toastrTitle = response.statusText  + ' ' + `(Error code: ${response.status})`;
						toastrMessage = response.message;
					}

					this.toastrService.error(toastrMessage, toastrTitle, {
						timeOut: 15_000,
						extendedTimeOut: 0
					});
					return throwError(response);
				})
			);
	}
}
