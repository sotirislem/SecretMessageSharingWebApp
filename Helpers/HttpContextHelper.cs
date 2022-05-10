using SecretsManagerWebApp.Middlewares;
using UAParser;

namespace SecretsManagerWebApp.Helpers
{
	internal static class HttpContextHelper
	{
		internal static string GetClientInfo(HttpContext httpContext)
		{
			var userAgent = httpContext.Request.Headers["User-Agent"];
			var uaParser = Parser.GetDefault();

			ClientInfo clientInfo = uaParser.Parse(userAgent);
			return $"{clientInfo.OS}, {clientInfo.Device}, {clientInfo.UA}";
		}

		internal static string GetClientIP(HttpContext httpContext)
		{
			return httpContext.Connection.RemoteIpAddress?.ToString()!;
		}

		internal static DateTime GetRequestDateTime(HttpContext httpContext)
		{
			return httpContext.Features.Get<IHttpRequestTimeFeature>()!.RequestTime;
		}

		internal static string GetRequestHeaderValue(HttpRequest httpRequest, string key)
		{
			return httpRequest.Headers[key].ToString();
		}
	}
}
