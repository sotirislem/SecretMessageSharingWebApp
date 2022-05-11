using Microsoft.AspNetCore.SignalR;

namespace SecretMessageSharingWebApp.Hubs
{
	public class SecretMessageDeliveryNotificationHub : Hub
    {
        public const string HubMethodName = "secret-message-delivery-notification";
        public static List<string> ActiveConnections = new List<string>();

        private ILogger<SecretMessageDeliveryNotificationHub> _logger;

        public SecretMessageDeliveryNotificationHub(ILogger<SecretMessageDeliveryNotificationHub> logger)
		{
            this._logger = logger;
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

            ActiveConnections.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
