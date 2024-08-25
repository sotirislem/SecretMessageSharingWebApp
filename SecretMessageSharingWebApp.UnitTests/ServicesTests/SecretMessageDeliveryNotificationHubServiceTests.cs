using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SecretMessageSharingWebApp.Hubs;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Services;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public class SecretMessageDeliveryNotificationHubServiceTests
{
	private readonly ILogger<SecretMessageDeliveryNotificationHubService> _logger;
	private readonly IMemoryCacheService _memoryCacheService;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub> _hubContext;
	
	private readonly SecretMessageDeliveryNotificationHubService _sut;

	public SecretMessageDeliveryNotificationHubServiceTests()
	{
		_logger = Substitute.For<ILogger<SecretMessageDeliveryNotificationHubService>>();
		_memoryCacheService = Substitute.For<IMemoryCacheService>();
		_httpContextAccessor = Substitute.For<IHttpContextAccessor>();
		_hubContext = Substitute.For<IHubContext<SecretMessageDeliveryNotificationHub, ISecretMessageDeliveryNotificationHub>>();

		_sut = new SecretMessageDeliveryNotificationHubService(
			_logger,
			_memoryCacheService,
			_httpContextAccessor,
			_hubContext);
	}

	[Fact]
	public async Task SendNotification_ShouldSendNotification_WhenClientIsActive()
	{
		// Arrange
		var clientId = "client1";
		var connectionId = "connection1";
		var secretMessageDeliveryNotification = new SecretMessageDeliveryNotification();

		var hubClient = Substitute.For<ISecretMessageDeliveryNotificationHub>();
		_hubContext.Clients.Client(connectionId).Returns(hubClient);
		_sut.AddConnection(clientId, connectionId);

		var httpContext = Substitute.For<HttpContext>();
		httpContext.Request.Headers.Returns(new HeaderDictionary { { "Client-Id", clientId } });
		_httpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _sut.SendNotification(clientId, secretMessageDeliveryNotification);

		// Assert
		result.Should().BeTrue();
		await hubClient.Received().SendSecretMessageDeliveryNotification(secretMessageDeliveryNotification);
		_logger.ReceivedWithAnyArgs().LogInformation(default);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task SendNotification_ShouldQueueNotification_WhenClientIsNotActive(bool isSelfNotification)
	{
		// Arrange
		var clientId = "client1";
		var requestHeadersClientId = isSelfNotification
			? clientId
			: "client2";

		var secretMessageDeliveryNotification = new SecretMessageDeliveryNotification();
		var queue = new Queue<SecretMessageDeliveryNotification>();

		_memoryCacheService.GetValue<Queue<SecretMessageDeliveryNotification>>(clientId, Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue)
			.Returns((true, queue));

		var httpContext = Substitute.For<HttpContext>();
		httpContext.Request.Headers.Returns(new HeaderDictionary { { "Client-Id", requestHeadersClientId } });
		_httpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _sut.SendNotification(clientId, secretMessageDeliveryNotification);

		// Assert
		secretMessageDeliveryNotification.IsSelfNotification.Should().Be(isSelfNotification);
		result.Should().BeFalse();
		queue.Should().Contain(secretMessageDeliveryNotification);
		_memoryCacheService.Received().SetValue(clientId, queue, Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue);
		_logger.DidNotReceiveWithAnyArgs().LogInformation(default);
	}

	[Fact]
	public async Task SendAnyPendingNotificationFromMemoryCacheQueue_ShouldSendPendingNotifications()
	{
		// Arrange
		var clientId = "client1";
		var connectionId = "connection1";
		var queue = new Queue<SecretMessageDeliveryNotification>();
		var notification = new SecretMessageDeliveryNotification();
		queue.Enqueue(notification);

		_memoryCacheService.GetValue<Queue<SecretMessageDeliveryNotification>>(clientId, Constants.MemoryKeys.SecretMessageDeliveryNotificationQueue)
			.Returns((true, queue));

		var hubClient = Substitute.For<ISecretMessageDeliveryNotificationHub>();
		_hubContext.Clients.Client(connectionId).Returns(hubClient);
		_sut.AddConnection(clientId, connectionId);

		// Act
		await _sut.SendAnyPendingNotificationFromMemoryCacheQueue(clientId);

		// Assert
		await hubClient.Received().SendSecretMessageDeliveryNotification(notification);
		_logger.ReceivedWithAnyArgs().LogInformation(default);
		queue.Should().BeEmpty();
	}
}