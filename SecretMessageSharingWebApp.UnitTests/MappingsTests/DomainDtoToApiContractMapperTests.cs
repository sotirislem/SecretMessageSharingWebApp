using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.UnitTests.MappingsTests;

public class DomainDtoToApiContractMapperTests
{
	private readonly IFixture _fixture;

	public DomainDtoToApiContractMapperTests()
	{
		_fixture = new Fixture();
	}

	[Fact]
	public void ToSecretMessageDeliveryNotification_ShouldMapAllFieldsCorrectly()
	{
		// Arrange
		var getLog = _fixture.Build<GetLog>()
			.With(gl => gl.SecretMessageCreatedDateTime, DateTime.UtcNow)
			.With(gl => gl.RequestDateTime, DateTime.UtcNow.AddMinutes(5))
			.Create();

		// Act
		var result = getLog.ToSecretMessageDeliveryNotification();

		// Assert
		result.MessageId.Should().Be(getLog.SecretMessageId);
		result.MessageCreatedOn.Should().Be(getLog.SecretMessageCreatedDateTime!.Value);
		result.MessageDeliveredOn.Should().Be(getLog.RequestDateTime);
		result.RecipientIP.Should().Be(getLog.RequestCreatorIP.MaskIpAddress());
		result.RecipientClientInfo.Should().Be(getLog.RequestClientInfo);
		result.RecipientCity.Should().Be(getLog.RecipientCity);
		result.RecipientRegion.Should().Be(getLog.RecipientRegion);
		result.RecipientCountry.Should().Be(getLog.RecipientCountry);
		result.RecipientCountryCode.Should().Be(getLog.RecipientCountryCode);
		result.RecipientLocationSource.Should().Be(getLog.RecipientLocationSource);
		result.RecipientIsp.Should().Be(getLog.RecipientIsp);
		result.RecipientTimezone.Should().Be(getLog.RecipientTimezone);
		result.RecipientUtcOffsetMinutes.Should().Be(getLog.RecipientUtcOffsetMinutes);
		result.RecipientLanguage.Should().Be(getLog.RecipientLanguage);
		result.OtpVerified.Should().Be(getLog.OtpVerified);
	}

	[Fact]
	public void ToSecretMessageDeliveryNotification_ShouldMaskIPv4Address()
	{
		// Arrange
		var getLog = _fixture.Build<GetLog>()
			.With(gl => gl.RequestCreatorIP, "192.168.1.100")
			.Create();

		// Act
		var result = getLog.ToSecretMessageDeliveryNotification();

		// Assert
		result.RecipientIP.Should().Be("192.168.1.xxx");
	}

	[Fact]
	public void ToSecretMessageDeliveryNotification_ShouldMaskIpv6AddressToItsNetworkPrefix()
	{
		// Arrange
		var getLog = _fixture.Build<GetLog>()
			.With(gl => gl.RequestCreatorIP, "fe80::f2de:f1ff:fe41:dc92")
			.Create();

		// Act
		var result = getLog.ToSecretMessageDeliveryNotification();

		// Assert
		result.RecipientIP.Should().Be("fe80::/64");
	}

	[Fact]
	public void ToSecretMessageDeliveryNotification_ShouldHandleNullIpAddress()
	{
		// Arrange
		var getLog = _fixture.Build<GetLog>()
			.With(gl => gl.RequestCreatorIP, (string?)null)
			.Create();

		// Act
		var result = getLog.ToSecretMessageDeliveryNotification();

		// Assert
		result.RecipientIP.Should().BeNull();
	}

	[Fact]
	public void ToSecretMessageDeliveryNotification_ShouldHandleNullValues()
	{
		// Arrange
		var getLog = _fixture.Build<GetLog>()
			.With(gl => gl.SecretMessageCreatedDateTime, (DateTime?)null)
			.Create();

		// Act
		var action = () => getLog.ToSecretMessageDeliveryNotification();

		// Assert
		action.Should().Throw<InvalidOperationException>("because SecretMessageCreatedDateTime cannot be null when mapping to SecretMessageDeliveryNotification");
	}
}