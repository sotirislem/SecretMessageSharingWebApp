using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SecretMessageSharingWebApp.Middlewares;

internal sealed class GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		logger.LogError(exception, "Exception occurred: {exceptionMessage}", exception.Message);

		var problemDetails = new ProblemDetails
		{
			Status = StatusCodes.Status500InternalServerError,
			Title = "Internal Server Error"
		};

		httpContext.Response.StatusCode = problemDetails.Status.Value;

		await httpContext.Response
			.WriteAsJsonAsync(problemDetails, cancellationToken);

		return true;
	}
}