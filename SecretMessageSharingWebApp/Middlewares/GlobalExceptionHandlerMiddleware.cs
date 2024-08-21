using FastEndpoints;
using Microsoft.AspNetCore.Diagnostics;

namespace SecretMessageSharingWebApp.Middlewares;

internal sealed class GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		logger.LogError(exception,
			"{exceptionType}: {exceptionMessage}", exception.GetType(), exception.Message);

		var internalErrorResponse = new InternalErrorResponse()
		{
			Status = "Internal Server Error",
			Code = StatusCodes.Status500InternalServerError,
			Reason = "Something unexpected has happened",
			Note = "See application log for stack trace"
		};

		httpContext.Response.StatusCode = internalErrorResponse.Code;
		httpContext.Response.ContentType = "application/json";

		await httpContext.Response.WriteAsJsonAsync(internalErrorResponse, cancellationToken);

		return true;
	}
}