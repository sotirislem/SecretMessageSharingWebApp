using System.Net;

using Polly;
using Polly.Retry;
using Polly.Timeout;
using SendGrid;

namespace SecretMessageSharingWebApp.Providers;

/// <summary>
/// Builds the Polly pipeline used for SendGrid calls.
/// Transient HTTP responses and network failures are retried with exponential backoff;
/// permanent failures such as authentication or validation errors are returned immediately.
/// </summary>
public static class SendGridResiliencePipeline
{
	private const int MaxRetryAttempts = 3;
	private const int RetryDelaySeconds = 1;
	private const int AttemptTimeoutSeconds = 3;

	public static ResiliencePipeline<Response> Create(
		ILogger logger,
		int maxRetryAttempts = MaxRetryAttempts,
		TimeSpan? retryDelay = null,
		TimeSpan? attemptTimeout = null)
	{
		return new ResiliencePipelineBuilder<Response>()
			.AddRetry(new RetryStrategyOptions<Response>
			{
				ShouldHandle = new PredicateBuilder<Response>()
					.Handle<HttpRequestException>()
					.Handle<IOException>()
					.Handle<TimeoutException>()
					.Handle<TimeoutRejectedException>()
					.HandleResult(IsTransientResponse),
				MaxRetryAttempts = maxRetryAttempts,
				Delay = retryDelay ?? TimeSpan.FromSeconds(RetryDelaySeconds),
				BackoffType = DelayBackoffType.Exponential,
				UseJitter = true,
				OnRetry = args =>
				{
					logger.LogWarning(
						args.Outcome.Exception,
						"Transient SendGrid failure (status: {StatusCode}). Retrying attempt {RetryAttempt} of {MaxRetryAttempts} in {RetryDelay}.",
						args.Outcome.Result?.StatusCode,
						args.AttemptNumber + 1,
						maxRetryAttempts,
						args.RetryDelay);

					return default;
				}
			})
			// The timeout is added after the retry strategy, so every attempt gets its own
			// timeout instead of the whole retry sequence sharing one.
			.AddTimeout(attemptTimeout ?? TimeSpan.FromSeconds(AttemptTimeoutSeconds))
			.Build();
	}

	public static bool IsTransientResponse(Response response)
	{
		return response.StatusCode is
			HttpStatusCode.RequestTimeout or
			HttpStatusCode.TooManyRequests or
			HttpStatusCode.InternalServerError or
			HttpStatusCode.BadGateway or
			HttpStatusCode.ServiceUnavailable or
			HttpStatusCode.GatewayTimeout;
	}
}
