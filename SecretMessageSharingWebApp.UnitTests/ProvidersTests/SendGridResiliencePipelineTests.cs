using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using SecretMessageSharingWebApp.Providers;
using SendGrid;

namespace SecretMessageSharingWebApp.UnitTests.ProvidersTests;

public class SendGridResiliencePipelineTests
{
	private readonly ILogger _logger = Substitute.For<ILogger>();

	[Fact]
	public async Task Execute_ShouldNotRetry_WhenTheFirstAttemptSucceeds()
	{
		// Arrange
		var pipeline = CreatePipeline();
		var accepted = CreateResponse(HttpStatusCode.Accepted);
		var attempts = 0;

		// Act
		var result = await pipeline.ExecuteAsync(_ =>
		{
			attempts++;

			return ValueTask.FromResult(accepted);
		});

		// Assert
		result.Should().BeSameAs(accepted);
		attempts.Should().Be(1);
		_logger.DidNotReceive().Log(
			Arg.Any<LogLevel>(),
			Arg.Any<EventId>(),
			Arg.Any<object>(),
			Arg.Any<Exception?>(),
			Arg.Any<Func<object, Exception?, string>>());
	}

	[Theory]
	[InlineData(HttpStatusCode.RequestTimeout)]
	[InlineData(HttpStatusCode.TooManyRequests)]
	[InlineData(HttpStatusCode.InternalServerError)]
	[InlineData(HttpStatusCode.BadGateway)]
	[InlineData(HttpStatusCode.ServiceUnavailable)]
	[InlineData(HttpStatusCode.GatewayTimeout)]
	public async Task Execute_ShouldRetry_WhenTheResultIsTransient(HttpStatusCode transientStatusCode)
	{
		// Arrange
		var pipeline = CreatePipeline();
		var accepted = CreateResponse(HttpStatusCode.Accepted);
		var attempts = 0;

		// Act
		var result = await pipeline.ExecuteAsync(_ =>
		{
			attempts++;

			return ValueTask.FromResult(
				attempts == 1 ? CreateResponse(transientStatusCode) : accepted);
		});

		// Assert
		result.Should().BeSameAs(accepted);
		attempts.Should().Be(2);
	}

	[Theory]
	[InlineData(HttpStatusCode.BadRequest)]
	[InlineData(HttpStatusCode.Unauthorized)]
	[InlineData(HttpStatusCode.Forbidden)]
	[InlineData(HttpStatusCode.NotFound)]
	[InlineData(HttpStatusCode.UnprocessableEntity)]
	public async Task Execute_ShouldNotRetry_WhenTheResultIsPermanent(HttpStatusCode permanentStatusCode)
	{
		// Arrange
		var pipeline = CreatePipeline();
		var rejected = CreateResponse(permanentStatusCode);
		var attempts = 0;

		// Act
		var result = await pipeline.ExecuteAsync(_ =>
		{
			attempts++;

			return ValueTask.FromResult(rejected);
		});

		// Assert
		result.Should().BeSameAs(rejected);
		attempts.Should().Be(1);
	}

	[Theory]
	[InlineData(TransientFailure.HttpRequest)]
	[InlineData(TransientFailure.Io)]
	[InlineData(TransientFailure.Timeout)]
	public async Task Execute_ShouldRetry_WhenAnAttemptThrowsATransientException(TransientFailure failure)
	{
		// Arrange
		var pipeline = CreatePipeline();
		var accepted = CreateResponse(HttpStatusCode.Accepted);
		var attempts = 0;

		// Act
		var result = await pipeline.ExecuteAsync(_ =>
		{
			attempts++;

			if (attempts == 1)
			{
				throw failure switch
				{
					TransientFailure.HttpRequest => new HttpRequestException("The connection was reset."),
					TransientFailure.Io => new IOException("The socket was closed."),
					_ => new TimeoutException("The request timed out.")
				};
			}

			return ValueTask.FromResult(accepted);
		});

		// Assert
		result.Should().BeSameAs(accepted);
		attempts.Should().Be(2);
	}

	[Fact]
	public async Task Execute_ShouldRetryThreeTimesByDefault()
	{
		// Arrange
		var pipeline = CreatePipeline(retryDelay: TimeSpan.FromMilliseconds(10));
		var unavailable = CreateResponse(HttpStatusCode.ServiceUnavailable);
		var attempts = 0;

		// Act
		var result = await pipeline.ExecuteAsync(_ =>
		{
			attempts++;

			return ValueTask.FromResult(unavailable);
		});

		// Assert
		// One initial attempt plus the three retries the pipeline is configured with.
		result.Should().BeSameAs(unavailable);
		attempts.Should().Be(4);
	}

	[Fact]
	public async Task Execute_ShouldStopAfterTheConfiguredNumberOfRetries()
	{
		// Arrange
		var pipeline = CreatePipeline(maxRetryAttempts: 2);
		var unavailable = CreateResponse(HttpStatusCode.ServiceUnavailable);
		var attempts = 0;

		// Act
		var result = await pipeline.ExecuteAsync(_ =>
		{
			attempts++;

			return ValueTask.FromResult(unavailable);
		});

		// Assert
		result.Should().BeSameAs(unavailable);
		attempts.Should().Be(3);
	}

	[Fact]
	public async Task Execute_ShouldRethrow_WhenEveryAttemptThrows()
	{
		// Arrange
		var pipeline = CreatePipeline(maxRetryAttempts: 2);
		var attempts = 0;

		Func<Task> execute = async () => await pipeline.ExecuteAsync<Response>(_ =>
		{
			attempts++;

			throw new HttpRequestException("SendGrid is unreachable.");
		});

		// Act
		var act = execute;

		// Assert
		await act.Should().ThrowAsync<HttpRequestException>()
			.WithMessage("SendGrid is unreachable.");
		attempts.Should().Be(3);
	}

	[Fact]
	public async Task Execute_ShouldGiveEachAttemptItsOwnTimeout()
	{
		// Arrange
		var pipeline = CreatePipeline(attemptTimeout: TimeSpan.FromMilliseconds(100));
		var accepted = CreateResponse(HttpStatusCode.Accepted);
		var attempts = 0;

		// Act
		var result = await pipeline.ExecuteAsync<Response>(async cancellationToken =>
		{
			attempts++;

			// The first attempt is abandoned by the timeout, so the retry must start
			// with a fresh timeout budget instead of inheriting the expired one.
			if (attempts == 1)
			{
				await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
			}

			return accepted;
		});

		// Assert
		result.Should().BeSameAs(accepted);
		attempts.Should().Be(2);
	}

	[Fact]
	public async Task Execute_ShouldThrowTimeoutRejected_WhenEveryAttemptTimesOut()
	{
		// Arrange
		var pipeline = CreatePipeline(maxRetryAttempts: 2, attemptTimeout: TimeSpan.FromMilliseconds(50));
		var attempts = 0;

		Func<Task> execute = async () => await pipeline.ExecuteAsync<Response>(async cancellationToken =>
		{
			attempts++;

			await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

			return CreateResponse(HttpStatusCode.Accepted);
		});

		// Act
		var act = execute;

		// Assert
		await act.Should().ThrowAsync<TimeoutRejectedException>();
		attempts.Should().Be(3);
	}

	[Fact]
	public async Task Execute_ShouldWaitBeforeRetrying()
	{
		// Arrange
		// The configured delay is expressed in seconds, so a one second delay must be
		// clearly visible. Jitter can shorten it, but not down to an immediate retry.
		var pipeline = SendGridResiliencePipeline.Create(
			_logger,
			maxRetryAttempts: 1,
			retryDelay: TimeSpan.FromSeconds(1),
			attemptTimeout: TimeSpan.FromSeconds(5));
		var attempts = 0;
		var stopwatch = Stopwatch.StartNew();

		// Act
		await pipeline.ExecuteAsync(_ =>
		{
			attempts++;

			return ValueTask.FromResult(CreateResponse(
				attempts == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.Accepted));
		});

		stopwatch.Stop();

		// Assert
		attempts.Should().Be(2);
		stopwatch.Elapsed.Should().BeGreaterThan(TimeSpan.FromMilliseconds(400));
	}

	[Fact]
	public async Task Execute_ShouldWaitBeforeRetrying_WithTheDefaultDelay()
	{
		// Arrange
		// The default delay is configured in seconds, so a retry must not fire
		// immediately; hammering a throttled SendGrid endpoint is what the backoff
		// exists to prevent.
		var pipeline = SendGridResiliencePipeline.Create(_logger, maxRetryAttempts: 1);
		var attempts = 0;
		var stopwatch = Stopwatch.StartNew();

		// Act
		await pipeline.ExecuteAsync(_ =>
		{
			attempts++;

			return ValueTask.FromResult(CreateResponse(
				attempts == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.Accepted));
		});

		stopwatch.Stop();

		// Assert
		attempts.Should().Be(2);
		stopwatch.Elapsed.Should().BeGreaterThan(TimeSpan.FromMilliseconds(400));
	}

	[Fact]
	public async Task Execute_ShouldLogAWarning_WhenItRetries()
	{
		// Arrange
		var pipeline = CreatePipeline();
		var attempts = 0;

		// Act
		await pipeline.ExecuteAsync(_ =>
		{
			attempts++;

			return ValueTask.FromResult(CreateResponse(
				attempts == 1 ? HttpStatusCode.TooManyRequests : HttpStatusCode.Accepted));
		});

		// Assert
		_logger.Received(1).Log(
			LogLevel.Warning,
			Arg.Any<EventId>(),
			Arg.Any<object>(),
			Arg.Any<Exception?>(),
			Arg.Any<Func<object, Exception?, string>>());
	}

	[Theory]
	[InlineData(HttpStatusCode.RequestTimeout, true)]
	[InlineData(HttpStatusCode.TooManyRequests, true)]
	[InlineData(HttpStatusCode.InternalServerError, true)]
	[InlineData(HttpStatusCode.BadGateway, true)]
	[InlineData(HttpStatusCode.ServiceUnavailable, true)]
	[InlineData(HttpStatusCode.GatewayTimeout, true)]
	[InlineData(HttpStatusCode.Accepted, false)]
	[InlineData(HttpStatusCode.NoContent, false)]
	[InlineData(HttpStatusCode.BadRequest, false)]
	[InlineData(HttpStatusCode.Unauthorized, false)]
	[InlineData(HttpStatusCode.Forbidden, false)]
	[InlineData(HttpStatusCode.NotFound, false)]
	public void IsTransientResponse_ShouldOnlyAcceptRetryableStatusCodes(HttpStatusCode statusCode, bool expected)
	{
		// Act
		var result = SendGridResiliencePipeline.IsTransientResponse(CreateResponse(statusCode));

		// Assert
		result.Should().Be(expected);
	}

	private ResiliencePipeline<Response> CreatePipeline(
		int maxRetryAttempts = 3,
		TimeSpan? retryDelay = null,
		TimeSpan? attemptTimeout = null)
	{
		return SendGridResiliencePipeline.Create(
			_logger,
			maxRetryAttempts,
			retryDelay ?? TimeSpan.FromMilliseconds(10),
			attemptTimeout ?? TimeSpan.FromSeconds(5));
	}

	private static Response CreateResponse(HttpStatusCode statusCode)
	{
		return new Response(statusCode, responseBody: null!, responseHeaders: null!);
	}

	public enum TransientFailure
	{
		HttpRequest,
		Io,
		Timeout
	}
}
