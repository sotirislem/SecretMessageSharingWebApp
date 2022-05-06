import { HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';

@Injectable({
	providedIn: 'root'
})
export class UrlHelperService {

	constructor(@Inject('BASE_URL') private baseUrl: string) { }

	createLocalUrlWithParams(routeName: string, params: { [param: string]: string }) {
		let httpParams = new HttpParams();
		for (let param in params) {
			httpParams = httpParams.append(param, params[param]);
		}
		return `${this.baseUrl}/${routeName}?${httpParams}`;
	}
}
