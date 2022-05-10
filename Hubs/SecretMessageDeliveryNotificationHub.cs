using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace SecretsManagerWebApp.Hubs
{
    public class SecretMessageDeliveryNotificationHub : Hub
    {
        public static List<string> ActiveConnections = new List<string>();

        public override Task OnConnectedAsync()
        {
            ActiveConnections.Add(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            ActiveConnections.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
