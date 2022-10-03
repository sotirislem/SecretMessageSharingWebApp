using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public sealed class OtpServiceTests
{
	private readonly OtpService _sut;
	private readonly Fixture _fixture = new();

	public OtpServiceTests()
	{
		_sut = new OtpService();
	}

	[Fact]
	public void Generate_ShouldReturnAValidOneTimePasswordObject_WhenExecuted()
	{
		// Arrange
		// -

		// Act
		var result = _sut.Generate();

		// Assert
		result.Code.Length.Should().Be(Constants.OtpSize);
		result.CreatedTimestamp.Should().BeCloseTo(DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 1);
		result.AvailableValidationAttempts.Should().Be(Constants.OtpMaxValidationRetries);
	}

	[Fact]
	public void Validate_ShouldReturnIsValidEqualsTrue_WhenExecutedAndOtpIsValid()
	{
		// Arrange
		var oneTimePassword = _fixture.Build<OneTimePassword>()
			.With(x => x.CreatedTimestamp, DateTimeOffset.UtcNow.ToUnixTimeSeconds())
			.With(x => x.AvailableValidationAttempts, Constants.OtpMaxValidationRetries)
			.Create();
		var otpInputCode = oneTimePassword.Code;

		// Act
		var result = _sut.Validate(otpInputCode, oneTimePassword);

		// Assert
		result.isValid.Should().BeTrue();
		result.canRetry.Should().BeTrue();
		result.hasExpired.Should().BeFalse();
	}

	[Fact]
	public void Validate_ShouldReturnCanRetryEqualsTrue_WhenOtpIsInvalidButCanRetry()
	{
		// Arrange
		var oneTimePassword = _fixture.Build<OneTimePassword>()
			.With(x => x.CreatedTimestamp, DateTimeOffset.UtcNow.ToUnixTimeSeconds())
			.With(x => x.AvailableValidationAttempts, Constants.OtpMaxValidationRetries)
			.Create();
		var otpInputCode = _sut.Generate().Code;

		// Act
		var result = _sut.Validate(otpInputCode, oneTimePassword);

		// Assert
		result.isValid.Should().BeFalse();
		result.canRetry.Should().BeTrue();
		result.hasExpired.Should().BeFalse();
	}

	[Fact]
	public void Validate_ShouldReturnCanRetryEqualsFalse_WhenOtpMaxValidationRetriesReached()
	{
		// Arrange
		var oneTimePassword = _fixture.Build<OneTimePassword>()
			.With(x => x.CreatedTimestamp, DateTimeOffset.UtcNow.ToUnixTimeSeconds())
			.With(x => x.AvailableValidationAttempts, 0)
			.Create();
		var otpInputCode = _sut.Generate().Code;

		// Act
		var result = _sut.Validate(otpInputCode, oneTimePassword);

		// Assert
		result.isValid.Should().BeFalse();
		result.canRetry.Should().BeFalse();
		result.hasExpired.Should().BeTrue();
	}

	[Fact]
	public void Validate_ShouldReturnHasExpiredEqualsTrue_WhenOtpHasExpired()
	{
		// Arrange
		var oneTimePassword = _fixture.Build<OneTimePassword>()
			.With(x => x.CreatedTimestamp, DateTimeOffset.UtcNow.ToUnixTimeSeconds() - TimeSpan.FromMinutes(Constants.OtpExpirationMinutes).TotalSeconds - 1)
			.With(x => x.AvailableValidationAttempts, Constants.OtpMaxValidationRetries)
			.Create();
		var otpInputCode = oneTimePassword.Code;

		// Act
		var result = _sut.Validate(otpInputCode, oneTimePassword);

		// Assert
		result.isValid.Should().BeFalse();
		result.canRetry.Should().BeTrue();
		result.hasExpired.Should().BeTrue();
	}
}