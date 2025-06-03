
using System.Security.Claims;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Services;
using Microsoft.AspNetCore.SignalR;

namespace AutomatedTaskSystem.Hub
{
    public class UserHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly UserConnectionService _connectionManager;
        private readonly DataContext _dataContext;

        public UserHub(UserConnectionService connectionManager, DataContext dataContext)
        {
            _connectionManager = connectionManager;
            _dataContext = dataContext;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("id")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _dataContext.Users
                    .FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(userId));

                if (user != null)
                {
                    _connectionManager.SetUserConnected(userId);

                    if (user.Role == Models.Enums.UserRole.UserRoleEnum.Owner)
                    {
                        await Clients.User(userId).SendAsync("OnConnectedMessage", new
                        {
                            pendings = await _dataContext.LeaveRequests.Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending).CountAsync() +
                                await _dataContext.Permissions.Where(x => x.Status == Models.PermissionStatusEnum.Pending).CountAsync()
                        });
                    }
                    else if (user.Role == Models.Enums.UserRole.UserRoleEnum.TeamLeader)
                    {
                        await Clients.User(userId).SendAsync("OnConnectedMessage", new
                        {
                            pendings = await _dataContext.LeaveRequests
                                                         .Include(x => x.Opinions) // Include Opinions for efficient query generation
                                                         .Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending &&
                                                                     x.User.TeamleaderId == user.Id && // For team leader's team
                                                                     !x.Opinions.Any(o => o.UserId == user.Id)) // Crucial: No opinion from this user
                                                         .CountAsync() +
                                       await _dataContext.Permissions
                                                         // Assuming Permissions also has an Opinions collection and you want to apply similar logic
                                                         // If Permissions do not have Opinions, this part remains as just status and teamleader filter.
                                                         .Where(x => x.Status == Models.PermissionStatusEnum.Pending &&
                                                                     x.User.TeamleaderId == user.Id
                                                               && !x.Opinions.Any(o => o.UserId == user.Id)
                                                               )
                                                         .CountAsync()
                        });
                    }
                }
            }

            await base.OnConnectedAsync();
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
        //public override string GetUserId(HubConnectionContext connection)
        //{
        //    // Adjust this to match the claim that holds the user ID
        //    return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //}

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
