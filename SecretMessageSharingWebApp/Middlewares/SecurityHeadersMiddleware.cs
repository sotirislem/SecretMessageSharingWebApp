namespace SecretMessageSharingWebApp.Middlewares;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
	private const string ContentSecurityPolicy =
		"default-src 'self'; " +
		"base-uri 'self'; " +
		"object-src 'none'; " +
		"frame-ancestors 'none'; " +
		"form-action 'self'; " +
		"script-src 'self'; " +
		// Angular injects component styles at runtime, so inline styles are required.
		"style-src 'self' 'unsafe-inline'; " +
		"img-src 'self' data:; " +
		"font-src 'self' data:; " +
		"connect-src 'self' https://*.applicationinsights.azure.com https://*.in.applicationinsights.azure.com https://dc.services.visualstudio.com https://browser.events.data.microsoft.com https://js.monitor.azure.com; " +
		"worker-src 'self' blob:; " +
		"manifest-src 'self'; " +
		"media-src 'none'; " +
		"upgrade-insecure-requests";

	public async Task InvokeAsync(HttpContext context)
	{
		var headers = context.Response.Headers;

		headers["X-Content-Type-Options"] = "nosniff";
		headers["X-Frame-Options"] = "DENY";
		headers["Referrer-Policy"] = "no-referrer";
		// Geolocation is only requested after the recipient explicitly consents.
		headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(self), payment=(), usb=()";
		headers["Cross-Origin-Opener-Policy"] = "same-origin";
		headers["Cross-Origin-Resource-Policy"] = "same-origin";
		headers["X-Robots-Tag"] = "noindex, nofollow, noarchive";
		// Chromium only sends the Android device model with the Sec-CH-UA-Model
		// client hint after the origin opts in through Accept-CH.
		headers["Accept-CH"] = "Sec-CH-UA-Model";

		// Swagger UI relies on inline scripts/styles; it is only enabled in development.
		if (context.Request.Path.StartsWithSegments("/swagger") is false)
		{
			headers["Content-Security-Policy"] = ContentSecurityPolicy;
		}

		// Ciphertext and message metadata must never be stored by browsers or intermediaries.
		if (context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/signalr"))
		{
			headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
			headers["Pragma"] = "no-cache";
			headers["Expires"] = "0";
		}

		await next(context);
	}
}
