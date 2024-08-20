//using Microsoft.AspNetCore.SignalR;
//using Microsoft.Extensions.Logging;
//using SecretMessageSharingWebApp.Hubs;
//using SecretMessageSharingWebApp.Models.Api.Responses;
//using SecretMessageSharingWebApp.Services;
//using SecretMessageSharingWebApp.Services.Interfaces;

//namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

//public sealed class SecretMessageDeliveryNotificationHubServiceTests
//{
//	private readonly SecretMessageDeliveryNotificationHubService _sut;
//	private readonly Fixture _fixture = new();

//	private readonly ILogger<SecretMessageDeliveryNotificationHubService> _logger
//		= Substitute.For<ILogger<SecretMessageDeliveryNotificationHubService>>();
//	private readonly IMemoryCacheService _memoryCacheService
//		= Substitute.For<IMemoryCacheService>();
//	private readonly IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> _secretMessageDeliveryNotificationHub
//		= Substitute.For<IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub>>();

//	public SecretMessageDeliveryNotificationHubServiceTests()
//	{
//		_sut = new SecretMessageDeliveryNotificationHubService(_logger, _memoryCacheService, _secretMessageDeliveryNotificationHub);
//	}

//	[Fact]
//	public void SendNotification_ShouldSuccessfullySendADeliveryNotification_WhenCreatorsClientIdExistsInMemoryAndHisConnectionIsAlive()
//	{
//		// Arrange
//		var secretMessageDeliveryNotification = _fixture.Create<SecretMessageDeliveryNotification>();
//		var connectionId = _fixture.Create<string>();
//		var clientId = _fixture.Create<string>();

//		_sut.AddConnection(connectionId, clientId);
//		_memoryCacheService.GetValue<string>(secretMessageDeliveryNotification.MessageId, Constants.MemoryKeys.SecretMessageCreatorClientId).Returns((true, clientId));

//		// Act
//		var result = _sut.SendNotification(secretMessageDeliveryNotification).Result;

//		// Assert
//		_secretMessageDeliveryNotificationHub.Clients.Client(connectionId).Received().SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);
//		_logger.ReceivedWithAnyArgs().LogInformation(default);

//		result.Should().BeTrue();
//	}

//	[Fact]
//	public void SendNotification_ShouldNotSendADeliveryNotification_WhenCreatorsClientIdExistsInMemoryButHisConnectionIsNotAlive()
//	{
//		// Arrange
//		var secretMessageDeliveryNotification = _fixture.Create<SecretMessageDeliveryNotification>();
//		var connectionId = _fixture.Create<string>();
//		var clientId = _fixture.Create<string>();

//		_sut.AddConnection(connectionId, clientId);
//		_memoryCacheService.GetValue<string>(secretMessageDeliveryNotification.MessageId, Constants.MemoryKeys.SecretMessageCreatorClientId).Returns((true, clientId));

//		// Act
//		_sut.RemoveConnection(clientId);
//		var result = _sut.SendNotification(secretMessageDeliveryNotification).Result;

//		// Assert
//		_secretMessageDeliveryNotificationHub.Clients.Client(connectionId).DidNotReceive().SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);
//		_logger.DidNotReceiveWithAnyArgs().LogInformation(default);

//		result.Should().BeFalse();
//	}

//	[Fact]
//	public void SendNotification_ShouldNotSendADeliveryNotification_WhenCreatorsClientIdDoesNotExistsInMemoryRegardlessTheConnectionStatus()
//	{
//		// Arrange
//		var secretMessageDeliveryNotification = _fixture.Create<SecretMessageDeliveryNotification>();

//		_memoryCacheService.GetValue<string>(secretMessageDeliveryNotification.MessageId, Constants.MemoryKeys.SecretMessageCreatorClientId).Returns((false, null));

//		// Act
//		var result = _sut.SendNotification(secretMessageDeliveryNotification).Result;

//		// Assert
//		_secretMessageDeliveryNotificationHub.Clients.Client(Arg.Any<string>()).DidNotReceive().SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);
//		_logger.DidNotReceiveWithAnyArgs().LogInformation(default);

//		result.Should().BeFalse();
//	}

//	[Fact]
//	public void SendNotification_ShouldSaveNotificationToMemoryCacheQueueForFutureDelivery_WhenNotificationCannotBeSend()
//	{
//		// Arrange
//		var secretMessageDeliveryNotification = _fixture.Create<SecretMessageDeliveryNotification>();
//		var connectionId = _fixture.Create<string>();
//		var clientId = _fixture.Create<string>();

//		_memoryCacheService.GetValue<string>(secretMessageDeliveryNotification.MessageId, Constants.MemoryKeys.SecretMessageCreatorClientId).Returns((true, clientId));
//		_memoryCacheService.GetValue<Queue<SecretMessageDeliveryNotification>>(clientId, Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue).Returns((false, null));

//		// Act
//		var result = _sut.SendNotification(secretMessageDeliveryNotification).Result;

//		// Assert
//		result.Should().BeFalse();
//		_memoryCacheService.Received().SetValue(clientId, Arg.Is<Queue<SecretMessageDeliveryNotification>>(x => x.Count > 0), Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue);
//	}

//	[Fact]
//	public void SendAnyPendingSecretMessageDeliveryNotificationFromMemoryCacheQueue_ShouldSendEveryPendingNotification_WhenExecuted()
//	{
//		// Arrange
//		var pendingSecretMessageDeliveryNotifications = _fixture.CreateMany<SecretMessageDeliveryNotification>(5);

//		var connectionId = _fixture.Create<string>();
//		var clientId = _fixture.Create<string>();

//		_sut.AddConnection(connectionId, clientId);

//		var inMemomyQueue = new Queue<SecretMessageDeliveryNotification>(pendingSecretMessageDeliveryNotifications);
//		_memoryCacheService.GetValue<Queue<SecretMessageDeliveryNotification>>(clientId, Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue).Returns((true, inMemomyQueue));

//		// Act
//		_ = _sut.SendAnyPendingSecretMessageDeliveryNotificationFromMemoryCacheQueue(clientId);

//		// Assert
//		foreach (var notification in inMemomyQueue)
//		{
//			_secretMessageDeliveryNotificationHub.Clients.Client(connectionId).Received().SendSecretMessageDeliveryNotification(notification);
//		}
//	}
//}