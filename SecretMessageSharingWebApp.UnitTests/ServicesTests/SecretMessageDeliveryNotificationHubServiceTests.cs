using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SecretMessageSharingWebApp.Hubs;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Services;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests
{
	public class SecretMessageDeliveryNotificationHubServiceTests
	{
		private readonly SecretMessageDeliveryNotificationHubService _sut;
		private readonly Fixture _fixture = new();

		private readonly ILogger<SecretMessageDeliveryNotificationHubService> _logger 
			= Substitute.For<ILogger<SecretMessageDeliveryNotificationHubService>>();
		private readonly IMemoryCacheService _memoryCacheService 
			= Substitute.For<IMemoryCacheService>();
		private readonly IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> _secretMessageDeliveryNotificationHub 
			= Substitute.For<IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub>>();

		public SecretMessageDeliveryNotificationHubServiceTests()
		{
			_sut = new SecretMessageDeliveryNotificationHubService(_logger, _memoryCacheService, _secretMessageDeliveryNotificationHub);
		}

		[Fact]
		public void Send_ShouldSuccessfullySendADeliveryNotification_WhenCreatorsSignalRConnectionIdExistsInMemoryAndHisConnectionIsAlive()
		{
			// Arrange
			var secretMessageDeliveryNotification = _fixture.Create<SecretMessageDeliveryNotification>();
			var connectionId = _fixture.Create<string>();

			_sut.AddConnection(connectionId);
			_memoryCacheService.GetValue(secretMessageDeliveryNotification.MessageId).Returns((true, connectionId));

			// Act
			var result = _sut.SendNotification(secretMessageDeliveryNotification).Result;

			// Assert
			_secretMessageDeliveryNotificationHub.Clients.Client(connectionId).Received().SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);
			_logger.ReceivedWithAnyArgs().LogInformation(default);

			result.Should().BeTrue();
		}

		[Fact]
		public void Send_ShouldNotSendADeliveryNotification_WhenCreatorsSignalRConnectionIdExistsInMemoryButHisConnectionIsNotAlive()
		{
			// Arrange
			var secretMessageDeliveryNotification = _fixture.Create<SecretMessageDeliveryNotification>();
			var connectionId = _fixture.Create<string>();

			//_sut.AddConnection(connectionId);
			_memoryCacheService.GetValue(secretMessageDeliveryNotification.MessageId).Returns((true, connectionId));

			// Act
			var result = _sut.SendNotification(secretMessageDeliveryNotification).Result;

			// Assert
			_secretMessageDeliveryNotificationHub.Clients.Client(connectionId).DidNotReceive().SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);
			_logger.DidNotReceiveWithAnyArgs().LogInformation(default);

			result.Should().BeFalse();
		}

		[Fact]
		public void Send_ShouldNotSendADeliveryNotification_WhenCreatorsSignalRConnectionDoesNotExistsInMemoryRegardlessTheConnectionStatus()
		{
			// Arrange
			var secretMessageDeliveryNotification = _fixture.Create<SecretMessageDeliveryNotification>();

			_memoryCacheService.GetValue(secretMessageDeliveryNotification.MessageId).Returns((false, null));

			// Act
			var result = _sut.SendNotification(secretMessageDeliveryNotification).Result;

			// Assert
			_secretMessageDeliveryNotificationHub.Clients.Client(Arg.Any<string>()).DidNotReceive().SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);
			_logger.DidNotReceiveWithAnyArgs().LogInformation(default);

			result.Should().BeFalse();
		}
	}
}