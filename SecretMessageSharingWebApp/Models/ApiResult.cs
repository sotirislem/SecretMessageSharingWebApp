namespace SecretMessageSharingWebApp.Models;

public abstract record ApiResult
{
	public static SuccessNoContentResult SuccessNoContent() => new();
	public static UnauthorizedResult Unauthorized() => new();
	public static BadRequestResult BadRequest(string? message = null) => new(message);
	public static NotFoundResult NotFound(string? message = null) => new(message);
	public static InternalServerErrorResult InternalServerError(string? message = null) => new(message);

	public virtual string ResultType { get; }
	public virtual IResult HttpResult { get; }
}

public abstract record ApiResult<T> : ApiResult
{
	public static SuccessResult<T> SuccessWithData(T data) => new(data);

	public override string ResultType => typeof(T).FullName ?? string.Empty;

	public override IResult HttpResult
	{
		get
		{
			if (this is SuccessResult<T> successResult)
			{
				return Results.Ok(successResult.Data);
			}

			if (this is SuccessNoContentResult)
			{
				return Results.NoContent();
			}

			if (this is UnauthorizedResult)
			{
				return Results.Unauthorized();
			}

			if (this is BadRequestResult badRequestResult)
			{
				return Results.BadRequest(badRequestResult.Message);
			}

			if (this is NotFoundResult notFoundResult)
			{
				return Results.NotFound(notFoundResult.Message);
			}

			if (this is InternalServerErrorResult internalServerErrorResult)
			{
				return Results.Json(
					internalServerErrorResult.Message,
					statusCode: StatusCodes.Status500InternalServerError,
					contentType: "text/plain");
			}

			throw new ArgumentOutOfRangeException(nameof(HttpResult),
				$"Could not map {nameof(ApiResult<T>)} type to {nameof(HttpResult)}");
		}
	}
}

public sealed record SuccessResult<T>(T Data) : ApiResult<T>;

public sealed record SuccessNoContentResult() : ApiResult<object?>;

public sealed record UnauthorizedResult() : ApiResult<object?>;

public sealed record BadRequestResult(string? Message) : ApiResult<string?>;

public sealed record NotFoundResult(string? Message) : ApiResult<string?>;

public sealed record InternalServerErrorResult(string? Message) : ApiResult<string?>;