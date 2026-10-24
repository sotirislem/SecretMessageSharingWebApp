import { inject, Injectable } from '@angular/core';

import { BASE_URL } from '../tokens';

@Injectable({
	providedIn: 'root'
})
export class UrlHelperService {
	private readonly baseUrl = inject(BASE_URL);

	createLocalUrlWithParams(routeName: string, params: Record<string, string>, fragment?: string): string {
		const url = new URL(`${this.baseUrl}/${routeName}`);

		for (const [param, value] of Object.entries(params)) {
			url.searchParams.set(param, value);
		}

		if (fragment) {
			url.hash = fragment;
		}

		return url.toString();
	}
}
