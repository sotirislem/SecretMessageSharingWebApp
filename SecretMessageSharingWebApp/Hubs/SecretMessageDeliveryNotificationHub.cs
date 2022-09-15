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
            var clientId = GetClientId();
            if (string.IsNullOrEmpty(clientId))
			{
                Context.Abort();
                return Task.CompletedTask;
            }

            _secretMessageDeliveryNotificationHubService.AddConnection(Context.ConnectionId, clientId);
            _logger.LogInformation("SecretMessageDeliveryNotificationHub => New connection, ID: {connectionId}, Client: {clientId}", Context.ConnectionId, clientId);
            
            _secretMessageDeliveryNotificationHubService.SendAnyPendingSecretMessageDeliveryNotificationFromMemoryCacheQueue(clientId);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var clientId = GetClientId();
            if (!string.IsNullOrEmpty(clientId))
            {
                _secretMessageDeliveryNotificationHubService.RemoveConnection(clientId);
                _logger.LogInformation("SecretMessageDeliveryNotificationHub => Connection terminated, ID: {connectionId}, Client: {clientId}", Context.ConnectionId, clientId);
            }

            if (exception is not null)
			{
                _logger.LogError("SecretMessageDeliveryNotificationHub => Exception: {exceptionMessage}", exception.GetAllErrorMessages());
            }

            return base.OnDisconnectedAsync(exception);
        }

        private string? GetClientId()
		{
            return Context.GetHttpContext()?.Request.Query
                .Where(x => x.Key == "client_id")
                .Select(x => x.Value.ToString())
                .FirstOrDefault();
        }
    }
}
