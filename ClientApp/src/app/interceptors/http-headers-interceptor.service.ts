import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
	HttpRequest,
	HttpHandler,
	HttpEvent,
	HttpInterceptor,
    HttpHeaders
} from '@angular/common/http';
import { SignalRService } from '../services/signalr.service';

@Injectable()
export class HttpHeadersInterceptor implements HttpInterceptor {

	constructor(private signalRService: SignalRService) { }

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		const signalRConnectionId = this.signalRService.getHubConnectionId();

		if (signalRConnectionId) {
			req = req.clone({
				headers: new HttpHeaders({
					'SignalR-ConnectionId': signalRConnectionId,
				})
			});
		}

		return next.handle(req);
	}
}
