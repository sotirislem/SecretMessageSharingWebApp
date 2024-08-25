using System.Text.Json;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.UnitTests.MappingsTests;

public class DomainToEntityDtoMapperTests
{
	private readonly IFixture _fixture;

	public DomainToEntityDtoMapperTests()
	{
		_fixture = new Fixture();
	}

	[Fact]
	public void ToEntity_ShouldMapSecretMessageToEntityCorrectly_WhenOtpIsRequired()
	{
		// Arrange
		var otp = _fixture.Build<Models.Common.OtpSettings>()
			.With(o => o.Required, true)
			.Create();

		var secretMessage = _fixture.Build<SecretMessage>()
			.With(sm => sm.Otp, otp)
			.Create();

		// Act
		var result = secretMessage.ToEntity();

		// Assert
		result.CreatedDateTime.Should().Be(secretMessage.CreatedDateTime);
		result.JsonData.Should().Be(JsonSerializer.Serialize(secretMessage.Data));
		result.CreatorClientId.Should().Be(secretMessage.CreatorClientId);
		result.CreatorIP.Should().Be(secretMessage.CreatorIP);
		result.CreatorClientInfo.Should().Be(secretMessage.CreatorClientInfo);
		result.EncryptionKeySha256.Should().Be(secretMessage.EncryptionKeySha256);

		result.Otp.Should().NotBeNull();
		result.Otp!.RecipientsEmail.Should().Be(secretMessage.Otp.RecipientsEmail);
	}

	[Fact]
	public void ToEntity_ShouldMapSecretMessageToEntityCorrectly_WhenOtpIsNotRequired()
	{
		// Arrange
		var otp = _fixture.Build<Models.Common.OtpSettings>()
			.With(o => o.Required, false)
			.Create();

		var secretMessage = _fixture.Build<SecretMessage>()
			.With(sm => sm.Otp, otp)
			.Create();

		// Act
		var result = secretMessage.ToEntity();

		// Assert
		result.CreatedDateTime.Should().Be(secretMessage.CreatedDateTime);
		result.JsonData.Should().Be(JsonSerializer.Serialize(secretMessage.Data));
		result.CreatorClientId.Should().Be(secretMessage.CreatorClientId);
		result.CreatorIP.Should().Be(secretMessage.CreatorIP);
		result.CreatorClientInfo.Should().Be(secretMessage.CreatorClientInfo);
		result.EncryptionKeySha256.Should().Be(secretMessage.EncryptionKeySha256);

		result.Otp.Should().BeNull();
	}

	[Fact]
	public void ToEntity_MapsGetLogToEntityCorrectly()
	{
		// Arrange
		var getLog = _fixture.Create<GetLog>();

		// Act
		var result = getLog.ToEntity();

		// Assert
		result.SecretMessageId.Should().Be(getLog.SecretMessageId);
		result.RequestDateTime.Should().Be(getLog.RequestDateTime);
		result.RequestCreatorIP.Should().Be(getLog.RequestCreatorIP);
		result.RequestClientInfo.Should().Be(getLog.RequestClientInfo);
		result.SecretMessageCreatedDateTime.Should().Be(getLog.SecretMessageCreatedDateTime);
		result.SecretMessageCreatorIP.Should().Be(getLog.SecretMessageCreatorIP);
		result.SecretMessageCreatorClientInfo.Should().Be(getLog.SecretMessageCreatorClientInfo);
	}
}