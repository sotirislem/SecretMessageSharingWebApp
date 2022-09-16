import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
	HttpRequest,
	HttpHandler,
	HttpEvent,
	HttpInterceptor,
    HttpHeaders
} from '@angular/common/http';

@Injectable()
export class HttpHeadersInterceptor implements HttpInterceptor {

	constructor(@Inject('CLIENT_ID') private clientId: string)
	{ }

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

		req = req.clone({
			headers: req.headers.set('Client-Id', this.clientId)
		});

		return next.handle(req);
	}
}
