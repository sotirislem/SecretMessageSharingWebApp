namespace SecretMessageSharingWebApp.Models;

public abstract record ApiResult
{
	public static SuccessResult<T> Success<T>(T data) => new(data);
	public static SuccessNoContentResult SuccessNoContent() => new();
	public static UnauthorizedResult Unauthorized() => new();
	public static BadRequestResult BadRequest(string? message = null) => new(message);
	public static NotFoundResult NotFound(string? message = null) => new(message);
	public static InternalServerErrorResult InternalServerError(string? message = null) => new(message);

	public int HttpStatusCode
	{
		get
		{
			if (GetType().IsGenericType &&
				GetType().GetGenericTypeDefinition() == typeof(SuccessResult<>))
			{
				return StatusCodes.Status200OK;
			}
			else if (this is SuccessNoContentResult)
			{
				return StatusCodes.Status204NoContent;
			}
			else if (this is UnauthorizedResult)
			{
				return StatusCodes.Status401Unauthorized;
			}
			else if (this is BadRequestResult)
			{
				return StatusCodes.Status400BadRequest;
			}
			else if (this is NotFoundResult)
			{
				return StatusCodes.Status404NotFound;
			}
			else if (this is InternalServerErrorResult)
			{
				return StatusCodes.Status500InternalServerError;
			}

			throw new ArgumentOutOfRangeException(nameof(HttpStatusCode),
				$"Could not map {nameof(ApiResult)} type to {nameof(HttpStatusCode)}");
		}
	}
}

public sealed record SuccessResult<T>(T Data) : ApiResult;

public sealed record SuccessNoContentResult() : ApiResult;

public sealed record UnauthorizedResult() : ApiResult;

public sealed record BadRequestResult(string? Message) : ApiResult;

public sealed record NotFoundResult(string? Message) : ApiResult;

public sealed record InternalServerErrorResult(string? Message) : ApiResult;