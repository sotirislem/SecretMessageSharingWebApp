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

		return $"{clientInfo.Device}, {clientInfo.OS}, {clientInfo.UA}";
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
		return new HttpContextClientInfo
		{
			RequestDateTime = httpContext.GetRequestDateTime(),
			ClientIP = httpContext.GetClientIP(),
			ClientInfo = httpContext.GetClientInfo(),
			ClientId = httpContext.GetRequestHeaderValue("Client-Id")
		};
	}

	public static string? GetRequestHeaderValue(this HttpContext httpContext, string key)
	{
		return httpContext.Request.Headers[key].FirstOrDefault();
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
}
