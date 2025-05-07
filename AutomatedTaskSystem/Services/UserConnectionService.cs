using AutomatedTaskSystem.Dtos;
using AutomatedTaskSystem.Services.TaskService;
using System.Collections.Concurrent;

namespace AutomatedTaskSystem.Services
{
    public class UserConnectionService
    {
        private readonly ConcurrentDictionary<string, UserStatus> _userStatuses = new();
        private readonly Timer _cleanupTimer;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UserConnectionService(IServiceScopeFactory serviceScopeFactory)
        {
            _cleanupTimer = new Timer(RemoveInactiveUsers, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
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

            return !status.IsConnected && DateTime.UtcNow - status.LastUpdated > TimeSpan.FromSeconds(5);
        }

        private void RemoveInactiveUsers(object? state)
        {
            var now = DateTime.UtcNow;

            var usersToRemove = _userStatuses
                .Where(kvp => !kvp.Value.IsConnected && now - kvp.Value.LastUpdated > TimeSpan.FromSeconds(5))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var userId in usersToRemove)
            {
                // Pause tasks for the disconnected user
                PauseUserTasks(userId);
                _userStatuses.TryRemove(userId, out _);
            }
        }


        private void PauseUserTasks(string userId)
        {
            try
            {
                // Create a scope and resolve ITaskService from the scoped provider
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

                    // Call the task service to pause all tasks for this user
                    taskService.PauseAllTasksForUser(Convert.ToInt32(userId));
                    //UserDisconnected(userId);
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions gracefully
                Console.WriteLine($"Error while pausing tasks for user {userId}: {ex.Message}");
            }
        }
    }
}
