using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Models;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Security;

namespace SecretMessageSharingWebApp.UnitTests.MappingsTests;

public class ApiContractToDomainDtoMapperTests
{
	private readonly IFixture _fixture;

	public ApiContractToDomainDtoMapperTests()
	{
		_fixture = new Fixture();
	}

	[Fact]
	public void ToSecretMessage_ShouldMapAllFieldsCorrectly()
	{
		// Arrange
		var tabId = Guid.NewGuid().ToString();
		var request = _fixture.Build<StoreNewSecretMessageRequest>()
			.With(x => x.TabId, tabId)
			.Create();
		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		// Act
		var result = request.ToSecretMessage(httpContextClientInfo);

		// Assert
		result.CreatedDateTime.Should().Be(httpContextClientInfo.RequestDateTime);
		result.Data.Should().NotBeNull();
		result.Data.IV.Should().Be(request.SecretMessageData.IV);
		result.Data.CT.Should().Be(request.SecretMessageData.CT);
		result.Otp.Should().NotBeNull();
		result.Otp.Required.Should().Be(request.Otp.Required);
		result.Otp.RecipientsEmail.Should().Be(request.Otp.RecipientsEmail.Trim());
		result.EncryptionKeySha256.Should().Be(KeyHashVerifier.Create(request.EncryptionKeySha256));
		result.CreatorClientId.Should().Be(request.ClientId);
		result.CreatorTabId.Should().Be(tabId);
		result.CreatorIP.Should().Be(httpContextClientInfo.ClientIP);
		result.CreatorClientInfo.Should().Be(httpContextClientInfo.ClientInfo);
	}

	[Fact]
	public void ToSecretMessage_ShouldTrimRecipientsEmail()
	{
		// Arrange
		var request = _fixture.Build<StoreNewSecretMessageRequest>()
			.With(x => x.Otp, new OtpSettings
			{
				Required = true,
				RecipientsEmail = " test@example.com "
			})
			.Create();

		var httpContextClientInfo = _fixture.Create<HttpContextClientInfo>();

		// Act
		var result = request.ToSecretMessage(httpContextClientInfo);

		// Assert
		result.Otp.RecipientsEmail.Should().Be("test@example.com");
	}
}