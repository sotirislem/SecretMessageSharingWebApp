using Microsoft.AspNetCore.SignalR;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Hubs
{
	public interface ISecretMessageDeliveryNotificationHub
    {
        [HubMethodName("message-delivery-notification")]
        Task SendSecretMessageDeliveryNotification(SecretMessageDeliveryNotification secretMessageDeliveryNotification);
    }

    public class SecretMessageDeliveryNotificationHub : Hub<ISecretMessageDeliveryNotificationHub>
    {
        public const string Url = "signalr/secret-message-delivery-notification-hub";

        private readonly ILogger<SecretMessageDeliveryNotificationHub> _logger;
        private readonly ISecretMessageDeliveryNotificationHubService _secretMessageDeliveryNotificationHubService;

        public SecretMessageDeliveryNotificationHub(ILogger<SecretMessageDeliveryNotificationHub> logger, ISecretMessageDeliveryNotificationHubService secretMessageDeliveryNotificationHubService)
		{
            _logger = logger;
            _secretMessageDeliveryNotificationHubService = secretMessageDeliveryNotificationHubService;
        }

        public override Task OnConnectedAsync()
        {
            _secretMessageDeliveryNotificationHubService.AddConnection(Context.ConnectionId);

            _logger.LogInformation("SecretMessageDeliveryNotificationHub => New connection, ID: {connectionId}", Context.ConnectionId);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _secretMessageDeliveryNotificationHubService.RemoveConnection(Context.ConnectionId);

            _logger.LogInformation("SecretMessageDeliveryNotificationHub => Connection terminated, ID: {connectionId}", Context.ConnectionId);
            if (exception is not null)
			{
                _logger.LogError("SecretMessageDeliveryNotificationHub => Exception: {exceptionMessage}", exception.GetAllErrorMessages());
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
