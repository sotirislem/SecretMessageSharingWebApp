namespace SecretMessageSharingWebApp.Middlewares
{
    public interface IHttpRequestTimeFeature
    {
        DateTime RequestTime { get; }
    }

    public class HttpRequestTimeFeature : IHttpRequestTimeFeature
    {
        public DateTime RequestTime { get; }

        public HttpRequestTimeFeature()
        {
            RequestTime = DateTime.Now;
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
            var httpRequestTimeFeature = new HttpRequestTimeFeature();
            context.Features.Set<IHttpRequestTimeFeature>(httpRequestTimeFeature);

            return _next(context);
        }
    }
}
