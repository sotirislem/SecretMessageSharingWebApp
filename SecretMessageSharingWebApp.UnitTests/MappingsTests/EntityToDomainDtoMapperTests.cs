using System.Text.Json;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Common;
using OtpSettings = SecretMessageSharingWebApp.Data.Entities.OtpSettings;

namespace SecretMessageSharingWebApp.UnitTests.MappingsTests;

public class EntityToDomainDtoMapperTests
{
	private readonly IFixture _fixture;

	public EntityToDomainDtoMapperTests()
	{
		_fixture = new Fixture();
	}

	[Fact]
	public void ToDomain_ShouldMapSecretMessageEntityToDomainCorrectly_WhenOtpIsNotNull()
	{
		// Arrange
		var entity = _fixture.Build<SecretMessageEntity>()
			.With(x => x.Otp, _fixture.Create<OtpSettings>())
			.With(x => x.JsonData, JsonSerializer.Serialize(_fixture.Create<SecretMessageData>()))
			.Create();

		// Act
		var result = entity.ToDomain();

		// Assert
		result.Id.Should().Be(entity.Id);
		result.CreatedDateTime.Should().Be(entity.CreatedDateTime);
		result.Data.Should().BeEquivalentTo(JsonSerializer.Deserialize<SecretMessageData>(entity.JsonData));
		result.EncryptionKeySha256.Should().Be(entity.EncryptionKeySha256);
		result.CreatorClientId.Should().Be(entity.CreatorClientId);
		result.CreatorTabId.Should().Be(entity.CreatorTabId);
		result.CreatorIP.Should().Be(entity.CreatorIP);
		result.CreatorClientInfo.Should().Be(entity.CreatorClientInfo);

		result.Otp.Should().NotBeNull();
		result.Otp.Should().BeEquivalentTo(entity.Otp.ToDomain());
	}

	[Fact]
	public void ToDomain_ShouldMapSecretMessageEntityToDomainCorrectly_WhenOtpIsNull()
	{
		// Arrange
		var entity = _fixture.Build<SecretMessageEntity>()
			.With(x => x.Otp, (OtpSettings?)null)
			.With(x => x.JsonData, JsonSerializer.Serialize(_fixture.Create<SecretMessageData>()))
			.Create();

		// Act
		var result = entity.ToDomain();

		// Assert
		result.Id.Should().Be(entity.Id);
		result.CreatedDateTime.Should().Be(entity.CreatedDateTime);
		result.Data.Should().BeEquivalentTo(JsonSerializer.Deserialize<SecretMessageData>(entity.JsonData));
		result.EncryptionKeySha256.Should().Be(entity.EncryptionKeySha256);
		result.CreatorClientId.Should().Be(entity.CreatorClientId);
		result.CreatorTabId.Should().Be(entity.CreatorTabId);
		result.CreatorIP.Should().Be(entity.CreatorIP);
		result.CreatorClientInfo.Should().Be(entity.CreatorClientInfo);

		result.Otp.Should().NotBeNull();
		result.Otp.Required.Should().BeFalse();
		result.Otp.RecipientsEmail.Should().BeEmpty();
	}

	[Fact]
	public void ToDomain_ShouldMapOtpSettingsEntityToDomainCorrectly()
	{
		// Arrange
		var entity = _fixture.Create<Data.Entities.OtpSettings>();

		// Act
		var result = entity.ToDomain();

		// Assert
		result.Required.Should().Be(entity.Required);
		result.RecipientsEmail.Should().Be(entity.RecipientsEmail);
	}

	[Fact]
	public void ToDomain_ShouldMapGetLogEntityToDomainCorrectly()
	{
		// Arrange
		var entity = _fixture.Create<GetLogEntity>();

		// Act
		var result = entity.ToDomain();

		// Assert
		result.Id.Should().Be(entity.Id);
		result.RequestDateTime.Should().Be(entity.RequestDateTime);
		result.RequestCreatorIP.Should().Be(entity.RequestCreatorIP);
		result.RequestClientInfo.Should().Be(entity.RequestClientInfo);
		result.SecretMessageId.Should().Be(entity.SecretMessageId);
		result.SecretMessageCreatedDateTime.Should().Be(entity.SecretMessageCreatedDateTime);
		result.SecretMessageCreatorIP.Should().Be(entity.SecretMessageCreatorIP);
		result.SecretMessageCreatorClientInfo.Should().Be(entity.SecretMessageCreatorClientInfo);
	}

	[Fact]
	public void ToRecentlyStoredSecretMessage_ShouldMapSecretMessageEntityToRecentlyStoredSecretMessageCorrectly()
	{
		// Arrange
		var entity = _fixture.Create<SecretMessageEntity>();

		// Act
		var result = entity.ToRecentlyStoredSecretMessage();

		// Assert
		result.Id.Should().Be(entity.Id);
		result.CreatedDateTime.Should().Be(entity.CreatedDateTime);
		result.DeliveryDetails.Should().BeNull();
	}

	[Fact]
	public void ToRecentlyStoredSecretMessage_ShouldMapGetLogEntityToRecentlyStoredSecretMessageCorrectly()
	{
		// Arrange
		var entity = _fixture.Create<GetLogEntity>();

		// Act
		var result = entity.ToRecentlyStoredSecretMessage();

		// Assert
		result.Id.Should().Be(entity.SecretMessageId);
		result.CreatedDateTime.Should().Be(entity.SecretMessageCreatedDateTime!.Value);
		result.DeliveryDetails.Should().NotBeNull();
		result.DeliveryDetails!.DeliveredAt.Should().Be(entity.RequestDateTime);
		result.DeliveryDetails.RecipientIP.Should().Be(entity.RequestCreatorIP.MaskIpAddress());
		result.DeliveryDetails.RecipientClientInfo.Should().Be(entity.RequestClientInfo);
		result.DeliveryDetails.RecipientCity.Should().Be(entity.RecipientCity);
		result.DeliveryDetails.RecipientRegion.Should().Be(entity.RecipientRegion);
		result.DeliveryDetails.RecipientCountry.Should().Be(entity.RecipientCountry);
		result.DeliveryDetails.RecipientCountryCode.Should().Be(entity.RecipientCountryCode);
		result.DeliveryDetails.RecipientLocationSource.Should().Be(entity.RecipientLocationSource);
		result.DeliveryDetails.RecipientIsp.Should().Be(entity.RecipientIsp);
		result.DeliveryDetails.RecipientTimezone.Should().Be(entity.RecipientTimezone);
		result.DeliveryDetails.RecipientUtcOffsetMinutes.Should().Be(entity.RecipientUtcOffsetMinutes);
		result.DeliveryDetails.RecipientLanguage.Should().Be(entity.RecipientLanguage);
		result.DeliveryDetails.OtpVerified.Should().Be(entity.OtpVerified);
	}
}