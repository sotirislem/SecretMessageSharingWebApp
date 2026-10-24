using System.Net;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using SecretMessageSharingWebApp.ConfigurationSettings;
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

		_clientFactory.Create(_settings.ApiKey).Returns(_client);
		_cancellationTokenProvider.Token.Returns(_cancellationToken);

		_sut = new SendGridEmailService(_logger, _cancellationTokenProvider, _settings, _clientFactory);
	}

	[Fact]
	public async Task SendOtp_ShouldSendEmailAndReturnTrue_WhenSendGridAccepts()
	{
		// Arrange
		var otp = _fixture.Create<OneTimePassword>();
		var messageId = _fixture.Create<string>();
		var recipientsEmail = _fixture.Create<string>();
		SendGridMessage? sentMessage = null;

		_client.SendEmailAsync(Arg.Do<SendGridMessage>(message => sentMessage = message), _cancellationToken)
			.Returns(new Response(HttpStatusCode.Accepted, responseBody: null!, responseHeaders: null!));

		// Act
		var result = await _sut.SendOtp(otp, messageId, recipientsEmail);

		// Assert
		result.Should().BeTrue();
		_clientFactory.Received(1).Create(_settings.ApiKey);
		await _client.Received(1).SendEmailAsync(Arg.Any<SendGridMessage>(), _cancellationToken);

		sentMessage.Should().NotBeNull();
		sentMessage!.From.Email.Should().Be(_settings.AuthSender);
		sentMessage.Personalizations.Should().ContainSingle();
		sentMessage.Personalizations[0].Subject.Should().Be($"{Constants.AppName} - OTP");
		sentMessage.Personalizations[0].Tos.Should().ContainSingle()
			.Which.Email.Should().Be(recipientsEmail);
		sentMessage.Contents.Should().ContainSingle()
			.Which.Type.Should().Be("text/html");
		sentMessage.Contents[0].Value.Should().Contain(messageId).And.Contain(otp.Code);
	}

	[Fact]
	public async Task SendOtp_ShouldReturnFalseAndLogError_WhenSendGridRejects()
	{
		// Arrange
		var otp = _fixture.Create<OneTimePassword>();
		var messageId = _fixture.Create<string>();

		_client.SendEmailAsync(Arg.Any<SendGridMessage>(), _cancellationToken)
			.Returns(new Response(HttpStatusCode.BadRequest, responseBody: null!, responseHeaders: null!));

		// Act
		var result = await _sut.SendOtp(otp, messageId, _fixture.Create<string>());

		// Assert
		result.Should().BeFalse();
		_logger.ReceivedWithAnyArgs().LogError(default);
	}
}
