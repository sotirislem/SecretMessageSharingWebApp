using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Repositories.Interfaces;
using SecretMessageSharingWebApp.Services;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public class SecretMessagesServiceTests
{
	private readonly IFixture _fixture;
	private readonly ILogger<SecretMessagesService> _logger;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly ISecretMessagesRepository _secretMessagesRepository;

	private readonly SecretMessagesService _sut;

	public SecretMessagesServiceTests()
	{
		_fixture = new Fixture();
		_logger = Substitute.For<ILogger<SecretMessagesService>>();
		_httpContextAccessor = Substitute.For<IHttpContextAccessor>();
		_secretMessagesRepository = Substitute.For<ISecretMessagesRepository>();

		_sut = new SecretMessagesService(_logger, _httpContextAccessor, _secretMessagesRepository);
	}

	[Fact]
	public async Task Store_ShouldInsertSecretMessageAndSaveToSession()
	{
		// Arrange
		var secretMessage = _fixture.Create<SecretMessage>();

		var session = Substitute.For<ISession>();
		_httpContextAccessor.HttpContext!.Session = session;

		// Act
		var result = await _sut.Store(secretMessage);

		// Assert
		await _secretMessagesRepository.Received(1).Insert(Arg.Any<SecretMessageEntity>());
		session.Received().Set(Constants.SessionKeys.RecentlyStoredSecretMessagesList, Arg.Any<byte[]>());
		_logger.ReceivedWithAnyArgs().LogInformation(default);
		result.Should().BeEquivalentTo(secretMessage, options => options.Excluding(s => s.Id));
	}

	[Fact]
	public async Task Exists_ShouldReturnCorrectExistenceAndOtpSettings()
	{
		// Arrange
		var secretMessageEntity = _fixture.Build<SecretMessageEntity>()
			.With(e => e.JsonData, JsonSerializer.Serialize(new SecretMessageData()))
			.Create();

		_secretMessagesRepository.GetById(secretMessageEntity.Id).Returns(secretMessageEntity);

		// Act
		var result = await _sut.Exists(secretMessageEntity.Id);

		// Assert
		result.exists.Should().BeTrue();
		result.otpSettings.Should().BeEquivalentTo(secretMessageEntity.Otp.ToDomain());
	}

	[Fact]
	public async Task Retrieve_ShouldDeleteMessageAndLogInformation_WhenMessageExists()
	{
		// Arrange
		var secretMessageEntity = _fixture.Build<SecretMessageEntity>()
			.With(e => e.JsonData, JsonSerializer.Serialize(new SecretMessageData()))
			.Create();

		_secretMessagesRepository.GetById(secretMessageEntity.Id).Returns(secretMessageEntity);
		_secretMessagesRepository.Delete(secretMessageEntity).Returns(1);

		// Act
		var result = await _sut.Retrieve(secretMessageEntity.Id);

		// Assert
		await _secretMessagesRepository.Received(1).Delete(secretMessageEntity);
		_logger.ReceivedWithAnyArgs().LogInformation(default);
		result.Should().BeEquivalentTo(secretMessageEntity.ToDomain());
	}

	[Fact]
	public async Task Retrieve_ShouldNotDeleteMessageAndLogInformation_WhenMessageDoesNotExist()
	{
		// Arrange
		var nonExistentId = _fixture.Create<string>();
		_secretMessagesRepository.GetById(nonExistentId).Returns((SecretMessageEntity?)null);

		// Act
		var result = await _sut.Retrieve(nonExistentId);

		// Assert
		await _secretMessagesRepository.DidNotReceive().Delete(Arg.Any<SecretMessageEntity>());
		_logger.ReceivedWithAnyArgs().LogInformation(default);
		result.Should().BeNull();
	}

	[Fact]
	public async Task Delete_ShouldDeleteMessageAndLogInformation_WhenMessageExists()
	{
		// Arrange
		var secretMessageEntity = _fixture.Create<SecretMessageEntity>();
		_secretMessagesRepository.GetById(secretMessageEntity.Id).Returns(secretMessageEntity);
		_secretMessagesRepository.Delete(secretMessageEntity).Returns(1);

		// Act
		var result = await _sut.Delete(secretMessageEntity.Id);

		// Assert
		await _secretMessagesRepository.Received(1).Delete(secretMessageEntity);
		_logger.ReceivedWithAnyArgs().LogInformation(default);
		result.Should().BeTrue();
	}

	[Fact]
	public async Task Delete_ShouldNotDeleteMessageAndLogInformation_WhenMessageDoesNotExist()
	{
		// Arrange
		var nonExistentId = _fixture.Create<string>();
		_secretMessagesRepository.GetById(nonExistentId).Returns((SecretMessageEntity?)null);

		// Act
		var result = await _sut.Delete(nonExistentId);

		// Assert
		await _secretMessagesRepository.DidNotReceive().Delete(Arg.Any<SecretMessageEntity>());
		_logger.ReceivedWithAnyArgs().LogInformation(default);
		result.Should().BeFalse();
	}

	[Fact]
	public async Task GetRecentlyStoredSecretMessagesInfo_ShouldReturnMappedList()
	{
		// Arrange
		var recentlyStoredSecretMessagesList = _fixture.Create<List<string>>();
		var secretMessageEntities = _fixture.CreateMany<SecretMessageEntity>(2).ToList();

		_secretMessagesRepository.SelectEntitiesWhere(Arg.Any<Expression<Func<SecretMessageEntity, bool>>>())
			.Returns(secretMessageEntities);

		// Act
		var result = await _sut.GetRecentlyStoredSecretMessagesInfo(recentlyStoredSecretMessagesList);

		// Assert
		result.Should().BeEquivalentTo(secretMessageEntities.Select(e => e.ToRecentlyStoredSecretMessage()));
	}
}