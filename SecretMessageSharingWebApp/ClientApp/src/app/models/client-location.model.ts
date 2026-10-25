/**
 * Request context that helps the sender understand where a message was opened.
 *
 * Coordinates are only present when the browser granted location access. They are
 * rounded to three decimals before leaving the browser and the server only uses
 * them to resolve a city — it never stores them.
 */
export interface ClientLocation {
	latitude?: number;
	longitude?: number;
	timezone?: string;
	utcOffsetMinutes?: number;
	language?: string;
	/** Device model when the browser exposes it, for example "Pixel 7". Desktop browsers have none. */
	deviceModel?: string;
}
