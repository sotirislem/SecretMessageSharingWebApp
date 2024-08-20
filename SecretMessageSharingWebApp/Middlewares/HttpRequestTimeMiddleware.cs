using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Middlewares;

public sealed record HttpRequestDateTimeFeature
{
	public DateTime RequestDateTime { get; init; }
}

public sealed class HttpRequestTimeMiddleware(IDateTimeProviderService dateTimeProviderService) : IMiddleware
{
	public Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		var httpRequestTimeFeature = new HttpRequestDateTimeFeature()
		{
			RequestDateTime = dateTimeProviderService.LocalNow()
		};

		context.Features.Set(httpRequestTimeFeature);

		return next(context);
	}
}
