using System.Globalization;
using System.Net;
using SecretMessageSharingWebApp.Middlewares;
using SecretMessageSharingWebApp.Models;
using UAParser;

namespace SecretMessageSharingWebApp.Extensions;

public static class HttpContextExtensions
{
	public static string? GetClientInfo(this HttpContext httpContext)
	{
		var userAgent = httpContext.Request.Headers.UserAgent;
		if (string.IsNullOrWhiteSpace(userAgent))
		{
			return null;
		}

		var uaParser = Parser.GetDefault();
		ClientInfo clientInfo = uaParser.Parse(userAgent);
		var userAgentString = userAgent.ToString();

		var browser = FormatClientPart(clientInfo.UA.Family, clientInfo.UA.Major);
		var operatingSystem = FormatClientPart(clientInfo.OS.Family, clientInfo.OS.Major);
		var deviceModel = FormatDeviceModel(
			httpContext.GetRequestHeaderValue("X-Client-Device-Model") ??
			httpContext.GetRequestHeaderValue("Sec-CH-UA-Model"));
		var deviceType = GetDeviceType(clientInfo, userAgentString);

		var deviceAndBrowser = string.Join(
			" on ",
			new[] { browser, operatingSystem }.Where(part => string.IsNullOrWhiteSpace(part) is false));

		if (string.IsNullOrWhiteSpace(deviceAndBrowser))
		{
			deviceAndBrowser = clientInfo.ToString();
		}

		var deviceDetails = string.Join(
			", ",
			new[] { deviceModel, deviceType }.Where(part => string.IsNullOrWhiteSpace(part) is false));

		return $"{deviceAndBrowser} ({deviceDetails})";
	}

	/// <summary>
	/// Chromium exposes the Android device model (for example "Pixel 7") through the
	/// user-agent client hints; the SPA forwards the same value as a request header.
	/// Desktop browsers do not expose a model at all.
	/// </summary>
	private static string? FormatDeviceModel(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		// Client hints use a quoted structured-header string.
		var model = value.Trim().Trim('"').Trim();

		if (model.Length is 0 or > 64)
		{
			return null;
		}

		// Device names are alphanumeric, optionally separated by spaces or dashes.
		return model.All(character =>
			char.IsLetterOrDigit(character) || character is ' ' or '-' or '_' or '.' or '+' or '/' or '(' or ')')
			? model
			: null;
	}

	private static string? FormatClientPart(string? family, string? version)
	{
		if (string.IsNullOrWhiteSpace(family) || string.Equals(family, "Other", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		return string.IsNullOrWhiteSpace(version) ? family : $"{family} {version}";
	}

	private static string GetDeviceType(ClientInfo clientInfo, string userAgent)
	{
		if (clientInfo.Device.IsSpider)
		{
			return "Bot";
		}

		if (userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase) ||
			userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase))
		{
			return "Tablet";
		}

		if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase) &&
			userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase) is false)
		{
			return "Tablet";
		}

		if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase) ||
			userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
			userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase))
		{
			return "Mobile";
		}

		return "Desktop";
	}

	public static string GetClientIP(this HttpContext httpContext)
	{
		var ipAddress = httpContext.Connection.RemoteIpAddress;

		if (ipAddress is null)
		{
			return "Unknown";
		}

		// Kestrel reports the loopback address (::1 or 127.0.0.1) when the app is opened
		// from the developer's own machine. Store a readable value in development so the
		// sender's IP does not show up as an IPv6 loopback address in the UI.
		if (IPAddress.IsLoopback(ipAddress) && httpContext.RequestServices?.GetService<IWebHostEnvironment>()?.IsDevelopment() is true)
		{
			return "localhost";
		}

		return ipAddress.ToString();
	}

	public static DateTime GetRequestDateTime(this HttpContext httpContext)
	{
		return httpContext.Features.Get<HttpRequestDateTimeFeature>()!.RequestDateTime;
	}

	public static HttpContextClientInfo GetHttpContextClientInfo(this HttpContext httpContext)
	{
		var latitude = httpContext.GetClientLatitude();
		var longitude = httpContext.GetClientLongitude();

		// A partial coordinate pair cannot be used for a lookup.
		if (latitude is null || longitude is null)
		{
			latitude = null;
			longitude = null;
		}

		return new HttpContextClientInfo
		{
			RequestDateTime = httpContext.GetRequestDateTime(),
			ClientIP = httpContext.GetClientIP(),
			ClientInfo = httpContext.GetClientInfo(),
			ClientId = httpContext.GetRequestHeaderValue("Client-Id"),
			ClientLatitude = latitude,
			ClientLongitude = longitude,
			ClientTimeZone = httpContext.GetClientTimeZone(),
			ClientUtcOffsetMinutes = httpContext.GetClientUtcOffsetMinutes(),
			ClientLanguage = httpContext.GetClientLanguage()
		};
	}

	public static string? GetRequestHeaderValue(this HttpContext httpContext, string key)
	{
		return httpContext.Request.Headers[key].FirstOrDefault();
	}

	public static double? GetClientLatitude(this HttpContext httpContext)
	{
		return TryParseCoordinate(httpContext.GetRequestHeaderValue("X-Client-Latitude"), -90, 90);
	}

	public static double? GetClientLongitude(this HttpContext httpContext)
	{
		return TryParseCoordinate(httpContext.GetRequestHeaderValue("X-Client-Longitude"), -180, 180);
	}

	public static string? GetClientTimeZone(this HttpContext httpContext)
	{
		var timeZone = httpContext.GetRequestHeaderValue("X-Client-Timezone")?.Trim();

		if (string.IsNullOrWhiteSpace(timeZone) || timeZone.Length > 64)
		{
			return null;
		}

		// IANA ids use letters, digits, '/', '_', '-' and (rarely) '+'.
		return timeZone.All(character =>
			char.IsLetterOrDigit(character) || character is '/' or '_' or '-' or '+')
			? timeZone
			: null;
	}

	public static int? GetClientUtcOffsetMinutes(this HttpContext httpContext)
	{
		var value = httpContext.GetRequestHeaderValue("X-Client-Utc-Offset-Minutes");

		if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var offset) is false)
		{
			return null;
		}

		return offset is >= -840 and <= 840 ? offset : null;
	}

	public static string? GetClientLanguage(this HttpContext httpContext)
	{
		var language =
			httpContext.GetRequestHeaderValue("X-Client-Language") ??
			httpContext.Request.Headers.AcceptLanguage.FirstOrDefault();

		if (string.IsNullOrWhiteSpace(language))
		{
			return null;
		}

		// Accept-Language can contain a weighted list; only the first preference is useful.
		var primaryLanguage = language.Split(',')[0].Trim();

		if (primaryLanguage.Length is 0 or > 35)
		{
			return null;
		}

		return primaryLanguage.All(character =>
			char.IsLetterOrDigit(character) || character is '-' or '_')
			? primaryLanguage
			: null;
	}

	public static string? ExtractJwtTokenFromRequestHeaders(this HttpContext httpContext)
	{
		var authorizationHeader = httpContext.GetRequestHeaderValue("Authorization");
		if (authorizationHeader is null)
		{
			return null;
		}

		var authorizationHeaderSplitted = authorizationHeader.Split(" ");
		if (authorizationHeaderSplitted.Length == 2 && authorizationHeaderSplitted[0] == "Bearer")
		{
			return authorizationHeaderSplitted[1];
		}

		return null;
	}

	private static double? TryParseCoordinate(string? value, double minimum, double maximum)
	{
		if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var coordinate) is false)
		{
			return null;
		}

		if (double.IsFinite(coordinate) is false)
		{
			return null;
		}

		return coordinate >= minimum && coordinate <= maximum
			? coordinate
			: null;
	}
}
