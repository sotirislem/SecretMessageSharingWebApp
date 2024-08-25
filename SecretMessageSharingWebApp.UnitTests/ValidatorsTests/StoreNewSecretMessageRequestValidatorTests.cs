using System.Security.Cryptography;
using System.Text;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Validators;

namespace SecretMessageSharingWebApp.UnitTests.ValidatorsTests;

public class StoreNewSecretMessageRequestValidatorTests
{
	private readonly StoreNewSecretMessageRequestValidator _validator;
	private readonly Fixture _fixture;

	public StoreNewSecretMessageRequestValidatorTests()
	{
		_validator = new StoreNewSecretMessageRequestValidator();
		_fixture = new Fixture();
	}

	[Fact]
	public void StoreNewSecretMessageRequestValidator_ShouldHaveError_WhenClientIdIsNotValidGuid()
	{
		// Arrange
		var request = _fixture.Build<StoreNewSecretMessageRequest>()
			.With(r => r.ClientId, "invalid-guid")
			.Create();

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldHaveValidationErrorFor(r => r.ClientId);
	}

	[Fact]
	public void StoreNewSecretMessageRequestValidator_ShouldNotHaveError_WhenClientIdIsValidGuid()
	{
		// Arrange
		var request = _fixture.Build<StoreNewSecretMessageRequest>()
			.With(r => r.ClientId, Guid.NewGuid().ToString())
			.Create();

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldNotHaveValidationErrorFor(r => r.ClientId);
	}

	[Fact]
	public void StoreNewSecretMessageRequestValidator_ShouldHaveError_WhenSecretMessageDataIsNull()
	{
		// Arrange
		var request = _fixture.Build<StoreNewSecretMessageRequest>()
			.With(r => r.SecretMessageData, null as SecretMessageData)
			.Create();

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldHaveValidationErrorFor(r => r.SecretMessageData);
	}

	[Fact]
	public void StoreNewSecretMessageRequestValidator_ShouldHaveError_WhenSecretMessageDataPropertiesAreEmpty()
	{
		// Arrange
		var secretMessageData = new SecretMessageData { IV = "", Salt = "", CT = "" };
		var request = _fixture.Build<StoreNewSecretMessageRequest>()
			.With(r => r.SecretMessageData, secretMessageData)
			.Create();

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldHaveValidationErrorFor(r => r.SecretMessageData.IV);
		result.ShouldHaveValidationErrorFor(r => r.SecretMessageData.Salt);
		result.ShouldHaveValidationErrorFor(r => r.SecretMessageData.CT);
	}

	[Fact]
	public void StoreNewSecretMessageRequestValidator_ShouldNotHaveError_WhenSecretMessageDataPropertiesAreNotEmpty()
	{
		// Arrange
		var secretMessageData = _fixture.Build<SecretMessageData>()
			.With(smd => smd.IV, "non-empty-IV")
			.With(smd => smd.Salt, "non-empty-Salt")
			.With(smd => smd.CT, "non-empty-CT")
			.Create();

		var request = _fixture.Build<StoreNewSecretMessageRequest>()
			.With(r => r.SecretMessageData, secretMessageData)
			.Create();

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldNotHaveValidationErrorFor(r => r.SecretMessageData.IV);
		result.ShouldNotHaveValidationErrorFor(r => r.SecretMessageData.Salt);
		result.ShouldNotHaveValidationErrorFor(r => r.SecretMessageData.CT);
	}

	[Fact]
	public void StoreNewSecretMessageRequestValidator_ShouldHaveError_WhenOtpIsNull()
	{
		// Arrange
		var request = _fixture.Build<StoreNewSecretMessageRequest>()
			.With(r => r.Otp, null as OtpSettings)
			.Create();

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldHaveValidationErrorFor(r => r.Otp);
	}

	[Fact]
	public void StoreNewSecretMessageRequestValidator_ShouldHaveError_WhenOtpRequiredAndRecipientsEmailIsInvalid()
	{
		// Arrange
		var otp = _fixture.Build<OtpSettings>()
			.With(o => o.Required, true)
			.With(o => o.RecipientsEmail, "invalid-email")
			.Create();

		var request = _fixture.Build<StoreNewSecretMessageRequest>()
			.With(r => r.Otp, otp)
			.Create();

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldHaveValidationErrorFor(r => r.Otp.RecipientsEmail);
	}

	[Fact]
	public void StoreNewSecretMessageRequestValidator_ShouldNotHaveError_WhenOtpRequiredAndRecipientsEmailIsValid()
	{
		// Arrange
		var otp = _fixture.Build<OtpSettings>()
			.With(o => o.Required, true)
			.With(o => o.RecipientsEmail, "valid@example.com")
			.Create();

		var request = _fixture.Build<StoreNewSecretMessageRequest>()
			.With(r => r.Otp, otp)
			.Create();

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldNotHaveValidationErrorFor(r => r.Otp.RecipientsEmail);
	}

	[Fact]
	public void StoreNewSecretMessageRequestValidator_ShouldNotHaveError_WhenOtpIsNotRequired()
	{
		// Arrange
		var otp = _fixture.Build<OtpSettings>()
			.With(o => o.Required, false)
			.Create();

		var request = _fixture.Build<StoreNewSecretMessageRequest>()
			.With(r => r.Otp, otp)
			.Create();

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldNotHaveValidationErrorFor(r => r.Otp.RecipientsEmail);
	}

	[Fact]
	public void StoreNewSecretMessageRequestValidator_ShouldHaveError_WhenEncryptionKeySha256IsInvalid()
	{
		// Arrange
		var request = _fixture.Build<StoreNewSecretMessageRequest>()
			.With(r => r.EncryptionKeySha256, "invalid-sha256")
			.Create();

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldHaveValidationErrorFor(r => r.EncryptionKeySha256);
	}

	[Fact]
	public void StoreNewSecretMessageRequestValidator_ShouldNotHaveError_WhenEncryptionKeySha256IsValid()
	{
		// Arrange
		var randomString = Guid.NewGuid().ToString();
		using var sha256 = SHA256.Create();
		var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(randomString));
		var hashString = Convert.ToHexString(hashBytes).ToLowerInvariant();

		var request = _fixture.Build<StoreNewSecretMessageRequest>()
			.With(r => r.EncryptionKeySha256, hashString)
			.Create();

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldNotHaveValidationErrorFor(r => r.EncryptionKeySha256);
	}
}