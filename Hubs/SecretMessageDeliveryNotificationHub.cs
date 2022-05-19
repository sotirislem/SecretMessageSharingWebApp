using Microsoft.AspNetCore.SignalR;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Models.Api;

namespace SecretMessageSharingWebApp.Hubs
{
    public interface ISecretMessageDeliveryNotificationHub
    {
        [HubMethodName("message-delivery-notification")]
        Task SendMessageDeliveryNotification(MessageDeliveryDetails messageDeliveryDetails);
    }

    public class SecretMessageDeliveryNotificationHub : Hub<ISecretMessageDeliveryNotificationHub>
    {
        public const string Url = "signalr/secret-message-delivery-notification-hub";
        public static readonly List<string> ActiveConnections = new List<string>();

        private ILogger<SecretMessageDeliveryNotificationHub> _logger;

        public SecretMessageDeliveryNotificationHub(ILogger<SecretMessageDeliveryNotificationHub> logger)
		{
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("SecretMessageDeliveryNotificationHub => New connection, ID: {connectionId}", Context.ConnectionId);

            ActiveConnections.Add(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("SecretMessageDeliveryNotificationHub => Connection terminated, ID: {connectionId}", Context.ConnectionId);
            if (exception is not null)
			{
                _logger.LogError("SecretMessageDeliveryNotificationHub => Exception: {exceptionMessage}", exception.GetAllErrorMessages());
            }

            ActiveConnections.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
