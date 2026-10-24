using Microsoft.AspNetCore.Http;
using SecretMessageSharingWebApp.Middlewares;

namespace SecretMessageSharingWebApp.UnitTests.MiddlewaresTests;

public class SecurityHeadersMiddlewareTests
{
	[Fact]
	public async Task InvokeAsync_ShouldAddSecurityHeaders()
	{
		// Arrange
		var context = new DefaultHttpContext();
		context.Request.Path = "/";
		var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		context.Response.Headers["Content-Security-Policy"].ToString().Should().Contain("default-src 'self'");
		context.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
		context.Response.Headers["X-Frame-Options"].ToString().Should().Be("DENY");
		context.Response.Headers["Referrer-Policy"].ToString().Should().Be("no-referrer");
		context.Response.Headers["X-Robots-Tag"].ToString().Should().Contain("noindex");
	}

	[Fact]
	public async Task InvokeAsync_ShouldDisableCachingForApiAndSignalR()
	{
		// Arrange
		var apiContext = new DefaultHttpContext();
		apiContext.Request.Path = "/api/secret-messages/1";

		var signalRContext = new DefaultHttpContext();
		signalRContext.Request.Path = "/signalr/negotiate";

		var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

		// Act
		await middleware.InvokeAsync(apiContext);
		await middleware.InvokeAsync(signalRContext);

		// Assert
		apiContext.Response.Headers["Cache-Control"].ToString().Should().Contain("no-store");
		signalRContext.Response.Headers["Cache-Control"].ToString().Should().Contain("no-store");
	}

	[Fact]
	public async Task InvokeAsync_ShouldNotSetCspForSwagger()
	{
		// Arrange
		var context = new DefaultHttpContext();
		context.Request.Path = "/swagger/index.html";
		var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		context.Response.Headers.ContainsKey("Content-Security-Policy").Should().BeFalse();
	}
}
