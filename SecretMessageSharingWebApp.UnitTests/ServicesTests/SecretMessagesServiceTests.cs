using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Repositories.Interfaces;
using SecretMessageSharingWebApp.Services;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests
{
	public class SecretMessagesServiceTests
	{
		private readonly SecretMessagesService _sut;
		private readonly Fixture _fixture = new();

		private readonly ISecretMessagesRepository _secretMessagesRepository = Substitute.For<ISecretMessagesRepository>();
		private readonly ILogger<SecretMessagesService> _logger = Substitute.For<ILogger<SecretMessagesService>>();

		public SecretMessagesServiceTests()
		{
			_sut = new SecretMessagesService(_secretMessagesRepository, _logger);
		}

		[Fact]
		public void Store_ShouldReturnInsertedSecretMessage_WhenExecuted()
		{
			// Arrange
			var secretMessage = _fixture.Build<SecretMessage>()
				.Without(x => x.Id)
				.Create();

			// Act
			var result = _sut.Store(secretMessage).Result;

			// Assert
			_secretMessagesRepository.Received().Insert(Arg.Any<SecretMessageDto>());
			_logger.ReceivedWithAnyArgs().LogInformation(default);

			result.Should().BeEquivalentTo(secretMessage, opt => opt.Excluding(r => r.Id));
			result.Id.Should().NotBeNullOrEmpty();
		}

		[Fact]
		public void Retrieve_ShouldReturnFoundSecretMessage_WhenExecutedAndSecretMessageExists()
		{
			// Arrange
			var secretMessageDto = _fixture.Build<SecretMessageDto>()
				.With(x => x.DeleteOnRetrieve, true)
				.With(x => x.JsonData, JsonConvert.SerializeObject(_fixture.Create<SecretMessageData>()))
				.Create();
			var id = secretMessageDto.Id;

			_secretMessagesRepository.Get(id).Returns(secretMessageDto);

			// Act
			var result = _sut.Retrieve(id).Result;

			// Assert
			_secretMessagesRepository.Received().Get(id);
			_secretMessagesRepository.Received().Delete(Arg.Any<SecretMessageDto>());
			_logger.ReceivedWithAnyArgs().LogInformation(default);

			result.Should().BeEquivalentTo(secretMessageDto.ToSecretMessage());
		}

		[Fact]
		public void Retrieve_ShouldReturnNull_WhenExecutedAndSecretMessageDoesNotExist()
		{
			// Arrange
			var id = _fixture.Create<string>();

			_secretMessagesRepository.Get(id).ReturnsNull();

			// Act
			var result = _sut.Retrieve(id).Result;

			// Assert
			_secretMessagesRepository.Received().Get(id);
			_secretMessagesRepository.DidNotReceive().Delete(Arg.Any<SecretMessageDto>());
			_logger.ReceivedWithAnyArgs().LogInformation(default);

			result.Should().BeNull();
		}

		[Fact]
		public void Delete_ShouldDeleteExistingSecretMessageAndReturnTrue_WhenExecutedAndSecretMessageExists()
		{
			// Arrange
			var secretMessageDto = _fixture.Create<SecretMessageDto>();
			var id = secretMessageDto.Id;

			_secretMessagesRepository.Get(id).Returns(secretMessageDto);

			// Act
			var result = _sut.Delete(id).Result;

			// Assert
			_secretMessagesRepository.Received().Get(id);
			_secretMessagesRepository.Received().Delete(secretMessageDto);
			_logger.ReceivedWithAnyArgs().LogInformation(default);

			result.Should().BeTrue();
		}

		[Fact]
		public void Delete_ShouldDoNothingAndReturnFalse_WhenExecutedAndSecretMessageDoesNotExist()
		{
			// Arrange
			var id = _fixture.Create<string>();

			_secretMessagesRepository.Get(id).ReturnsNull();

			// Act
			var result = _sut.Delete(id).Result;

			// Assert
			_secretMessagesRepository.Received().Get(id);
			_secretMessagesRepository.DidNotReceive().Delete(Arg.Any<SecretMessageDto>());
			_logger.DidNotReceiveWithAnyArgs().LogInformation(default);

			result.Should().BeFalse();
		}

		[Fact]
		public void GetRecentlyStoredSecretMessagesInfo_ShouldReturnProperIEnumerable_WhenGivenValidRecentlyStoredSecretMessagesList()
		{
			// Arrange
			var allSecretMessages = _fixture.CreateMany<SecretMessageDto>(10);
			var recentlyStoredSecretMessages = allSecretMessages.TakeLast(5);
			var recentlyStoredSecretMessagesList = recentlyStoredSecretMessages.Select(m => m.Id).ToList();

			_secretMessagesRepository.GetDbSetAsQueryable().Returns(allSecretMessages.AsQueryable());

			// Act
			var result = _sut.GetRecentlyStoredSecretMessagesInfo(recentlyStoredSecretMessagesList);

			// Assert
			result.Should().BeEquivalentTo(recentlyStoredSecretMessages.Select(m => m.ToRecentlyStoredSecretMessage()));
		}
	}
}