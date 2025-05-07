
using AutomatedTaskSystem.Services;
using Microsoft.AspNetCore.SignalR;

namespace AutomatedTaskSystem.Hub
{
    public class UserHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly UserConnectionService _connectionManager;

        public UserHub(UserConnectionService connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _connectionManager.SetUserConnected(userId);
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _connectionManager.SetUserDisconnected(userId);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task CheckUserDisconnection(string userId)
        {
            var isDisconnected = _connectionManager.IsUserDisconnectedFor5Minutes(userId);
            await Clients.User(userId).SendAsync("UserDisconnected", isDisconnected);
        }

        public Task UpdateUserActivity(string userId)
        {
            _connectionManager.UpdateUserActivity(userId);
            return Task.CompletedTask;
        }
    }
}
