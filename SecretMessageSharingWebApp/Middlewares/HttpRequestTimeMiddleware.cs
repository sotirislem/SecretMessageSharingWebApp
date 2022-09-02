using SecretMessageSharingWebApp.Extensions;

namespace SecretMessageSharingWebApp.Middlewares
{
    public interface IHttpRequestDateTimeFeature
    {
        DateTime RequestDateTime { get; }
    }

    public class HttpRequestDateTimeFeature : IHttpRequestDateTimeFeature
    {
        public DateTime RequestDateTime { get; }

        public HttpRequestDateTimeFeature()
        {
            RequestDateTime = DateTime.Now.ToLocalTimeZone();
        }
    }

    public class HttpRequestTimeMiddleware
    {
        private readonly RequestDelegate _next;

        public HttpRequestTimeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            var httpRequestTimeFeature = new HttpRequestDateTimeFeature();
            context.Features.Set<IHttpRequestDateTimeFeature>(httpRequestTimeFeature);

            return _next(context);
        }
    }
}
