import { Injectable } from '@angular/core';

import { ClientLocation } from '../models/client-location.model';

interface UserAgentData {
	getHighEntropyValues(hints: string[]): Promise<{ model?: string }>;
}

interface NavigatorWithUserAgentData extends Navigator {
	userAgentData?: UserAgentData;
}

/**
 * Collects the request context that is sent with message retrieval.
 *
 * The browser's own permission prompt is the only prompt: when location access is
 * granted, coordinates are rounded and sent; when it is declined, the server
 * estimates the city from the IP address. Timezone, language, and (when the
 * browser exposes it) the device model are always sent because they describe
 * the request, not the person.
 *
 * Retrieval deliberately waits for the recipient to answer the prompt. There is
 * no timeout, because proceeding without an answer would send the request while
 * the recipient is still deciding whether to share their location.
 */
@Injectable({
	providedIn: 'root'
})
export class RecipientLocationService {
	private static readonly COORDINATE_DECIMALS = 3;

	/**
	 * Builds the context for the retrieval request. Never throws: if geolocation
	 * is unavailable or denied by the browser, only the request context (timezone,
	 * language, and device model when available) is returned and the server falls
	 * back to the IP-based city.
	 *
	 * @param onPermissionPrompt Invoked only when a browser prompt is expected,
	 * so the caller can explain why retrieval is paused.
	 */
	async collect(onPermissionPrompt?: () => void): Promise<ClientLocation> {
		const location: ClientLocation = {
			timezone: this.getTimeZone(),
			utcOffsetMinutes: -new Date().getTimezoneOffset(),
			language: navigator.language,
			deviceModel: await this.getDeviceModel()
		};

		if (typeof navigator.geolocation?.getCurrentPosition !== 'function') {
			return location;
		}

		const permissionState = await this.getPermissionState();

		// A permission that was already denied is an answer, so there is nothing
		// left to wait for and the request can continue with the IP fallback.
		if (permissionState === 'denied') {
			return location;
		}

		if (permissionState !== 'granted') {
			onPermissionPrompt?.();
		}

		const position = await this.getCurrentPosition();
		if (!position) {
			return location;
		}

		return {
			...location,
			latitude: this.roundCoordinate(position.coords.latitude),
			longitude: this.roundCoordinate(position.coords.longitude)
		};
	}

	private getTimeZone(): string | undefined {
		try {
			return Intl.DateTimeFormat().resolvedOptions().timeZone || undefined;
		} catch {
			return undefined;
		}
	}

	/**
	 * Chromium exposes the Android device model through the user-agent client
	 * hints; desktop browsers do not have a model to report. The model is optional
	 * context, so any failure simply leaves it out.
	 */
	private async getDeviceModel(): Promise<string | undefined> {
		const userAgentData = (navigator as NavigatorWithUserAgentData).userAgentData;
		if (!userAgentData?.getHighEntropyValues) {
			return undefined;
		}

		try {
			const { model } = await userAgentData.getHighEntropyValues(['model']);

			return model?.trim() || undefined;
		} catch {
			return undefined;
		}
	}

	private async getPermissionState(): Promise<PermissionState | null> {
		if (!navigator.permissions?.query) {
			return null;
		}

		try {
			const status = await navigator.permissions.query({ name: 'geolocation' });

			return status.state;
		} catch {
			// Some browsers do not expose the geolocation permission state, so a
			// prompt may still be shown even though the state is unknown.
			return null;
		}
	}

	private getCurrentPosition(): Promise<GeolocationPosition | null> {
		return new Promise<GeolocationPosition | null>(resolve => {
			let settled = false;

			const finish = (position: GeolocationPosition | null): void => {
				if (settled) return;

				settled = true;
				resolve(position);
			};

			navigator.geolocation.getCurrentPosition(
				position => finish(position),
				() => finish(null),
				{
					// City-level precision is enough, and a cached fix avoids
					// prompting the device for a fresh GPS reading every time.
					enableHighAccuracy: false,
					maximumAge: 10 * 60 * 1000
					// No `timeout`: an open browser prompt is not an answer. The
					// promise settles when the recipient allows or declines.
				});
		});
	}

	private roundCoordinate(value: number): number {
		const factor = 10 ** RecipientLocationService.COORDINATE_DECIMALS;

		return Math.round(value * factor) / factor;
	}
}
