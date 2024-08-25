using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Validators;

namespace SecretMessageSharingWebApp.UnitTests.ValidatorsTests;

public class ValidateSecretMessageOtpRequestValidatorTests
{
	private readonly ValidateSecretMessageOtpRequestValidator _validator;

	public ValidateSecretMessageOtpRequestValidatorTests()
	{
		_validator = new ValidateSecretMessageOtpRequestValidator();
	}

	[Theory]
	[InlineData("12345678")]		// Valid case
	public void ValidateSecretMessageOtpRequestValidator_ShouldNotHaveError_WhenOtpCodeIsValid(string otpCode)
	{
		// Arrange
		var request = new ValidateSecretMessageOtpRequest
		{
			OtpCode = otpCode
		};

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldNotHaveValidationErrorFor(r => r.OtpCode);
	}

	[Theory]
	[InlineData(null)]				// Null value
	[InlineData("")]				// Empty string
	[InlineData("12345")]			// Too short
	[InlineData("12345678901")]		// Too long
	[InlineData("12345abc")]		// Contains non-numeric characters
	public void ValidateSecretMessageOtpRequestValidator_ShouldHaveError_WhenOtpCodeIsInvalid(string otpCode)
	{
		// Arrange
		var request = new ValidateSecretMessageOtpRequest
		{
			OtpCode = otpCode
		};

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldHaveValidationErrorFor(r => r.OtpCode);
	}
}