
using System.Security.Claims;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models.Enums.UserRole;
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
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier.ToString())?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _dataContext.Users
                    .FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(userId));

                if (user != null)
                {
                    _connectionManager.SetUserConnected(userId);

                    if (user.Role == UserRoleEnum.Owner)
                    {
                        await Clients.User(userId).SendAsync("OnConnectedMessage", new
                        {
                            pendings = await _dataContext.LeaveRequests.Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending).CountAsync() +
                                await _dataContext.Permissions.Where(x => x.Status == Models.PermissionStatusEnum.Pending).CountAsync() +
                                await _dataContext.WorkFromHomeRequests.Where(x => x.Status == Models.WorkFromHomeStatusEnum.Pending).CountAsync()
                        });
                    }// ... (your existing code before this section)

                    else if (user.Role == UserRoleEnum.SectionHead)
                    {
                        // Find IDs of users whose group belongs to a section headed by the current projectManager
                        var userIdsManagedByThisProjectManager = await _dataContext.Users
                            .Where(u => u.Group != null &&
                                        u.Group.sectionGroups.Any(sg =>
                                            sg.Section != null &&
                                            sg.Section.HeadId == user.Id // Filter by the connected user's ID
                                        ))
                            .Select(u => u.Id)
                            .ToListAsync();

                        // Count pending Leave requests for these identified users
                        var pendingLeaveRequestsForPM = await _dataContext.LeaveRequests
                            .Include(x => x.Opinions)
                            .Where(lr => lr.Status == Models.LeaveRequestStatusEnum.Pending &&
                                         !lr.Opinions.Any(o => o.UserId == user.Id) &&
                                         userIdsManagedByThisProjectManager.Contains(lr.UserId))
                            .CountAsync();

                        // Count pending Permission requests for these identified users
                        var pendingPermissionRequestsForPM = await _dataContext.Permissions
                            .Include(x => x.Opinions)
                            .Where(p => p.Status == Models.PermissionStatusEnum.Pending &&
                                         !p.Opinions.Any(o => o.UserId == user.Id) &&
                                         userIdsManagedByThisProjectManager.Contains(p.UserId))
                            .CountAsync();

                        // Count pending Work From Home requests for these identified users
                        var pendingWorkFromHomeRequestsForPM = await _dataContext.WorkFromHomeRequests
                            .Include(x => x.Opinions)
                            .Where(wfh => wfh.Status == Models.WorkFromHomeStatusEnum.Pending &&
                                         !wfh.Opinions.Any(o => o.UserId == user.Id) &&
                                         userIdsManagedByThisProjectManager.Contains(wfh.UserId))
                            .CountAsync();

                        var totalPendings = pendingLeaveRequestsForPM + pendingPermissionRequestsForPM + pendingWorkFromHomeRequestsForPM;

                        await Clients.User(userId).SendAsync("OnConnectedMessage", new
                        {
                            pendings = totalPendings
                        });
                    }
                    // ... (rest of your hub code)
                    else if (user.Role == UserRoleEnum.TeamLeader)
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
                                                         .CountAsync() +
                                        await _dataContext.WorkFromHomeRequests
                                        // Assuming Permissions also has an Opinions collection and you want to apply similar logic
                                        // If Permissions do not have Opinions, this part remains as just status and teamleader filter.
                                        .Where(x => x.Status == Models.WorkFromHomeStatusEnum.Pending &&
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
