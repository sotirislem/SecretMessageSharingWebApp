using Microsoft.Extensions.Logging;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Repositories.Interfaces;
using SecretMessageSharingWebApp.Services;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests
{
	public class GetLogsServiceTests
	{
		private readonly GetLogsService _sut;
		private readonly Fixture _fixture = new();

		private readonly IGetLogsRepository _getLogsRepository = Substitute.For<IGetLogsRepository>();
		private readonly ILogger<GetLogsService> _logger = Substitute.For<ILogger<GetLogsService>>();

		public GetLogsServiceTests()
		{
			_sut = new GetLogsService(_getLogsRepository, _logger);
		}

		[Fact]
		public void CreateNewLog_ShouldReturnInsertedGetLog_WhenExecuted()
		{
			// Arrange
			var getLog = _fixture.Create<GetLog>();

			// Act
			var result = _sut.CreateNewLog(getLog).Result;

			// Assert
			_getLogsRepository.Received().Insert(Arg.Any<GetLogDto>());
			_logger.ReceivedWithAnyArgs().LogInformation(default);

			result.Should().BeEquivalentTo(getLog, opt => opt.Excluding(r => r.Id));
			result.Id.Should().NotBeNullOrEmpty();
		}

		[Fact]
		public void GetRecentlyStoredSecretMessagesInfo_ShouldReturnProperIEnumerable_WhenGivenValidRecentlyStoredSecretMessagesList()
		{
			// Arrange
			var allGetLogs = _fixture.Build<GetLogDto>().CreateMany(10);
			var recentlyStoredGetLogs = allGetLogs.TakeLast(5);
			var recentlyStoredSecretMessagesList = recentlyStoredGetLogs.Select(m => m.SecretMessageId).ToList();

			_getLogsRepository.GetDbSetAsQueryable().Returns(allGetLogs.AsQueryable());

			// Act
			var result = _sut.GetRecentlyStoredSecretMessagesInfo(recentlyStoredSecretMessagesList);

			// Assert
			result.Should().BeEquivalentTo(recentlyStoredGetLogs.Select(l => l.ToRecentlyStoredSecretMessage()));
		}
	}
}