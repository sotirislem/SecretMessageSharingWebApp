import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { LoaderService } from '../services/loader.service';
import {
	HttpResponse,
	HttpRequest,
	HttpHandler,
	HttpEvent,
	HttpInterceptor
} from '@angular/common/http';

@Injectable()
export class HttpLoaderInterceptor implements HttpInterceptor {
	private pendingRequests: HttpRequest<any>[] = [];

	constructor(private loaderService: LoaderService) { }

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		this.addPendingRequest(req);

		return next.handle(req).pipe(
			finalize(() => {
				this.removePendingRequest(req);
			})
		);
	}

	private addPendingRequest(req: HttpRequest<any>) {
		this.pendingRequests.push(req);
		this.loaderService.enableLoading();
	}

	private removePendingRequest(req: HttpRequest<any>) {
		const i = this.pendingRequests.indexOf(req);
		if (i >= 0) {
			this.pendingRequests.splice(i, 1);
		}

		if (this.pendingRequests.length > 0) {
			this.loaderService.enableLoading();
		} else {
			this.loaderService.disableLoading();
		}
	}
}
