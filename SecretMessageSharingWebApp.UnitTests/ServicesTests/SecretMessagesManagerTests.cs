using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services;
using SecretMessageSharingWebApp.Services.Interfaces;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Security;
using OtpSettings = SecretMessageSharingWebApp.Models.Common.OtpSettings;
using RecipientLocation = SecretMessageSharingWebApp.Models.Common.RecipientLocation;

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
	private readonly IRecipientLocationService _recipientLocationService;
	
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
		_recipientLocationService = Substitute.For<IRecipientLocationService>();

		_sut = new SecretMessagesManager(
			_logger,
			_secretMessagesService,
			_recentlyStoredMessagesService,
			_getLogsService,
			_otpService,
			_sendGridEmailService,
			_memoryCacheService,
			_jwtService,
			_notificationHubService,
			_recipientLocationService
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
	public async Task ValidateOtp_ShouldRemoveOtp_WhenOtpHasExpired()
	{
		// Arrange
		var messageId = _fixture.Create<string>();
		var otpCode = _fixture.Create<string>();
		var otp = _fixture.Create<OneTimePassword>();
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
			.Returns((false, false, true));

		// Act
		var result = await _sut.ValidateOtp(messageId, otpCode);

		// Assert
		result.Should().BeEquivalentTo(ApiResult<ValidateSecretMessageOtpResponse>.SuccessWithData(new ValidateSecretMessageOtpResponse
		{
			IsValid = false,
			CanRetry = false,
			HasExpired = true,
			AuthToken = null
		}));
		_memoryCacheService.Received(1).RemoveValue(messageId, Constants.MemoryKeys.SecretMessageOtp);
		_jwtService.DidNotReceive().GenerateToken(Arg.Any<string>());
	}

	[Fact]
	public async Task GetMessage_ShouldReturnNotFound_WhenSecretMessageNotExists()
	{
		// Arrange
		var messageId = _fixture.Create<string>();
		var encryptionKeySha256 = _fixture.Create<string>();
		var jwtToken = _fixture.Create<string>();
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		_secretMessagesService.Get(messageId)
			.Returns((SecretMessage?)null);

		// Act
		var result = await _sut.GetMessage(messageId, encryptionKeySha256, jwtToken, httpContextClientInfo);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.NotFound());
		await _secretMessagesService.DidNotReceive().Delete(Arg.Any<string>());
		await _getLogsService.DidNotReceive().CreateNewLog(Arg.Any<GetLog>());
	}

	[Fact]
	public async Task GetMessage_ShouldReturnBadRequest_WhenEncryptionKeyMismatch()
	{
		// Arrange
		var secretMessage = _fixture.Create<SecretMessage>() with
		{
			EncryptionKeySha256 = KeyHashVerifier.Create(_fixture.Create<string>())
		};
		var messageId = secretMessage.Id;
		var wrongEncryptionKeySha256 = _fixture.Create<string>();
		var jwtToken = _fixture.Create<string>();
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		_secretMessagesService.Get(messageId)
			.Returns(secretMessage);

		// Act
		var result = await _sut.GetMessage(messageId, wrongEncryptionKeySha256, jwtToken, httpContextClientInfo);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.BadRequest("Bad encryption key hash"));
		await _secretMessagesService.DidNotReceive().Delete(Arg.Any<string>());
		await _getLogsService.DidNotReceive().CreateNewLog(Arg.Any<GetLog>());
	}

	[Fact]
	public async Task GetMessage_ShouldReturnUnauthorized_WhenJwtTokenInvalidForOtpMessage()
	{
		// Arrange
		var encryptionKeySha256 = _fixture.Create<string>();
		var secretMessage = _fixture.Create<SecretMessage>() with
		{
			EncryptionKeySha256 = KeyHashVerifier.Create(encryptionKeySha256),
			Otp = new OtpSettings
			{
				Required = true,
				RecipientsEmail = _fixture.Create<string>()
			}
		};
		var messageId = secretMessage.Id;
		var jwtToken = _fixture.Create<string>();
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		_secretMessagesService.Get(messageId)
			.Returns(secretMessage);

		_jwtService.ValidateToken(jwtToken, messageId)
			.Returns(false);

		// Act
		var result = await _sut.GetMessage(messageId, encryptionKeySha256, jwtToken, httpContextClientInfo);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.Unauthorized());
		await _secretMessagesService.DidNotReceive().Delete(Arg.Any<string>());
		await _getLogsService.DidNotReceive().CreateNewLog(Arg.Any<GetLog>());
	}

	[Fact]
	public async Task GetMessage_ShouldReturnSuccessWithData_WhenMessageExistsAndJwtTokenValid()
	{
		// Arrange
		var encryptionKeySha256 = _fixture.Create<string>();
		var secretMessage = _fixture.Create<SecretMessage>() with
		{
			EncryptionKeySha256 = KeyHashVerifier.Create(encryptionKeySha256),
			Otp = new OtpSettings
			{
				Required = false,
				RecipientsEmail = ""
			}
		};
		var messageId = secretMessage.Id;
		var jwtToken = _fixture.Create<string>();
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		var getLog = _fixture.Create<GetLog>();
		var secretMessageDeliveryNotification = getLog.ToSecretMessageDeliveryNotification();

		_secretMessagesService.Get(messageId)
			.Returns(secretMessage);

		_secretMessagesService.Delete(messageId)
			.Returns(true);

		_getLogsService.CreateNewLog(Arg.Any<GetLog>())
			.Returns(getLog);

		_jwtService.ValidateToken(jwtToken, messageId)
			.Returns(true);

		_notificationHubService.SendNotification(secretMessage.CreatorClientId, null, secretMessageDeliveryNotification)
			.Returns(true);

		// Act
		var result = await _sut.GetMessage(messageId, encryptionKeySha256, jwtToken, httpContextClientInfo);

		// Assert
		result.Should().BeEquivalentTo(ApiResult<GetSecretMessageResponse>.SuccessWithData(new GetSecretMessageResponse
		{
			Data = secretMessage.Data,
			DeliveryNotificationSent = true
		}));
		await _secretMessagesService.Received(1).Delete(messageId);
	}

	[Fact]
	public async Task GetMessage_ShouldTargetTheCreatorTab_WhenTheCreatorOpensTheMessage()
	{
		// Arrange
		var encryptionKeySha256 = _fixture.Create<string>();
		var clientId = Guid.NewGuid().ToString();
		var creatorTabId = Guid.NewGuid().ToString();
		var secretMessage = _fixture.Create<SecretMessage>() with
		{
			EncryptionKeySha256 = KeyHashVerifier.Create(encryptionKeySha256),
			CreatorClientId = clientId,
			CreatorTabId = creatorTabId,
			Otp = new OtpSettings
			{
				Required = false,
				RecipientsEmail = ""
			}
		};
		var messageId = secretMessage.Id;
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>() with
		{
			ClientId = clientId
		};

		var getLog = _fixture.Create<GetLog>();
		var secretMessageDeliveryNotification = getLog.ToSecretMessageDeliveryNotification();

		_secretMessagesService.Get(messageId)
			.Returns(secretMessage);

		_secretMessagesService.Delete(messageId)
			.Returns(true);

		_getLogsService.CreateNewLog(Arg.Any<GetLog>())
			.Returns(getLog);

		_notificationHubService.SendNotification(clientId, creatorTabId, secretMessageDeliveryNotification)
			.Returns(true);

		// Act
		var result = await _sut.GetMessage(messageId, encryptionKeySha256, null, httpContextClientInfo);

		// Assert
		result.Should().BeOfType<SuccessResult<GetSecretMessageResponse>>();
		await _notificationHubService.Received(1).SendNotification(clientId, creatorTabId, secretMessageDeliveryNotification);
	}

	[Fact]
	public async Task GetMessage_ShouldNotifyEveryTab_WhenSomebodyElseOpensTheMessage()
	{
		// Arrange
		var encryptionKeySha256 = _fixture.Create<string>();
		var secretMessage = _fixture.Create<SecretMessage>() with
		{
			EncryptionKeySha256 = KeyHashVerifier.Create(encryptionKeySha256),
			Otp = new OtpSettings
			{
				Required = false,
				RecipientsEmail = ""
			}
		};
		var messageId = secretMessage.Id;
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		var getLog = _fixture.Create<GetLog>();
		var secretMessageDeliveryNotification = getLog.ToSecretMessageDeliveryNotification();

		_secretMessagesService.Get(messageId)
			.Returns(secretMessage);

		_secretMessagesService.Delete(messageId)
			.Returns(true);

		_getLogsService.CreateNewLog(Arg.Any<GetLog>())
			.Returns(getLog);

		_notificationHubService.SendNotification(secretMessage.CreatorClientId, null, secretMessageDeliveryNotification)
			.Returns(true);

		// Act
		await _sut.GetMessage(messageId, encryptionKeySha256, null, httpContextClientInfo);

		// Assert
		await _notificationHubService.Received(1).SendNotification(secretMessage.CreatorClientId, null, secretMessageDeliveryNotification);
	}

	[Fact]
	public async Task GetMessage_ShouldReturnSuccessWithData_WhenStoredVerifierIsDomainSeparated()
	{
		// Arrange
		var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()))).ToLowerInvariant();

		var secretMessage = _fixture.Create<SecretMessage>() with
		{
			EncryptionKeySha256 = KeyHashVerifier.Create(keyHash),
			Otp = new OtpSettings
			{
				Required = false,
				RecipientsEmail = ""
			}
		};
		var messageId = secretMessage.Id;
		var jwtToken = _fixture.Create<string>();
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		var getLog = _fixture.Create<GetLog>();

		_secretMessagesService.Get(messageId)
			.Returns(secretMessage);

		_secretMessagesService.Delete(messageId)
			.Returns(true);

		_getLogsService.CreateNewLog(Arg.Any<GetLog>())
			.Returns(getLog);

		// Act
		var result = await _sut.GetMessage(messageId, keyHash, jwtToken, httpContextClientInfo);

		// Assert
		result.Should().BeEquivalentTo(ApiResult<GetSecretMessageResponse>.SuccessWithData(new GetSecretMessageResponse
		{
			Data = secretMessage.Data,
			DeliveryNotificationSent = false
		}));
		await _secretMessagesService.Received(1).Delete(messageId);
	}

	[Fact]
	public async Task GetMessage_ShouldReturnNotFound_WhenMessageWasAlreadyRetrieved()
	{
		// Arrange
		var encryptionKeySha256 = _fixture.Create<string>();
		var secretMessage = _fixture.Create<SecretMessage>() with
		{
			EncryptionKeySha256 = KeyHashVerifier.Create(encryptionKeySha256),
			Otp = new OtpSettings
			{
				Required = false,
				RecipientsEmail = ""
			}
		};
		var messageId = secretMessage.Id;
		var jwtToken = _fixture.Create<string>();
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		var getLog = _fixture.Create<GetLog>();

		_secretMessagesService.Get(messageId)
			.Returns(secretMessage);

		_secretMessagesService.Delete(messageId)
			.Returns(false);

		_getLogsService.CreateNewLog(Arg.Any<GetLog>())
			.Returns(getLog);

		// Act
		var result = await _sut.GetMessage(messageId, encryptionKeySha256, jwtToken, httpContextClientInfo);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.NotFound());
		await _notificationHubService.DidNotReceive().SendNotification(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<SecretMessageDeliveryNotification>());
	}

	[Fact]
	public async Task GetMessage_ShouldPersistRecipientLocationAndRequestContext()
	{
		// Arrange
		var encryptionKeySha256 = _fixture.Create<string>();
		var secretMessage = _fixture.Create<SecretMessage>() with
		{
			EncryptionKeySha256 = KeyHashVerifier.Create(encryptionKeySha256),
			CreatorIP = "8.8.8.8",
			Otp = new OtpSettings
			{
				Required = false,
				RecipientsEmail = ""
			}
		};
		var messageId = secretMessage.Id;
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>() with
		{
			ClientIP = "8.8.4.4",
			ClientId = Guid.NewGuid().ToString(),
			ClientLatitude = 44.4268,
			ClientLongitude = 26.1025,
			ClientTimeZone = "Europe/Bucharest",
			ClientUtcOffsetMinutes = 180,
			ClientLanguage = "ro-RO"
		};

		var location = new RecipientLocation
		{
			City = "Bucharest",
			Region = "Bucuresti",
			Country = "Romania",
			CountryCode = "RO",
			Isp = "Example ISP",
			Source = Constants.LocationSources.Device
		};

		var getLog = _fixture.Create<GetLog>();
		GetLog? capturedLog = null;

		_secretMessagesService.Get(messageId)
			.Returns(secretMessage);

		_secretMessagesService.Delete(messageId)
			.Returns(true);

		_recipientLocationService
			.ResolveAsync(Arg.Any<string?>(), Arg.Any<double?>(), Arg.Any<double?>(), Arg.Any<CancellationToken>())
			.Returns(location);

		_getLogsService.CreateNewLog(Arg.Any<GetLog>())
			.Returns(callInfo =>
			{
				capturedLog = callInfo.Arg<GetLog>();
				return getLog;
			});

		// Act
		var result = await _sut.GetMessage(messageId, encryptionKeySha256, null, httpContextClientInfo);

		// Assert
		result.Should().BeOfType<SuccessResult<GetSecretMessageResponse>>();
		capturedLog.Should().NotBeNull();
		capturedLog!.RecipientCity.Should().Be("Bucharest");
		capturedLog.RecipientRegion.Should().Be("Bucuresti");
		capturedLog.RecipientCountry.Should().Be("Romania");
		capturedLog.RecipientCountryCode.Should().Be("RO");
		capturedLog.RecipientLocationSource.Should().Be(Constants.LocationSources.Device);
		capturedLog.RecipientIsp.Should().Be("Example ISP");
		capturedLog.RecipientTimezone.Should().Be("Europe/Bucharest");
		capturedLog.RecipientUtcOffsetMinutes.Should().Be(180);
		capturedLog.RecipientLanguage.Should().Be("ro-RO");
		capturedLog.OtpVerified.Should().BeFalse();

		await _recipientLocationService.Received(1).ResolveAsync(
			"8.8.4.4",
			44.4268,
			26.1025,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetMessage_ShouldStillSucceed_WhenLocationLookupFails()
	{
		// Arrange
		var encryptionKeySha256 = _fixture.Create<string>();
		var secretMessage = _fixture.Create<SecretMessage>() with
		{
			EncryptionKeySha256 = KeyHashVerifier.Create(encryptionKeySha256),
			Otp = new OtpSettings
			{
				Required = false,
				RecipientsEmail = ""
			}
		};
		var messageId = secretMessage.Id;
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		var getLog = _fixture.Create<GetLog>();
		GetLog? capturedLog = null;

		_secretMessagesService.Get(messageId)
			.Returns(secretMessage);

		_secretMessagesService.Delete(messageId)
			.Returns(true);

		_recipientLocationService
			.ResolveAsync(Arg.Any<string?>(), Arg.Any<double?>(), Arg.Any<double?>(), Arg.Any<CancellationToken>())
			.Returns((RecipientLocation?)null);

		_getLogsService.CreateNewLog(Arg.Any<GetLog>())
			.Returns(callInfo =>
			{
				capturedLog = callInfo.Arg<GetLog>();
				return getLog;
			});

		// Act
		var result = await _sut.GetMessage(messageId, encryptionKeySha256, null, httpContextClientInfo);

		// Assert
		result.Should().BeOfType<SuccessResult<GetSecretMessageResponse>>();
		capturedLog.Should().NotBeNull();
		capturedLog!.RecipientCity.Should().BeNull();
		capturedLog.RecipientLocationSource.Should().BeNull();
	}

	[Fact]
	public async Task SendOtp_ShouldReturnSuccessNoContent_WhenCachedOtpIsStillValid()
	{
		// Arrange
		var messageId = _fixture.Create<string>();
		var otpSettings = new OtpSettings
		{
			Required = true,
			RecipientsEmail = _fixture.Create<string>()
		};
		var cachedOtp = _fixture.Create<OneTimePassword>();

		_secretMessagesService.Exists(messageId)
			.Returns((true, otpSettings));

		_memoryCacheService.GetValue<OneTimePassword>(messageId, Constants.MemoryKeys.SecretMessageOtp, remove: false)
			.Returns((true, cachedOtp));

		_otpService.IsExpired(cachedOtp)
			.Returns(false);

		// Act
		var result = await _sut.SendOtp(messageId);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.SuccessNoContent());
		_otpService.DidNotReceive().Generate();
		await _sendGridEmailService.DidNotReceive()
			.SendOtp(Arg.Any<OneTimePassword>(), Arg.Any<string>(), Arg.Any<string>());
		_memoryCacheService.DidNotReceive()
			.SetValue(Arg.Any<string>(), Arg.Any<OneTimePassword>(), Arg.Any<string>());
	}

	[Fact]
	public async Task SendOtp_ShouldGenerateAndSendNewOtp_WhenCachedOtpHasExpired()
	{
		// Arrange
		var messageId = _fixture.Create<string>();
		var otpSettings = new OtpSettings
		{
			Required = true,
			RecipientsEmail = _fixture.Create<string>()
		};
		var expiredOtp = _fixture.Create<OneTimePassword>();
		var newOtp = _fixture.Create<OneTimePassword>();

		_secretMessagesService.Exists(messageId)
			.Returns((true, otpSettings));

		_memoryCacheService.GetValue<OneTimePassword>(messageId, Constants.MemoryKeys.SecretMessageOtp, remove: false)
			.Returns((true, expiredOtp));

		_otpService.IsExpired(expiredOtp)
			.Returns(true);

		_otpService.Generate()
			.Returns(newOtp);

		_sendGridEmailService.SendOtp(newOtp, messageId, otpSettings.RecipientsEmail)
			.Returns(true);

		// Act
		var result = await _sut.SendOtp(messageId);

		// Assert
		result.Should().BeEquivalentTo(ApiResult.SuccessNoContent());
		_memoryCacheService.Received().SetValue(messageId, newOtp, Constants.MemoryKeys.SecretMessageOtp);
	}

	[Fact]
	public async Task ValidateOtp_ShouldAllowRetry_WhenOtpIsInvalidButRetriesRemain()
	{
		// Arrange
		var messageId = _fixture.Create<string>();
		var otpCode = _fixture.Create<string>();
		var otp = _fixture.Create<OneTimePassword>();
		var otpSettings = new OtpSettings
		{
			Required = true,
			RecipientsEmail = _fixture.Create<string>()
		};

		_secretMessagesService.Exists(messageId)
			.Returns((true, otpSettings));

		_memoryCacheService.GetValue<OneTimePassword>(messageId, Constants.MemoryKeys.SecretMessageOtp, remove: false)
			.Returns((true, otp));

		_otpService.Validate(otpCode, otp)
			.Returns((false, true, false));

		// Act
		var result = await _sut.ValidateOtp(messageId, otpCode);

		// Assert
		result.Should().BeEquivalentTo(ApiResult<ValidateSecretMessageOtpResponse>.SuccessWithData(new ValidateSecretMessageOtpResponse
		{
			IsValid = false,
			CanRetry = true,
			HasExpired = false,
			AuthToken = null
		}));
		_memoryCacheService.DidNotReceive().RemoveValue(messageId, Constants.MemoryKeys.SecretMessageOtp);
		_jwtService.DidNotReceive().GenerateToken(Arg.Any<string>());
	}

	[Fact]
	public async Task GetMessage_ShouldReturnSuccessWithData_WhenOtpIsRequiredAndJwtTokenIsValid()
	{
		// Arrange
		var encryptionKeySha256 = _fixture.Create<string>();
		var secretMessage = _fixture.Create<SecretMessage>() with
		{
			EncryptionKeySha256 = KeyHashVerifier.Create(encryptionKeySha256),
			Otp = new OtpSettings
			{
				Required = true,
				RecipientsEmail = _fixture.Create<string>()
			}
		};
		var messageId = secretMessage.Id;
		var jwtToken = _fixture.Create<string>();
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		var getLog = _fixture.Create<GetLog>();
		var secretMessageDeliveryNotification = getLog.ToSecretMessageDeliveryNotification();

		_secretMessagesService.Get(messageId)
			.Returns(secretMessage);

		_jwtService.ValidateToken(jwtToken, messageId)
			.Returns(true);

		_getLogsService.CreateNewLog(Arg.Any<GetLog>())
			.Returns(getLog);

		_secretMessagesService.Delete(messageId)
			.Returns(true);

		_notificationHubService.SendNotification(secretMessage.CreatorClientId, null, secretMessageDeliveryNotification)
			.Returns(true);

		// Act
		var result = await _sut.GetMessage(messageId, encryptionKeySha256, jwtToken, httpContextClientInfo);

		// Assert
		result.Should().BeEquivalentTo(ApiResult<GetSecretMessageResponse>.SuccessWithData(new GetSecretMessageResponse
		{
			Data = secretMessage.Data,
			DeliveryNotificationSent = true
		}));
		_jwtService.Received(1).ValidateToken(jwtToken, messageId);
		await _secretMessagesService.Received(1).Delete(messageId);
	}
}