using SecretMessageSharingWebApp.Middlewares;
using UAParser;

namespace SecretMessageSharingWebApp.Helpers
{
	public static class HttpContextExtensions
	{
		public static string GetClientInfo(this HttpContext httpContext)
		{
			var userAgent = httpContext.Request.Headers["User-Agent"];
			var uaParser = Parser.GetDefault();

			ClientInfo clientInfo = uaParser.Parse(userAgent);
			return $"{clientInfo.OS}, {clientInfo.Device}, {clientInfo.UA}";
		}

		public static string GetClientIP(this HttpContext httpContext)
		{
			return httpContext.Connection.RemoteIpAddress?.ToString()!;
		}

		public static DateTime GetRequestDateTime(this HttpContext httpContext)
		{
			return httpContext.Features.Get<IHttpRequestTimeFeature>()!.RequestTime;
		}

		public static string GetRequestHeaderValue(this HttpRequest httpRequest, string key)
		{
			return httpRequest.Headers[key].ToString();
		}
	}
}
