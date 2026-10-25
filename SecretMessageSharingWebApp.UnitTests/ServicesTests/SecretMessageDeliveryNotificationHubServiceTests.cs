using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SecretMessageSharingWebApp.Hubs;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Services;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public class SecretMessageDeliveryNotificationHubServiceTests
{
	private const string NotificationQueueKey = Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue;

	private readonly ILogger<SecretMessageDeliveryNotificationHubService> _logger;
	private readonly IMemoryCacheService _memoryCacheService;
	private readonly IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> _hubContext;

	private readonly SecretMessageDeliveryNotificationHubService _sut;

	public SecretMessageDeliveryNotificationHubServiceTests()
	{
		_logger = Substitute.For<ILogger<SecretMessageDeliveryNotificationHubService>>();
		_memoryCacheService = Substitute.For<IMemoryCacheService>();
		_hubContext = Substitute.For<IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub>>();

		_sut = new SecretMessageDeliveryNotificationHubService(
			_logger,
			_memoryCacheService,
			_hubContext);
	}

	[Fact]
	public async Task SendNotification_ShouldSendToEveryConnectionOfTheClient_WhenNoTargetTabIsGiven()
	{
		// Arrange
		var clientId = Guid.NewGuid().ToString();
		var secretMessageDeliveryNotification = new SecretMessageDeliveryNotification();

		var hubClient = SetupClientsFor("connection1", "connection2");

		_sut.AddConnection(clientId, "tab1", "connection1");
		_sut.AddConnection(clientId, "tab2", "connection2");

		// Act
		var result = await _sut.SendNotification(clientId, null, secretMessageDeliveryNotification);

		// Assert
		result.Should().BeTrue();
		await hubClient.Received(1).SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);
	}

	[Fact]
	public async Task SendNotification_ShouldSendOnlyToTheTargetTab()
	{
		// Arrange
		var clientId = Guid.NewGuid().ToString();
		var secretMessageDeliveryNotification = new SecretMessageDeliveryNotification();

		var hubClient = SetupClientsFor("connection1");

		_sut.AddConnection(clientId, "tab1", "connection1");
		_sut.AddConnection(clientId, "tab2", "connection2");

		// Act
		var result = await _sut.SendNotification(clientId, "tab1", secretMessageDeliveryNotification);

		// Assert
		result.Should().BeTrue();
		await hubClient.Received(1).SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);
		_hubContext.Clients
			.DidNotReceive()
			.Clients(Arg.Is<IReadOnlyList<string>>(ids => ids.Contains("connection2")));
	}

	[Fact]
	public async Task RemoveConnection_ShouldKeepTheRemainingConnections()
	{
		// Arrange
		var clientId = Guid.NewGuid().ToString();
		var secretMessageDeliveryNotification = new SecretMessageDeliveryNotification();

		var hubClient = SetupClientsFor("connection2");

		_sut.AddConnection(clientId, "tab1", "connection1");
		_sut.AddConnection(clientId, "tab2", "connection2");

		// Act
		_sut.RemoveConnection("connection1");
		var result = await _sut.SendNotification(clientId, null, secretMessageDeliveryNotification);

		// Assert
		result.Should().BeTrue();
		await hubClient.Received(1).SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);
	}

	[Fact]
	public async Task RemoveConnection_ShouldNotAffectTheOtherTab()
	{
		// Arrange
		var clientId = Guid.NewGuid().ToString();
		var secretMessageDeliveryNotification = new SecretMessageDeliveryNotification();

		var hubClient = SetupClientsFor("connection2");

		_sut.AddConnection(clientId, "tab1", "connection1");
		_sut.AddConnection(clientId, "tab2", "connection2");

		// Act
		_sut.RemoveConnection("connection1");
		var result = await _sut.SendNotification(clientId, "tab2", secretMessageDeliveryNotification);

		// Assert
		result.Should().BeTrue();
		await hubClient.Received(1).SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);
	}

	[Fact]
	public async Task SendNotification_ShouldQueueNotificationForTheTargetTab_WhenThatTabIsNotConnected()
	{
		// Arrange
		var clientId = Guid.NewGuid().ToString();
		var secretMessageDeliveryNotification = new SecretMessageDeliveryNotification();

		// Another tab of the same client is connected, but the creator tab is not.
		_sut.AddConnection(clientId, "tab2", "connection2");

		// Act
		var result = await _sut.SendNotification(clientId, "tab1", secretMessageDeliveryNotification);

		// Assert
		result.Should().BeFalse();
		_hubContext.Clients.DidNotReceiveWithAnyArgs().Clients(default!);
		_memoryCacheService.Received(1).SetValue(
			clientId,
			Arg.Is<ConcurrentQueue<PendingSecretMessageDeliveryNotification>>(queue =>
				queue.Count == 1
				&& queue.Single().Notification == secretMessageDeliveryNotification
				&& queue.Single().TargetTabId == "tab1"),
			NotificationQueueKey);
	}

	[Fact]
	public async Task SendNotification_ShouldQueueNotification_WhenClientIsNotActive()
	{
		// Arrange
		var clientId = Guid.NewGuid().ToString();
		var secretMessageDeliveryNotification = new SecretMessageDeliveryNotification();

		// Act
		var result = await _sut.SendNotification(clientId, null, secretMessageDeliveryNotification);

		// Assert
		result.Should().BeFalse();
		_memoryCacheService.Received(1).SetValue(
			clientId,
			Arg.Is<ConcurrentQueue<PendingSecretMessageDeliveryNotification>>(queue =>
				queue.Count == 1
				&& queue.Single().Notification == secretMessageDeliveryNotification
				&& queue.Single().TargetTabId == null),
			NotificationQueueKey);
		_logger.DidNotReceiveWithAnyArgs().LogInformation(default);
	}

	[Fact]
	public async Task SendNotification_ShouldAppendToTheExistingQueue()
	{
		// Arrange
		var clientId = Guid.NewGuid().ToString();
		var secretMessageDeliveryNotification = new SecretMessageDeliveryNotification();
		var queue = new ConcurrentQueue<PendingSecretMessageDeliveryNotification>();

		_memoryCacheService
			.GetValue<ConcurrentQueue<PendingSecretMessageDeliveryNotification>>(
				clientId, NotificationQueueKey, remove: false)
			.Returns((true, queue));

		// Act
		var result = await _sut.SendNotification(clientId, "tab1", secretMessageDeliveryNotification);

		// Assert
		result.Should().BeFalse();
		queue.Should().ContainSingle(pending =>
			pending.Notification == secretMessageDeliveryNotification && pending.TargetTabId == "tab1");
		_memoryCacheService.Received(1).SetValue(clientId, queue, NotificationQueueKey);
	}

	[Fact]
	public async Task SendNotification_ShouldQueueNotification_WhenSendingThrows()
	{
		// Arrange
		var clientId = Guid.NewGuid().ToString();
		var secretMessageDeliveryNotification = new SecretMessageDeliveryNotification();

		var hubClient = Substitute.For<ISecretMessageDeliveryNotificationHub>();
		hubClient
			.SendSecretMessageDeliveryNotification(Arg.Any<SecretMessageDeliveryNotification>())
			.Returns(Task.FromException(new InvalidOperationException("SignalR failure")));

		_hubContext.Clients
			.Clients(Arg.Any<IReadOnlyList<string>>())
			.Returns(hubClient);

		_sut.AddConnection(clientId, "tab1", "connection1");

		// Act
		var result = await _sut.SendNotification(clientId, "tab1", secretMessageDeliveryNotification);

		// Assert
		result.Should().BeFalse();
		_memoryCacheService.Received(1).SetValue(
			clientId,
			Arg.Is<ConcurrentQueue<PendingSecretMessageDeliveryNotification>>(queue =>
				queue.Count == 1
				&& queue.Single().Notification == secretMessageDeliveryNotification
				&& queue.Single().TargetTabId == "tab1"),
			NotificationQueueKey);
	}

	[Fact]
	public async Task SendAnyPendingNotificationFromMemoryCacheQueue_ShouldSendUntargetedNotificationToTheConnectingConnection()
	{
		// Arrange
		var clientId = Guid.NewGuid().ToString();
		var connectionId = "connection1";
		var notification = new SecretMessageDeliveryNotification();
		var queue = CreateQueue((notification, null));

		_memoryCacheService
			.GetValue<ConcurrentQueue<PendingSecretMessageDeliveryNotification>>(
				clientId, NotificationQueueKey, remove: false)
			.Returns((true, queue));

		var hubClient = SetupClientConnection(connectionId);

		// Act
		await _sut.SendAnyPendingNotificationFromMemoryCacheQueue(clientId, "tab1", connectionId);

		// Assert
		await hubClient.Received(1).SendSecretMessageDeliveryNotification(notification);
		queue.Should().BeEmpty();
	}

	[Fact]
	public async Task SendAnyPendingNotificationFromMemoryCacheQueue_ShouldSendNotificationTargetedAtTheConnectingTab()
	{
		// Arrange
		var clientId = Guid.NewGuid().ToString();
		var connectionId = "connection1";
		var notification = new SecretMessageDeliveryNotification();
		var queue = CreateQueue((notification, "tab1"));

		_memoryCacheService
			.GetValue<ConcurrentQueue<PendingSecretMessageDeliveryNotification>>(
				clientId, NotificationQueueKey, remove: false)
			.Returns((true, queue));

		var hubClient = SetupClientConnection(connectionId);

		// Act
		await _sut.SendAnyPendingNotificationFromMemoryCacheQueue(clientId, "tab1", connectionId);

		// Assert
		await hubClient.Received(1).SendSecretMessageDeliveryNotification(notification);
		queue.Should().BeEmpty();
	}

	[Fact]
	public async Task SendAnyPendingNotificationFromMemoryCacheQueue_ShouldNotSendNotificationsOfAnotherTab()
	{
		// Arrange
		var clientId = Guid.NewGuid().ToString();
		var connectionId = "connection1";
		var notification = new SecretMessageDeliveryNotification();
		var queue = CreateQueue((notification, "tab2"));

		_memoryCacheService
			.GetValue<ConcurrentQueue<PendingSecretMessageDeliveryNotification>>(
				clientId, NotificationQueueKey, remove: false)
			.Returns((true, queue));

		var hubClient = SetupClientConnection(connectionId);

		// Act
		await _sut.SendAnyPendingNotificationFromMemoryCacheQueue(clientId, "tab1", connectionId);

		// Assert
		await hubClient.DidNotReceiveWithAnyArgs().SendSecretMessageDeliveryNotification(default!);
		queue.Should().ContainSingle(pending =>
			pending.Notification == notification && pending.TargetTabId == "tab2");
	}

	[Fact]
	public async Task SendAnyPendingNotificationFromMemoryCacheQueue_ShouldKeepNotifications_WhenSendingFails()
	{
		// Arrange
		var clientId = Guid.NewGuid().ToString();
		var connectionId = "connection1";
		var firstNotification = new SecretMessageDeliveryNotification();
		var secondNotification = new SecretMessageDeliveryNotification();
		var queue = CreateQueue((firstNotification, null), (secondNotification, null));

		_memoryCacheService
			.GetValue<ConcurrentQueue<PendingSecretMessageDeliveryNotification>>(
				clientId, NotificationQueueKey, remove: false)
			.Returns((true, queue));

		var hubClient = Substitute.For<ISecretMessageDeliveryNotificationHub>();
		hubClient
			.SendSecretMessageDeliveryNotification(Arg.Any<SecretMessageDeliveryNotification>())
			.Returns(Task.FromException(new InvalidOperationException("SignalR failure")));

		_hubContext.Clients
			.Client(connectionId)
			.Returns(hubClient);

		// Act
		await _sut.SendAnyPendingNotificationFromMemoryCacheQueue(clientId, "tab1", connectionId);

		// Assert
		queue.Should().HaveCount(2);
		queue.Should().Contain(pending => pending.Notification == firstNotification);
		queue.Should().Contain(pending => pending.Notification == secondNotification);
	}

	[Fact]
	public async Task SendAnyPendingNotificationFromMemoryCacheQueue_ShouldDoNothing_WhenThereIsNoQueue()
	{
		// Arrange
		var clientId = Guid.NewGuid().ToString();

		// Act
		await _sut.SendAnyPendingNotificationFromMemoryCacheQueue(clientId, "tab1", "connection1");

		// Assert
		_hubContext.Clients.DidNotReceive().Client(Arg.Any<string>());
	}

	private ISecretMessageDeliveryNotificationHub SetupClientsFor(params string[] connectionIds)
	{
		var hubClient = Substitute.For<ISecretMessageDeliveryNotificationHub>();

		_hubContext.Clients
			.Clients(Arg.Is<IReadOnlyList<string>>(ids =>
				ids.Count == connectionIds.Length
				&& ids.OrderBy(id => id).SequenceEqual(connectionIds.OrderBy(id => id))))
			.Returns(hubClient);

		return hubClient;
	}

	private ISecretMessageDeliveryNotificationHub SetupClientConnection(string connectionId)
	{
		var hubClient = Substitute.For<ISecretMessageDeliveryNotificationHub>();

		_hubContext.Clients
			.Client(connectionId)
			.Returns(hubClient);

		return hubClient;
	}

	private static ConcurrentQueue<PendingSecretMessageDeliveryNotification> CreateQueue(
		params (SecretMessageDeliveryNotification Notification, string? TargetTabId)[] notifications)
	{
		var queue = new ConcurrentQueue<PendingSecretMessageDeliveryNotification>();

		foreach (var (notification, targetTabId) in notifications)
		{
			queue.Enqueue(new PendingSecretMessageDeliveryNotification
			{
				Notification = notification,
				TargetTabId = targetTabId
			});
		}

		return queue;
	}
}
