using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Repositories.Interfaces;
using SecretMessageSharingWebApp.Services;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public class GetLogsServiceTests
{
	private readonly IFixture _fixture;
	private readonly IGetLogsRepository _getLogsRepository;
	private readonly ILogger<GetLogsService> _logger;

	private readonly GetLogsService _sut;

	public GetLogsServiceTests()
	{
		_fixture = new Fixture();
		_getLogsRepository = Substitute.For<IGetLogsRepository>();
		_logger = Substitute.For<ILogger<GetLogsService>>();

		_sut = new GetLogsService(_getLogsRepository, _logger);
	}

	[Fact]
	public async Task CreateNewLog_ShouldInsertLogAndReturnLog()
	{
		// Arrange
		var getLog = _fixture.Create<GetLog>();
		var getLogEntity = getLog.ToEntity();

		// Act
		var result = await _sut.CreateNewLog(getLog);

		// Assert
		await _getLogsRepository.Received(1).Insert(getLogEntity);
		_logger.ReceivedWithAnyArgs().LogInformation(default);

		result.Should().BeEquivalentTo(getLogEntity.ToDomain());
	}

	[Fact]
	public async Task GetRecentlyStoredSecretMessagesInfo_ShouldReturnMessages()
	{
		// Arrange
		var secretMessageIdList = _fixture.Create<List<string>>();
		var getLogEntities = _fixture.CreateMany<GetLogEntity>().ToList();
		var expectedMessages = getLogEntities.Select(entity => entity.ToRecentlyStoredSecretMessage()).ToList();

		_getLogsRepository.SelectEntitiesWhere(Arg.Any<Expression<Func<GetLogEntity, bool>>>())
			.Returns(getLogEntities);

		// Act
		var result = await _sut.GetRecentlyStoredSecretMessagesInfo(secretMessageIdList);

		// Assert
		result.Should().BeEquivalentTo(expectedMessages);
	}
}