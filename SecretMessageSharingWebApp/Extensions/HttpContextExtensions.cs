﻿using SecretMessageSharingWebApp.Middlewares;
using UAParser;

namespace SecretMessageSharingWebApp.Extensions
{
	public static class HttpContextExtensions
	{
		public static string? GetClientInfo(this HttpContext httpContext)
		{
			var userAgent = httpContext.Request.Headers["User-Agent"];
			if (string.IsNullOrWhiteSpace(userAgent))
			{
				return null;
			}

			var uaParser = Parser.GetDefault();
			ClientInfo clientInfo = uaParser.Parse(userAgent);

			return $"{clientInfo.Device}, {clientInfo.OS}, {clientInfo.UA}";
		}

		public static string? GetClientIP(this HttpContext httpContext)
		{
			return httpContext.Connection.RemoteIpAddress?.ToString();
		}

		public static DateTime GetRequestDateTime(this HttpContext httpContext)
		{
			return httpContext.Features.Get<IHttpRequestDateTimeFeature>()!.RequestDateTime;
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
}
