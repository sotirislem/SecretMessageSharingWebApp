using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public class OtpServiceTests
{
	private readonly IFixture _fixture;
	private readonly IDateTimeProviderService _dateTimeProviderService;

	private readonly OtpService _sut;

	public OtpServiceTests()
	{
		_fixture = new Fixture();
		_dateTimeProviderService = Substitute.For<IDateTimeProviderService>();

		_sut = new OtpService(_dateTimeProviderService);
	}

	[Fact]
	public void Generate_ShouldReturnOtpWithExpectedFormat()
	{
		// Arrange
		var now = DateTime.UtcNow;
		_dateTimeProviderService.UtcNow().Returns(now);

		// Act
		var result = _sut.Generate();

		// Assert
		result.Code.Should().HaveLength(Constants.OtpSize);
		result.ExpiresAt.Should().Be(now.AddMinutes(Constants.OtpExpirationMinutes));
	}

	[Fact]
	public void IsExpired_ShouldReturnTrue_WhenOtpIsExpired()
	{
		// Arrange
		var now = DateTime.UtcNow;
		var otp = new OneTimePassword
		{
			Code = _fixture.Create<string>(),
			ExpiresAt = now.AddMinutes(-Constants.OtpExpirationMinutes)
		};

		_dateTimeProviderService.UtcNow().Returns(now);

		// Act
		var result = _sut.IsExpired(otp);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsExpired_ShouldReturnTrue_WhenMaxValidationAttemptsExceeded()
	{
		// Arrange
		var now = DateTime.UtcNow;
		var otp = new OneTimePassword
		{
			Code = _fixture.Create<string>(),
			ExpiresAt = now.AddMinutes(Constants.OtpExpirationMinutes)
		};

		// Simulate multiple validation attempts
		for (int i = 0; i < Constants.OtpMaxValidationAttempts; i++)
		{
			otp.Validate(_fixture.Create<string>());
		}

		// Act
		var result = _sut.IsExpired(otp);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void Validate_ShouldReturnExpectedResults_WhenOtpIsValid()
	{
		// Arrange
		var otpCode = _fixture.Create<string>();
		var now = DateTime.UtcNow;
		var otp = new OneTimePassword
		{
			Code = otpCode,
			ExpiresAt = now.AddMinutes(Constants.OtpExpirationMinutes)
		};
		var otpInputCode = otpCode;

		_dateTimeProviderService.UtcNow().Returns(now);

		// Act
		var result = _sut.Validate(otpInputCode, otp);

		// Assert
		result.isValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_ShouldReturnExpectedResults_WhenOtpIsExpired()
	{
		// Arrange
		var otpCode = _fixture.Create<string>();
		var now = DateTime.UtcNow;
		var otp = new OneTimePassword
		{
			Code = otpCode,
			ExpiresAt = now.AddMinutes(-Constants.OtpExpirationMinutes)
		};

		var otpInputCode = otpCode;

		_dateTimeProviderService.UtcNow().Returns(now);

		// Act
		var result = _sut.Validate(otpInputCode, otp);

		// Assert
		result.isValid.Should().BeFalse();
		result.canRetry.Should().BeFalse();
		result.hasExpired.Should().BeTrue();
	}

	[Fact]
	public void Validate_ShouldReturnExpectedResults_WhenOtpCodeIsInvalid()
	{
		// Arrange
		var otpCode = _fixture.Create<string>();
		var now = DateTime.UtcNow;
		var otp = new OneTimePassword
		{
			Code = otpCode,
			ExpiresAt = now.AddMinutes(Constants.OtpExpirationMinutes)
		};

		var otpInputCode = _fixture.Create<string>();

		_dateTimeProviderService.UtcNow().Returns(now);

		// Act
		var result = _sut.Validate(otpInputCode, otp);

		// Assert
		result.isValid.Should().BeFalse();
		result.canRetry.Should().BeTrue();
		result.hasExpired.Should().BeFalse();
	}
}