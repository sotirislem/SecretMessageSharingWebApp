import { Injectable } from '@angular/core';
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

	constructor(private secretMessageDeliveryNotificationHubService: SecretMessageDeliveryNotificationHubService) { }

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

		if (this.secretMessageDeliveryNotificationHubService.hubConnectionId) {
			req = req.clone({
				headers: new HttpHeaders({
					'SignalR-ConnectionId': this.secretMessageDeliveryNotificationHubService.hubConnectionId
				})
			});
		}

		return next.handle(req);
	}
}
