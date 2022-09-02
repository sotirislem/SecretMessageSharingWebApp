using SecretMessageSharingWebApp.Middlewares;
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

		public static string? GetRequestHeaderValue(this HttpRequest httpRequest, string key)
		{
			return httpRequest.Headers?[key].ToString();
		}
	}
}
