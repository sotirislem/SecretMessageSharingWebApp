using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using SecretMessageSharingWebApp.BackgroundServices;
using SecretMessageSharingWebApp.Middlewares;
using SecretMessageSharingWebApp.Providers;
using SecretMessageSharingWebApp.Repositories;
using SecretMessageSharingWebApp.Repositories.Interfaces;
using SecretMessageSharingWebApp.Services;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Extensions;

public static class ServiceCollectionExtensions
{
	public static void BindConfigurationSettings<TOptions>(this IServiceCollection services, IConfiguration configuration) where TOptions : class
	{
		services.AddOptions<TOptions>()
			.Bind(configuration)
			.ValidateDataAnnotations();

		services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TOptions>>().Value);
	}
	
	public static void AddServices(this IServiceCollection services)
	{
		services.AddSingleton<IMemoryCacheService, MemoryCacheService>();

		services.AddSingleton<IDateTimeProviderService, DateTimeProviderService>();
		services.AddSingleton<ISecretMessageDeliveryNotificationHubService, SecretMessageDeliveryNotificationHubService>();
		services.AddSingleton<IOtpService, OtpService>();
		services.AddSingleton<IJwtService, JwtService>();

		services.AddScoped<ISendGridEmailService, SendGridEmailService>();

		services.AddScoped<ISecretMessagesRepository, SecretMessagesRepository>();
		services.AddScoped<ISecretMessagesService, SecretMessagesService>();
		services.AddScoped<IRecentlyStoredMessagesService, RecentlyStoredMessagesService>();
		services.AddScoped<ISecretMessagesManager, SecretMessagesManager>();

		services.AddScoped<IGetLogsRepository, GetLogsRepository>();
		services.AddScoped<IGetLogsService, GetLogsService>();
		
		services.AddScoped<ICancellationTokenProvider, CancellationTokenProvider>();
	}
	
	public static void AddBackgroundServices(this IServiceCollection services)
	{
		services.AddHostedService<DbAutoCleanerBackgroundService>();
	}
	
	public static void AddMiddlewares(this IServiceCollection services)
	{
		services.AddTransient<HttpRequestTimeMiddleware>();
		services.AddExceptionHandler<GlobalExceptionHandlerMiddleware>();
	}
	
	public static void SetRateLimiter(this IServiceCollection services)
	{
		services.AddRateLimiter(options =>
		{
			options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
			{
				var path = httpContext.Request.Path.Value;
			
				if (path is not null && (
					    path is "/index.html" ||
					    path.StartsWith("/static") || 
					    path.StartsWith("/swagger") ||
					    path.EndsWith(".js") || 
					    path.EndsWith(".css") || 
					    path.EndsWith(".png") || 
					    path.EndsWith(".jpg")))
				{
					return RateLimitPartition.GetNoLimiter(Constants.RateLimit.NoLimit);
				}
			
				return RateLimitPartition.GetFixedWindowLimiter(Constants.RateLimit.GlobalLimit, _ =>
					new FixedWindowRateLimiterOptions
					{
						PermitLimit = Constants.RateLimit.GlobalPermitLimitPerMinute,
						Window = TimeSpan.FromMinutes(1)
					});
			});
	
			options.OnRejected = async (context, token) =>
			{
				var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

				var ipAddress = context.HttpContext.GetClientIP();
				var method = context.HttpContext.Request.Method;
				var path = context.HttpContext.Request.Path;

				logger.LogWarning("DDoS Protection: Rate limit triggered. IP: {IP}, Path: {Path}, Method: {Method}", 
					ipAddress, path, method);
		
				context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
		
				if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
				{
					context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
				}
		
				await context.HttpContext.Response.WriteAsJsonAsync(new { 
					error = "Too many requests. Please try again later." 
				}, token);
			};
		});
	}
}
