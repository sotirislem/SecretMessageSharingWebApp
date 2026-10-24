using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Validators;

namespace SecretMessageSharingWebApp.UnitTests.ValidatorsTests;

public class GetSecretMessageRequestValidatorTests
{
	private readonly GetSecretMessageRequestValidator _validator;

	public GetSecretMessageRequestValidatorTests()
	{
		_validator = new GetSecretMessageRequestValidator();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("invalid-sha256")]
	[InlineData("000000000000000000000000000000000000000000000000000000000000000")]	// 63 characters
	[InlineData("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]	// 64 non-hex characters
	public void GetSecretMessageRequestValidator_ShouldHaveError_WhenHeaderIsMissingOrInvalid(string encryptionKeySha256)
	{
		// Arrange
		var request = CreateRequest(encryptionKeySha256);

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldHaveValidationErrorFor(r => r.EncryptionKeySha256);
	}

	[Fact]
	public void GetSecretMessageRequestValidator_ShouldNotHaveError_WhenHeaderIsValid()
	{
		// Arrange
		var request = CreateRequest(CreateSha256Hex());

		// Act
		var result = _validator.TestValidate(request);

		// Assert
		result.ShouldNotHaveValidationErrorFor(r => r.EncryptionKeySha256);
	}

	[Fact]
	public void GetSecretMessageRequest_ShouldBindOnlyFromRequiredHeader()
	{
		// Arrange
		var properties = typeof(GetSecretMessageRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);

		// Assert
		properties.Should().ContainSingle()
			.Which.Name.Should().Be(nameof(GetSecretMessageRequest.EncryptionKeySha256));

		var property = properties.Single();
		var attributes = property.GetCustomAttributes(inherit: false);

		var attributeNames = attributes.Select(a => a.GetType().Name).ToArray();
		attributeNames.Should().NotContain("QueryParamAttribute");
		attributeNames.Should().NotContain("BindFromAttribute");

		var fromHeader = attributes.Single(a => a.GetType().Name == "FromHeaderAttribute");
		fromHeader.GetType().GetProperty("HeaderName")!.GetValue(fromHeader).Should().Be("X-Key-Hash");
		fromHeader.GetType().GetProperty("IsRequired")!.GetValue(fromHeader).Should().Be(true);
	}

	private static GetSecretMessageRequest CreateRequest(string? header = null) => new()
	{
		EncryptionKeySha256 = header!
	};

	private static string CreateSha256Hex()
	{
		var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

		return Convert.ToHexString(hashBytes).ToLowerInvariant();
	}
}
