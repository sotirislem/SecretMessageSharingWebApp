//using System.Text.Json;
//using Microsoft.Extensions.Logging;
//using SecretMessageSharingWebApp.Data.Entities;
//using SecretMessageSharingWebApp.Mappings;
//using SecretMessageSharingWebApp.Models.Common;
//using SecretMessageSharingWebApp.Models.Domain;
//using SecretMessageSharingWebApp.Repositories.Interfaces;
//using SecretMessageSharingWebApp.Services;

//namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

//public sealed class SecretMessagesServiceTests
//{
//	private readonly SecretMessagesService _sut;
//	private readonly Fixture _fixture = new();

//	private readonly ISecretMessagesRepository _secretMessagesRepository = Substitute.For<ISecretMessagesRepository>();
//	private readonly ILogger<SecretMessagesService> _logger = Substitute.For<ILogger<SecretMessagesService>>();

//	public SecretMessagesServiceTests()
//	{
//		_sut = new SecretMessagesService(_secretMessagesRepository, _logger);
//	}

//	[Fact]
//	public void Store_ShouldReturnInsertedSecretMessage_WhenExecuted()
//	{
//		// Arrange
//		var secretMessage = _fixture.Build<SecretMessage>()
//			.Without(x => x.Id)
//			.Create();

//		// Act
//		var result = _sut.Store(secretMessage).Result;

//		// Assert
//		_secretMessagesRepository.Received().Insert(Arg.Any<SecretMessageEntity>());
//		_logger.ReceivedWithAnyArgs().LogInformation(default);

//		result.Should().BeEquivalentTo(secretMessage, opt => opt.Excluding(r => r.Id));
//		result.Id.Should().NotBeNullOrEmpty();
//	}

//	[Fact]
//	public void VerifyExistence_ShouldReturnTrueWithOtpSettings_WhenExecutedAndSecretMessageExistsAndOtpIsRequired()
//	{
//		// Arrange
//		var secretMessageDto = _fixture.Build<SecretMessageEntity>()
//			.With(x => x.Otp, new Data.Entities.OtpSettings() { RecipientsEmail = "example@example.com" })
//			.Create();

//		_secretMessagesRepository.GetDbSetAsQueryable().Returns(new List<SecretMessageEntity>()
//		{
//			secretMessageDto
//		}.AsQueryable());

//		// Act
//		var result = _sut.VerifyExistence(secretMessageDto.Id);

//		// Assert
//		_secretMessagesRepository.Received().GetDbSetAsQueryable();
//		_logger.ReceivedWithAnyArgs().LogInformation(default);

//		result.exists.Should().BeTrue();
//		result.otp.Should().NotBeNull();
//		result.otp?.Required.Should().BeTrue();
//		result.otp?.RecipientsEmail.Should().Be("example@example.com");
//	}

//	[Fact]
//	public void VerifyExistence_ShouldReturnTrueWithOtpSettings_WhenExecutedAndSecretMessageExistsAndOtpIsNotRequired()
//	{
//		// Arrange
//		var secretMessageDto = _fixture.Build<SecretMessageEntity>()
//			.Without(x => x.Otp)
//			.Create();

//		_secretMessagesRepository.GetDbSetAsQueryable().Returns(new List<SecretMessageEntity>()
//		{
//			secretMessageDto
//		}.AsQueryable());

//		// Act
//		var result = _sut.VerifyExistence(secretMessageDto.Id);

//		// Assert
//		_secretMessagesRepository.Received().GetDbSetAsQueryable();
//		_logger.ReceivedWithAnyArgs().LogInformation(default);

//		result.exists.Should().BeTrue();
//		result.otp.Should().NotBeNull();
//		result.otp?.Required.Should().BeFalse();
//		result.otp?.RecipientsEmail.Should().BeEmpty();
//	}

//	[Fact]
//	public void VerifyExistence_ShouldReturnFalseWithoutOtpSettings_WhenExecutedAndSecretMessageDoesNotExists()
//	{
//		// Arrange
//		var id = _fixture.Create<string>();

//		_secretMessagesRepository.GetDbSetAsQueryable().Returns(new List<SecretMessageEntity>().AsQueryable());

//		// Act
//		var result = _sut.VerifyExistence(id);

//		// Assert
//		_secretMessagesRepository.Received().GetDbSetAsQueryable();
//		_logger.ReceivedWithAnyArgs().LogInformation(default);

//		result.exists.Should().BeFalse();
//		result.otp.Should().BeNull();
//	}

//	[Fact]
//	public void Retrieve_ShouldReturnFoundSecretMessage_WhenExecutedAndSecretMessageExists()
//	{
//		// Arrange
//		var secretMessageDto = _fixture.Build<SecretMessageEntity>()
//			.With(x => x.DeleteOnRetrieve, true)
//			.With(x => x.JsonData, JsonSerializer.Serialize(_fixture.Create<SecretMessageData>()))
//			.Create();
//		var id = secretMessageDto.Id;

//		_secretMessagesRepository.GetById(id).Returns(secretMessageDto);

//		// Act
//		var result = _sut.Retrieve(id).Result;

//		// Assert
//		_secretMessagesRepository.Received().GetById(id);
//		_secretMessagesRepository.Received().Delete(Arg.Any<SecretMessageEntity>());
//		_logger.ReceivedWithAnyArgs().LogInformation(default);

//		result.Should().BeEquivalentTo(secretMessageDto.ToSecretMessage());
//	}

//	[Fact]
//	public void Retrieve_ShouldReturnNull_WhenExecutedAndSecretMessageDoesNotExist()
//	{
//		// Arrange
//		var id = _fixture.Create<string>();

//		_secretMessagesRepository.GetById(id).ReturnsNull();

//		// Act
//		var result = _sut.Retrieve(id).Result;

//		// Assert
//		_secretMessagesRepository.Received().GetById(id);
//		_secretMessagesRepository.DidNotReceive().Delete(Arg.Any<SecretMessageEntity>());
//		_logger.ReceivedWithAnyArgs().LogInformation(default);

//		result.Should().BeNull();
//	}

//	[Fact]
//	public void Delete_ShouldDeleteExistingSecretMessageAndReturnTrue_WhenExecutedAndSecretMessageExists()
//	{
//		// Arrange
//		var secretMessageDto = _fixture.Create<SecretMessageEntity>();
//		var id = secretMessageDto.Id;

//		_secretMessagesRepository.GetById(id).Returns(secretMessageDto);

//		// Act
//		var result = _sut.Delete(id).Result;

//		// Assert
//		_secretMessagesRepository.Received().GetById(id);
//		_secretMessagesRepository.Received().Delete(secretMessageDto);
//		_logger.ReceivedWithAnyArgs().LogInformation(default);

//		result.Should().BeTrue();
//	}

//	[Fact]
//	public void Delete_ShouldDoNothingAndReturnFalse_WhenExecutedAndSecretMessageDoesNotExist()
//	{
//		// Arrange
//		var id = _fixture.Create<string>();

//		_secretMessagesRepository.GetById(id).ReturnsNull();

//		// Act
//		var result = _sut.Delete(id).Result;

//		// Assert
//		_secretMessagesRepository.Received().GetById(id);
//		_secretMessagesRepository.DidNotReceive().Delete(Arg.Any<SecretMessageEntity>());
//		_logger.DidNotReceiveWithAnyArgs().LogInformation(default);

//		result.Should().BeFalse();
//	}

//	[Fact]
//	public void GetRecentlyStoredSecretMessagesInfo_ShouldReturnProperIEnumerable_WhenGivenValidRecentlyStoredSecretMessagesList()
//	{
//		// Arrange
//		var allSecretMessages = _fixture.CreateMany<SecretMessageEntity>(10);
//		var recentlyStoredSecretMessages = allSecretMessages.TakeLast(5);
//		var recentlyStoredSecretMessagesList = recentlyStoredSecretMessages.Select(m => m.Id).ToList();

//		_secretMessagesRepository.GetDbSetAsQueryable().Returns(allSecretMessages.AsQueryable());

//		// Act
//		var result = _sut.GetRecentlyStoredSecretMessagesInfo(recentlyStoredSecretMessagesList);

//		// Assert
//		result.Should().BeEquivalentTo(recentlyStoredSecretMessages.Select(m => m.ToRecentlyStoredSecretMessage()));
//	}
//}