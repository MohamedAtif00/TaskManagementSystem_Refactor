using AutomatedTaskSystem.Dtos;
using AutomatedTaskSystem.Hub;
using AutomatedTaskSystem.Models.Enums;
using AutomatedTaskSystem.Services.TaskService;
using AutomatedTaskSystem.Services.SessionTracking;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace AutomatedTaskSystem.Services
{
    public class UserConnectionService
    {
        private readonly ConcurrentDictionary<string, UserStatus> _userStatuses = new();
        private readonly Timer _cleanupTimer;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private static readonly TimeSpan InactivityTimeout = TimeSpan.FromMinutes(10);

        public UserConnectionService(IServiceScopeFactory serviceScopeFactory)
        {
            _cleanupTimer = new Timer(RemoveInactiveUsers, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void SetUserConnected(string userId)
        {
            _userStatuses[userId] = new UserStatus
            {
                LastUpdated = DateTime.UtcNow,
                IsConnected = true
            };
        }

        public void SetUserDisconnected(string userId)
        {
            if (_userStatuses.ContainsKey(userId))
            {
                _userStatuses[userId].LastUpdated = DateTime.UtcNow;
                _userStatuses[userId].IsConnected = false;
            }
        }

        public void UpdateUserActivity(string userId)
        {
            if (_userStatuses.ContainsKey(userId))
            {
                _userStatuses[userId].LastUpdated = DateTime.UtcNow;
            }
        }

        public bool IsUserDisconnectedFor5Minutes(string userId)
        {
            if (!_userStatuses.TryGetValue(userId, out var status))
                return true;

            return !status.IsConnected && DateTime.UtcNow - status.LastUpdated > InactivityTimeout;
        }

        private void RemoveInactiveUsers(object? state)
        {
            var now = DateTime.UtcNow;

            var usersToRemove =  _userStatuses
                .Where(kvp => !kvp.Value.IsConnected && now - kvp.Value.LastUpdated > InactivityTimeout)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var userId in usersToRemove)
            {
                PauseUserTasks(userId);
                EndSessionForInactivity(userId);
                _userStatuses.TryRemove(userId, out _);
            }

        }

        /// <summary>
        /// Gets the IDs of all users currently marked as connected.
        /// </summary>
        public List<string> GetOnlineUserIds()
        {
            return _userStatuses
                .Where(kvp => kvp.Value.IsConnected)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        private void PauseUserTasks(string userId)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                    taskService.PauseAllTasksForUser(Convert.ToInt32(userId));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while pausing tasks for user {userId}: {ex.Message}");
            }
        }

        private void EndSessionForInactivity(string userId)
        {
            try
            {
                if (!int.TryParse(userId, out int uid))
                    return;

                using var scope = _serviceScopeFactory.CreateScope();
                var sessionService = scope.ServiceProvider.GetRequiredService<IUserSessionService>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<UserHub>>();

                sessionService.InvalidateRefreshTokensForUserAsync(uid).GetAwaiter().GetResult();
                sessionService.EndOpenSessionsForUserAsync(uid, SessionLogoutReason.InactivityTimeout).GetAwaiter().GetResult();

                hubContext.Clients.User(userId).SendAsync("ForceLogout", new
                {
                    reason = nameof(SessionLogoutReason.InactivityTimeout)
                }).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while ending session for inactive user {userId}: {ex.Message}");
            }
        }
    }
}
