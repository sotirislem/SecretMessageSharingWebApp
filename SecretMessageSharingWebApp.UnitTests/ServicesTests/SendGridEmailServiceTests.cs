using System.Globalization;
using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using SendGrid;
using SendGrid.Helpers.Mail;
using SecretMessageSharingWebApp.ConfigurationSettings;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Providers;
using SecretMessageSharingWebApp.Services;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public class SendGridEmailServiceTests
{
	private readonly IFixture _fixture;
	private readonly ILogger<SendGridEmailService> _logger;
	private readonly ICancellationTokenProvider _cancellationTokenProvider;
	private readonly SendGridConfigurationSettings _settings;
	private readonly ISendGridClientFactory _clientFactory;
	private readonly ISendGridClient _client;
	private readonly CancellationToken _cancellationToken;
	private readonly ResiliencePipeline<Response> _resiliencePipeline;

	private readonly SendGridEmailService _sut;

	public SendGridEmailServiceTests()
	{
		_fixture = new Fixture();
		_logger = Substitute.For<ILogger<SendGridEmailService>>();
		_cancellationTokenProvider = Substitute.For<ICancellationTokenProvider>();
		_settings = new SendGridConfigurationSettings
		{
			ApiKey = _fixture.Create<string>(),
			AuthSender = _fixture.Create<string>()
		};
		_clientFactory = Substitute.For<ISendGridClientFactory>();
		_client = Substitute.For<ISendGridClient>();
		_cancellationToken = CancellationToken.None;

		_cancellationTokenProvider.Token.Returns(_cancellationToken);

		_resiliencePipeline = SendGridResiliencePipeline.Create(
			_logger,
			maxRetryAttempts: 2,
			retryDelay: TimeSpan.Zero,
			attemptTimeout: TimeSpan.FromSeconds(5));

		_clientFactory.Create(_settings.ApiKey)
			.Returns(new ResilientSendGridClient(_client, _resiliencePipeline));

		_sut = new SendGridEmailService(_logger, _cancellationTokenProvider, _settings, _clientFactory);
	}

	[Fact]
	public async Task SendOtp_ShouldSendEmailAndReturnTrue_WhenSendGridAccepts()
	{
		// Arrange
		var otp = _fixture.Create<OneTimePassword>() with { Code = "12345678" };
		var messageId = "message-id";
		var recipientsEmail = _fixture.Create<string>();
		SendGridMessage? sentMessage = null;

		_client.SendEmailAsync(Arg.Do<SendGridMessage>(message => sentMessage = message), Arg.Any<CancellationToken>())
			.Returns(new Response(HttpStatusCode.Accepted, responseBody: null!, responseHeaders: null!));

		// Act
		var result = await _sut.SendOtp(otp, messageId, recipientsEmail, null, null);

		// Assert
		result.Should().BeTrue();
		_clientFactory.Received(1).Create(_settings.ApiKey);
		await _client.Received(1).SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>());

		sentMessage.Should().NotBeNull();
		sentMessage!.From.Email.Should().Be(_settings.AuthSender);
		sentMessage.Personalizations.Should().ContainSingle();
		sentMessage.Personalizations[0].Subject.Should().Be($"{Constants.AppName} - OTP");
		sentMessage.Personalizations[0].Tos.Should().ContainSingle()
			.Which.Email.Should().Be(recipientsEmail);

		var html = sentMessage.Contents.Should().ContainSingle()
			.Which.Value;
		sentMessage.Contents[0].Type.Should().Be("text/html");

		var expiresAt = otp.ExpiresAt.ToUniversalTime()
			.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

		html.Should().Contain("Your secure code")
			.And.Contain("One-time code")
			.And.Contain("Message ID")
			.And.Contain(messageId)
			.And.Contain(otp.Code)
			.And.Contain("Expires on")
			.And.Contain(expiresAt)
			.And.Contain("UTC")
			.And.Contain($"{Constants.OtpExpirationMinutes} minutes")
			.And.Contain(Constants.AppUrl)
			.And.NotContain("{{");
	}

	[Fact]
	public async Task SendOtp_ShouldRenderTheExpiryInTheRecipientTimeZone()
	{
		// Arrange
		var otp = _fixture.Create<OneTimePassword>() with
		{
			Code = "12345678",
			ExpiresAt = new DateTimeOffset(2026, 7, 1, 9, 0, 0, TimeSpan.Zero)
		};
		SendGridMessage? sentMessage = null;

		_client.SendEmailAsync(Arg.Do<SendGridMessage>(message => sentMessage = message), Arg.Any<CancellationToken>())
			.Returns(new Response(HttpStatusCode.Accepted, responseBody: null!, responseHeaders: null!));

		// Act
		await _sut.SendOtp(otp, "message-id", "recipient@example.com", "Europe/Bucharest", 180);

		// Assert
		var html = sentMessage!.Contents[0].Value;
		html.Should().Contain("Expires on")
			.And.Contain("01/07/2026 12:00:00 (Europe/Bucharest, UTC+3:00)");
	}

	[Fact]
	public async Task SendOtp_ShouldFallBackToTheUtcOffset_WhenTheTimeZoneIsUnknown()
	{
		// Arrange
		var otp = _fixture.Create<OneTimePassword>() with
		{
			Code = "12345678",
			ExpiresAt = new DateTimeOffset(2026, 7, 1, 9, 0, 0, TimeSpan.Zero)
		};
		SendGridMessage? sentMessage = null;

		_client.SendEmailAsync(Arg.Do<SendGridMessage>(message => sentMessage = message), Arg.Any<CancellationToken>())
			.Returns(new Response(HttpStatusCode.Accepted, responseBody: null!, responseHeaders: null!));

		// Act
		await _sut.SendOtp(otp, "message-id", "recipient@example.com", "Not/AZone", 180);

		// Assert
		var html = sentMessage!.Contents[0].Value;
		html.Should().Contain("Expires on")
			.And.Contain("01/07/2026 12:00:00 (UTC+3:00)");
	}

	[Fact]
	public async Task SendOtp_ShouldHtmlEncodeTheMessageId()
	{
		// Arrange
		var otp = _fixture.Create<OneTimePassword>() with { Code = "12345678" };
		SendGridMessage? sentMessage = null;

		_client.SendEmailAsync(Arg.Do<SendGridMessage>(message => sentMessage = message), Arg.Any<CancellationToken>())
			.Returns(new Response(HttpStatusCode.Accepted, responseBody: null!, responseHeaders: null!));

		// Act
		await _sut.SendOtp(otp, "<script>alert(1)</script>", "recipient@example.com", null, null);

		// Assert
		var html = sentMessage!.Contents[0].Value;
		html.Should().NotContain("<script>").And.Contain("&lt;script&gt;");
	}

	[Fact]
	public async Task SendOtp_ShouldReturnFalseAndLogError_WhenSendGridRejects()
	{
		// Arrange
		var otp = _fixture.Create<OneTimePassword>();
		var messageId = _fixture.Create<string>();

		_client.SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>())
			.Returns(new Response(HttpStatusCode.BadRequest, responseBody: null!, responseHeaders: null!));

		// Act
		var result = await _sut.SendOtp(otp, messageId, _fixture.Create<string>(), null, null);

		// Assert
		result.Should().BeFalse();
		_logger.ReceivedWithAnyArgs().LogError(default);
	}

	[Fact]
	public async Task SendDeliveryNotification_ShouldSendEmailWithTheSameDetailsAsTheNotificationPopup()
	{
		// Arrange
		var recipientsEmail = "sender@example.com";
		SendGridMessage? sentMessage = null;

		var notification = new SecretMessageDeliveryNotification
		{
			MessageId = "message-id",
			MessageCreatedOn = new DateTime(2026, 10, 25, 10, 30, 0),
			MessageDeliveredOn = new DateTime(2026, 10, 25, 10, 35, 0),
			RecipientIP = "192.168.1.xxx",
			RecipientClientInfo = "Chrome 142 on Windows 11 (Desktop)",
			RecipientCity = "Bucharest",
			RecipientRegion = "Bucuresti",
			RecipientCountry = "Romania",
			RecipientCountryCode = "RO",
			RecipientLocationSource = "device",
			RecipientIsp = "DIGI Romania",
			RecipientTimezone = "Europe/Bucharest",
			RecipientUtcOffsetMinutes = 180,
			RecipientLanguage = "ro-RO",
			OtpVerified = true
		};

		_client.SendEmailAsync(Arg.Do<SendGridMessage>(message => sentMessage = message), Arg.Any<CancellationToken>())
			.Returns(new Response(HttpStatusCode.Accepted, responseBody: null!, responseHeaders: null!));

		// Act
		var result = await _sut.SendDeliveryNotification(notification, recipientsEmail);

		// Assert
		result.Should().BeTrue();
		_clientFactory.Received(1).Create(_settings.ApiKey);

		sentMessage.Should().NotBeNull();
		sentMessage!.From.Email.Should().Be(_settings.AuthSender);
		sentMessage.Personalizations.Should().ContainSingle();
		sentMessage.Personalizations[0].Subject.Should().Be($"{Constants.AppName} - Message delivery details");
		sentMessage.Personalizations[0].Tos.Should().ContainSingle()
			.Which.Email.Should().Be(recipientsEmail);

		var html = sentMessage.Contents.Should().ContainSingle()
			.Which.Value;
		sentMessage.Contents[0].Type.Should().Be("text/html");

		html.Should().Contain("message-id")
			.And.Contain("Your message was opened")
			.And.Contain("Delivery update")
			.And.Contain("Delivered")
			.And.Contain("Message ID")
			.And.Contain("25/10/2026 10:30:00")
			.And.Contain("25/10/2026 10:35:00")
			.And.Contain("5 minutes")
			.And.Contain("Bucharest, Bucuresti, Romania")
			.And.Contain("Precise (device location)")
			.And.Contain("192.168.1.xxx")
			.And.Contain("Chrome 142 on Windows 11 (Desktop)")
			.And.Contain("DIGI Romania")
			.And.Contain("Europe/Bucharest (UTC+3:00)")
			.And.Contain("(ro-RO)")
			.And.Contain("OTP verified")
			.And.Contain(Constants.AppUrl)
			.And.NotContain("{{");
	}

	[Fact]
	public async Task SendDeliveryNotification_ShouldHtmlEncodeRecipientControlledValues()
	{
		// Arrange
		SendGridMessage? sentMessage = null;
		var notification = _fixture.Create<SecretMessageDeliveryNotification>() with
		{
			RecipientCity = "<script>alert(1)</script>",
			RecipientClientInfo = "<b>device</b>"
		};

		_client.SendEmailAsync(Arg.Do<SendGridMessage>(message => sentMessage = message), Arg.Any<CancellationToken>())
			.Returns(new Response(HttpStatusCode.Accepted, responseBody: null!, responseHeaders: null!));

		// Act
		await _sut.SendDeliveryNotification(notification, "sender@example.com");

		// Assert
		var html = sentMessage!.Contents[0].Value;
		html.Should().NotContain("<script>").And.NotContain("<b>device</b>");
		html.Should().Contain("&lt;script&gt;").And.Contain("&lt;b&gt;device&lt;/b&gt;");
	}

	[Fact]
	public async Task SendDeliveryNotification_ShouldReturnFalseAndLogError_WhenSendGridRejects()
	{
		// Arrange
		var notification = _fixture.Create<SecretMessageDeliveryNotification>();

		_client.SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>())
			.Returns(new Response(HttpStatusCode.BadRequest, responseBody: null!, responseHeaders: null!));

		// Act
		var result = await _sut.SendDeliveryNotification(notification, "sender@example.com");

		// Assert
		result.Should().BeFalse();
		_logger.ReceivedWithAnyArgs().LogError(default);
	}

	[Fact]
	public async Task SendOtp_ShouldRetry_WhenSendGridReturnsATransientStatus()
	{
		// Arrange
		var otp = _fixture.Create<OneTimePassword>();

		_client.SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>())
			.Returns(
				new Response(HttpStatusCode.ServiceUnavailable, responseBody: null!, responseHeaders: null!),
				new Response(HttpStatusCode.Accepted, responseBody: null!, responseHeaders: null!));

		// Act
		var result = await _sut.SendOtp(otp, _fixture.Create<string>(), "recipient@example.com", null, null);

		// Assert
		result.Should().BeTrue();
		await _client.Received(2).SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>());
		_logger.ReceivedWithAnyArgs().LogWarning(default);
	}

	[Fact]
	public async Task SendOtp_ShouldNotRetry_WhenSendGridReturnsAPermanentStatus()
	{
		// Arrange
		var otp = _fixture.Create<OneTimePassword>();

		_client.SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>())
			.Returns(new Response(HttpStatusCode.Unauthorized, responseBody: null!, responseHeaders: null!));

		// Act
		var result = await _sut.SendOtp(otp, _fixture.Create<string>(), "recipient@example.com", null, null);

		// Assert
		result.Should().BeFalse();
		await _client.Received(1).SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SendOtp_ShouldReturnFalse_WhenNetworkFailuresPersistAfterRetrying()
	{
		// Arrange
		var otp = _fixture.Create<OneTimePassword>();
		const int expectedAttempts = 3; // initial attempt + 2 retries configured in the test pipeline

		_client.SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromException<Response>(new HttpRequestException("SendGrid is unreachable")));

		// Act
		var result = await _sut.SendOtp(otp, _fixture.Create<string>(), "recipient@example.com", null, null);

		// Assert
		result.Should().BeFalse();
		await _client.Received(expectedAttempts).SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>());
		_logger.ReceivedWithAnyArgs().LogError(default);
	}

	[Fact]
	public async Task SendDeliveryNotification_ShouldRetry_WhenSendGridReturnsATransientStatus()
	{
		// Arrange
		var notification = _fixture.Create<SecretMessageDeliveryNotification>();

		_client.SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>())
			.Returns(
				new Response(HttpStatusCode.TooManyRequests, responseBody: null!, responseHeaders: null!),
				new Response(HttpStatusCode.Accepted, responseBody: null!, responseHeaders: null!));

		// Act
		var result = await _sut.SendDeliveryNotification(notification, "sender@example.com");

		// Assert
		result.Should().BeTrue();
		await _client.Received(2).SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(HttpStatusCode.RequestTimeout, true)]
	[InlineData(HttpStatusCode.TooManyRequests, true)]
	[InlineData(HttpStatusCode.InternalServerError, true)]
	[InlineData(HttpStatusCode.BadGateway, true)]
	[InlineData(HttpStatusCode.ServiceUnavailable, true)]
	[InlineData(HttpStatusCode.GatewayTimeout, true)]
	[InlineData(HttpStatusCode.BadRequest, false)]
	[InlineData(HttpStatusCode.Unauthorized, false)]
	[InlineData(HttpStatusCode.Forbidden, false)]
	[InlineData(HttpStatusCode.NotFound, false)]
	[InlineData(HttpStatusCode.Accepted, false)]
	public void IsTransientResponse_ShouldClassifyStatusCodes(HttpStatusCode statusCode, bool expected)
	{
		var response = new Response(statusCode, responseBody: null!, responseHeaders: null!);

		SendGridResiliencePipeline.IsTransientResponse(response).Should().Be(expected);
	}
}
