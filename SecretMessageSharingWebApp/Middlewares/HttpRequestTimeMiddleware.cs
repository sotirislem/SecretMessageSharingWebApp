using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Middlewares;

public interface IHttpRequestDateTimeFeature
{
	DateTime RequestDateTime { get; init; }
}

public sealed class HttpRequestDateTimeFeature : IHttpRequestDateTimeFeature
{
	public DateTime RequestDateTime { get; init; }
}

public sealed class HttpRequestTimeMiddleware
{
	private readonly RequestDelegate _next;
	private readonly IDateTimeProviderService _dateTimeProviderService;

	public HttpRequestTimeMiddleware(RequestDelegate next, IDateTimeProviderService dateTimeProviderService)
	{
		_next = next;
		_dateTimeProviderService = dateTimeProviderService;
	}

	public Task InvokeAsync(HttpContext context)
	{
		var httpRequestTimeFeature = new HttpRequestDateTimeFeature()
		{
			RequestDateTime = _dateTimeProviderService.LocalNow()
		};
		context.Features.Set<IHttpRequestDateTimeFeature>(httpRequestTimeFeature);

		return _next(context);
	}
}
