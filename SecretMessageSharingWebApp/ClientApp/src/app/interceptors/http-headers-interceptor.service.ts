import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
	HttpRequest,
	HttpHandler,
	HttpEvent,
	HttpInterceptor,
    HttpHeaders
} from '@angular/common/http';
import { SecretMessageDeliveryNotificationHubService } from '../services/secret-message-delivery-notification-hub.service';

@Injectable()
export class HttpHeadersInterceptor implements HttpInterceptor {

	constructor(
		@Inject('CLIENT_ID') private clientId: string,
		private secretMessageDeliveryNotificationHubService: SecretMessageDeliveryNotificationHubService)
	{ }

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

		req = req.clone({
			headers: new HttpHeaders({
				'Client-Id': this.clientId
			})
		});

		return next.handle(req);
	}
}
