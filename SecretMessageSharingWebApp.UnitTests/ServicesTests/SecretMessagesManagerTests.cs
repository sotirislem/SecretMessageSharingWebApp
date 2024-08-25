using Microsoft.Extensions.Logging;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services;
using SecretMessageSharingWebApp.Services.Interfaces;
using SecretMessageSharingWebApp.Mappings;
using OtpSettings = SecretMessageSharingWebApp.Models.Common.OtpSettings;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public class SecretMessagesManagerTests
{
	private readonly IFixture _fixture;
	private readonly ILogger<SecretMessagesManager> _logger;
	private readonly ISecretMessagesService _secretMessagesService;
	private readonly IRecentlyStoredMessagesService _recentlyStoredMessagesService;
	private readonly IGetLogsService _getLogsService;
	private readonly IOtpService _otpService;
	private readonly ISendGridEmailService _sendGridEmailService;
	private readonly IMemoryCacheService _memoryCacheService;
	private readonly IJwtService _jwtService;
	private readonly ISecretMessageDeliveryNotificationHubService _notificationHubService;
	
	private readonly SecretMessagesManager _sut;

	public SecretMessagesManagerTests()
	{
		_fixture = new Fixture();
		_logger = Substitute.For<ILogger<SecretMessagesManager>>();
		_secretMessagesService = Substitute.For<ISecretMessagesService>();
		_recentlyStoredMessagesService = Substitute.For<IRecentlyStoredMessagesService>();
		_getLogsService = Substitute.For<IGetLogsService>();
		_otpService = Substitute.For<IOtpService>();
		_sendGridEmailService = Substitute.For<ISendGridEmailService>();
		_memoryCacheService = Substitute.For<IMemoryCacheService>();
		_jwtService = Substitute.For<IJwtService>();
		_notificationHubService = Substitute.For<ISecretMessageDeliveryNotificationHubService>();

		_sut = new SecretMessagesManager(
			_logger,
			_secretMessagesService,
			_recentlyStoredMessagesService,
			_getLogsService,
			_otpService,
			_sendGridEmailService,
			_memoryCacheService,
			_jwtService,
			_notificationHubService
		);
	}

	[Fact]
	public async Task DeleteRecentMessage_ShouldReturnBadRequest_WhenMessageNotInRecentlyStoredList()
	{
		// Arrange
		var messageId = _fixture.Create<string>();

		_recentlyStoredMessagesService.GetRecentlyStoredSecretMessagesList()
			.Returns(new List<string>());

		// Act
		var result = await _sut.DeleteRecentMessage(messageId);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.BadRequest("Message not on recently stored list"));
	}

	[Fact]
	public async Task DeleteRecentMessage_ShouldReturnSuccessNoContent_WhenMessageDeleted()
	{
		// Arrange
		var messageId = _fixture.Create<string>();

		_recentlyStoredMessagesService.GetRecentlyStoredSecretMessagesList()
			.Returns(new List<string> { messageId });

		_secretMessagesService.Delete(messageId)
			.Returns(true);

		// Act
		var result = await _sut.DeleteRecentMessage(messageId);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.SuccessNoContent());
	}

	[Fact]
	public async Task DeleteRecentMessage_ShouldReturnNotFound_WhenMessageNotDeleted()
	{
		// Arrange
		var messageId = _fixture.Create<string>();

		_recentlyStoredMessagesService.GetRecentlyStoredSecretMessagesList()
			.Returns(new List<string> { messageId });

		_secretMessagesService.Delete(messageId)
			.Returns(false);

		// Act
		var result = await _sut.DeleteRecentMessage(messageId);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.NotFound());
	}

	[Fact]
	public async Task SendOtp_ShouldReturnBadRequest_WhenMessageDoesNotExistOrDoesNotRequireOtp()
	{
		// Arrange
		var messageId = _fixture.Create<string>();

		_secretMessagesService.Exists(messageId)
			.Returns((false, null));

		// Act
		var result = await _sut.SendOtp(messageId);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.BadRequest("Message does not exist or does not require OTP"));
	}

	[Fact]
	public async Task SendOtp_ShouldReturnSuccessNoContent_WhenOtpIsValidAndSent()
	{
		// Arrange
		var messageId = _fixture.Create<string>();
		var otpSettings = new OtpSettings()
		{
			Required = true,
            RecipientsEmail = _fixture.Create<string>()
		};
		var oneTimePassword = _fixture.Create<OneTimePassword>();

		_secretMessagesService.Exists(messageId)
			.Returns((true, otpSettings));

		_memoryCacheService.GetValue<OneTimePassword>(messageId, Constants.MemoryKeys.SecretMessageOtp, remove: false)
			.Returns((false, null));

		_otpService.Generate()
			.Returns(oneTimePassword);

		_sendGridEmailService.SendOtp(oneTimePassword, messageId, otpSettings.RecipientsEmail)
			.Returns(true);

		// Act
		var result = await _sut.SendOtp(messageId);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.SuccessNoContent());
		_memoryCacheService.Received().SetValue(messageId, oneTimePassword, Constants.MemoryKeys.SecretMessageOtp);
	}

	[Fact]
	public async Task SendOtp_ShouldReturnInternalServerError_WhenOtpNotSent()
	{
		// Arrange
		var messageId = _fixture.Create<string>();
		var otpSettings = new OtpSettings()
		{
			Required = true,
			RecipientsEmail = _fixture.Create<string>()
		};
		var oneTimePassword = _fixture.Create<OneTimePassword>();

		_secretMessagesService.Exists(messageId)
			.Returns((true, otpSettings));

		_memoryCacheService.GetValue<OneTimePassword>(messageId, Constants.MemoryKeys.SecretMessageOtp, remove: false)
			.Returns((false, null));

		_otpService.Generate()
			.Returns(oneTimePassword);

		_sendGridEmailService.SendOtp(oneTimePassword, messageId, otpSettings.RecipientsEmail)
			.Returns(false);

		// Act
		var result = await _sut.SendOtp(messageId);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.InternalServerError());
		_memoryCacheService.DidNotReceive().SetValue(messageId, oneTimePassword, Constants.MemoryKeys.SecretMessageOtp);
	}

	[Fact]
	public async Task ValidateOtp_ShouldReturnBadRequest_WhenNoOtpToValidate()
	{
		// Arrange
		var messageId = _fixture.Create<string>();
		var otpCode = _fixture.Create<string>();
		var otpSettings = new OtpSettings()
		{
			Required = true,
			RecipientsEmail = _fixture.Create<string>()
		};

		_secretMessagesService.Exists(messageId)
			.Returns((true, otpSettings));

		_memoryCacheService.GetValue<OneTimePassword>(messageId, Constants.MemoryKeys.SecretMessageOtp, remove: false)
			.Returns((false, null));

		// Act
		var result = await _sut.ValidateOtp(messageId, otpCode);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.BadRequest("No acquired OTP to validate"));
	}

	[Fact]
	public async Task ValidateOtp_ShouldReturnSuccessWithData_WhenOtpValid()
	{
		// Arrange
		var messageId = _fixture.Create<string>();
		var jwtToken = _fixture.Create<string>();
		var otp = _fixture.Create<OneTimePassword>();
		var otpCode = otp.Code;
		var otpSettings = new OtpSettings()
		{
			Required = true,
			RecipientsEmail = _fixture.Create<string>()
		};

		_secretMessagesService.Exists(messageId)
			.Returns((true, otpSettings));

		_memoryCacheService.GetValue<OneTimePassword>(messageId, Constants.MemoryKeys.SecretMessageOtp, remove: false)
			.Returns((true, otp));

		_otpService.Validate(otpCode, otp)
			.Returns((true, false, false));

		_jwtService.GenerateToken(messageId)
			.Returns(jwtToken);

		// Act
		var result = await _sut.ValidateOtp(messageId, otpCode);

		// Assert
		result.Should().BeEquivalentTo(ApiResult<ValidateSecretMessageOtpResponse>.SuccessWithData(new ValidateSecretMessageOtpResponse
		{
			IsValid = true,
			CanRetry = false,
			HasExpired = false,
			AuthToken = jwtToken
		}));
		_memoryCacheService.Received().RemoveValue(messageId, Constants.MemoryKeys.SecretMessageOtp);
	}

	[Fact]
	public async Task GetMessage_ShouldReturnNotFound_WhenSecretMessageNotExists()
	{
		// Arrange
		var messageId = _fixture.Create<string>();
		var encryptionKeySha256 = _fixture.Create<string>();
		var jwtToken = _fixture.Create<string>();
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		_secretMessagesService.Retrieve(messageId)
			.Returns((SecretMessage?)null);

		// Act
		var result = await _sut.GetMessage(messageId, encryptionKeySha256, jwtToken, httpContextClientInfo);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.NotFound());
	}

	[Fact]
	public async Task GetMessage_ShouldReturnBadRequest_WhenEncryptionKeyMismatch()
	{
		// Arrange
		var secretMessage = _fixture.Create<SecretMessage>();
		var messageId = secretMessage.Id;
		var wrongEncryptionKeySha256 = _fixture.Create<string>();
		var jwtToken = _fixture.Create<string>();
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		_secretMessagesService.Retrieve(messageId)
			.Returns(secretMessage);

		// Act
		var result = await _sut.GetMessage(messageId, wrongEncryptionKeySha256, jwtToken, httpContextClientInfo);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.BadRequest("Bad encryption key hash"));
	}

	[Fact]
	public async Task GetMessage_ShouldReturnUnauthorized_WhenJwtTokenInvalidForOtpMessage()
	{
		// Arrange
		var secretMessage = _fixture.Create<SecretMessage>() with
		{
			Otp = new OtpSettings
			{
				Required = true,
				RecipientsEmail = _fixture.Create<string>()
			}
		};
		var messageId = secretMessage.Id;
		var encryptionKeySha256 = secretMessage.EncryptionKeySha256;
		var jwtToken = _fixture.Create<string>();
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		_secretMessagesService.Retrieve(messageId)
			.Returns(secretMessage);

		_jwtService.ValidateToken(jwtToken, messageId)
			.Returns(false);

		// Act
		var result = await _sut.GetMessage(messageId, encryptionKeySha256, jwtToken, httpContextClientInfo);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.Unauthorized());
	}

	[Fact]
	public async Task GetMessage_ShouldReturnSuccessWithData_WhenMessageExistsAndJwtTokenValid()
	{
		// Arrange
		var secretMessage = _fixture.Create<SecretMessage>() with
		{
			Otp = new OtpSettings
			{
				Required = false,
				RecipientsEmail = ""
			}
		};
		var messageId = secretMessage.Id;
		var encryptionKeySha256 = secretMessage.EncryptionKeySha256;
		var jwtToken = _fixture.Create<string>();
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		var getLog = _fixture.Create<GetLog>();
		var secretMessageDeliveryNotification = getLog.ToSecretMessageDeliveryNotification();

		_secretMessagesService.Retrieve(messageId)
			.Returns(secretMessage);

		_getLogsService.CreateNewLog(Arg.Any<GetLog>())
			.Returns(getLog);

		_jwtService.ValidateToken(jwtToken, messageId)
			.Returns(true);

		_notificationHubService.SendNotification(secretMessage.CreatorClientId, secretMessageDeliveryNotification)
			.Returns(true);

		// Act
		var result = await _sut.GetMessage(messageId, encryptionKeySha256, jwtToken, httpContextClientInfo);

		// Assert
		result.Should().BeEquivalentTo(ApiResult<GetSecretMessageResponse>.SuccessWithData(new GetSecretMessageResponse
		{
			Data = secretMessage.Data,
			DeliveryNotificationSent = true
		}));
	}
}