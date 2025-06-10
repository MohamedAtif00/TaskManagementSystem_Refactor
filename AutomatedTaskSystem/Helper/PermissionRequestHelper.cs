using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Hub;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Models;
using Microsoft.AspNetCore.SignalR;

using Task = System.Threading.Tasks.Task;

namespace AutomatedTaskSystem.Helper
{
    public class PermissionRequestHelper
    {
        private readonly DataContext _dataContext;
        private readonly IHubContext<UserHub> _hubContext;

        public PermissionRequestHelper(DataContext dataContext, IHubContext<UserHub> hubContext)
        {
            _dataContext = dataContext;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Sends SignalR update to the specified client (e.g., Team Leader) with their pending requests.
        /// </summary>
        /// <param name="userId">The ID of the user (e.g., Team Leader) to notify.</param>
        /// <param name="newPermissionRequestId">Optional ID of the newly created permission request.</param>
        public async Task SendPendingUpdatesToClient(int userId, int? newPermissionRequestId = null)
        {
            // Calculate pending permission requests for the given user (e.g., TL's direct reports)
            var pendingPermissionRequests = await _dataContext.Permissions
                .Where(x => x.Status == PermissionStatusEnum.Pending && x.User.TeamleaderId == userId)
                .CountAsync();

            // Calculate pending leave requests for the given user (e.g., TL's direct reports)
            var pendingLeaveRequests = await _dataContext.LeaveRequests
                .Where(x => x.Status == LeaveRequestStatusEnum.Pending && x.User.TeamleaderId == userId)
                .CountAsync();

            var totalPendings = pendingPermissionRequests + pendingLeaveRequests;

            await _hubContext.Clients.User(userId.ToString()).SendAsync("UpdatePendings", new
            {
                pendings = totalPendings,
                isNewRequest = newPermissionRequestId.HasValue,
                newPermissionId = newPermissionRequestId // Pass the new permission ID
            });
        }

        /// <summary>
        /// Sends SignalR update to the owner with total pending leave and permission requests.
        /// </summary>
        /// <param name="newPermissionRequestId">Optional ID of the newly created permission request.</param>
        public async Task SendOwnerPendingUpdate(int? newPermissionRequestId = null)
        {
            var owner = await _dataContext.Users.FirstOrDefaultAsync(x => x.Role == UserRoleEnum.Owner);
            if (owner != null)
            {
                var totalPendingLeaveRequests = await _dataContext.LeaveRequests
                    .Where(x => x.Status == LeaveRequestStatusEnum.Pending)
                    .CountAsync();
                var totalPendingPermissionRequests = await _dataContext.Permissions
                    .Where(x => x.Status == PermissionStatusEnum.Pending)
                    .CountAsync();

                await _hubContext.Clients.User(owner.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingLeaveRequests + totalPendingPermissionRequests,
                    isNewRequest = newPermissionRequestId.HasValue,
                    newPermissionId = newPermissionRequestId // Pass the new permission ID
                });
            }
            else
            {
                Console.WriteLine("Warning: No user with Role.Owner found to send SignalR update for permission.");
            }
        }

        public async System.Threading.Tasks.Task SendProjectManagersPendingUpdate(int? newLeaveRequestId = null)
        {
            var projectManagers = await _dataContext.Users.Where(x => x.Role == UserRoleEnum.ProjectManger).ToListAsync();
            foreach (var projectManager in projectManagers)
            {
                var pendingLeaveRequestsForPM = await _dataContext.LeaveRequests
                    .Include(x => x.Opinions)
                    .Where(x => x.Status == LeaveRequestStatusEnum.Pending && !x.Opinions.Any(o => o.UserId == projectManager.Id))
                    .CountAsync();

                var pendingPermissionRequestsForPM = await _dataContext.Permissions
                    .Include(x => x.Opinions)
                    .Where(x => x.Status == PermissionStatusEnum.Pending && !x.Opinions.Any(o => o.UserId == projectManager.Id))
                    .CountAsync();

                var totalPendingsForPM = pendingLeaveRequestsForPM + pendingPermissionRequestsForPM;

                await _hubContext.Clients.User(projectManager.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingsForPM,
                    isNewRequest = newLeaveRequestId.HasValue,
                    newLeaveRequestId = newLeaveRequestId
                });
            }
        }

        /// <summary>
        /// Sends SignalR update to project managers with their respective pending requests.
        /// </summary>
        /// <param name="newPermissionRequestId">Optional ID of the newly created permission request.</param>
        public async Task SendProjectManagersPendingUpdatechange(int? newPermissionRequestId = null)
        {
            var projectManagers = await _dataContext.Users.Where(x => x.Role == UserRoleEnum.ProjectManger).ToListAsync();
            foreach (var projectManager in projectManagers)
            {
                var pendingLeaveRequestsForPM = await _dataContext.LeaveRequests
                    .Include(x => x.User) // Include user to filter by project manager
                    .Include(x => x.Opinions)
                    .Where(x => x.Status == LeaveRequestStatusEnum.Pending &&
                                 !x.Opinions.Any(o => o.UserId == projectManager.Id)) // Filter for PM's own opinions
                    .CountAsync();

                var pendingPermissionRequestsForPM = await _dataContext.Permissions
                    .Include(x => x.User) // Include user to filter by project manager
                    .Include(x => x.Opinions)
                    .Where(x => x.Status == PermissionStatusEnum.Pending &&
                                 !x.Opinions.Any(o => o.UserId == projectManager.Id)) // Filter for PM's own opinions
                    .CountAsync();

                var totalPendingsForPM = pendingLeaveRequestsForPM + pendingPermissionRequestsForPM;

                await _hubContext.Clients.User(projectManager.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingsForPM,
                    isNewRequest = newPermissionRequestId.HasValue,
                    newPermissionId = newPermissionRequestId // Pass the new permission ID
                });
            }
        }

        /// <summary>
        /// Sends SignalR update to the teamleader with pending requests.
        /// </summary>
        public async System.Threading.Tasks.Task SendTeamLeaderPendingUpdates(
            int teamLeaderUserId,
            int? newLeaveRequestId = null,
            int? newPermissionId = null)
        {
            var pendingLeaveRequestsForTm = await _dataContext.LeaveRequests
                .Include(x => x.Opinions)
                .Where(x => x.Status == LeaveRequestStatusEnum.Pending &&
                            x.User.TeamleaderId == teamLeaderUserId &&
                            !x.Opinions.Any(o => o.UserId == teamLeaderUserId))
                .CountAsync();

            var pendingPermissionRequestsForTm = await _dataContext.Permissions
                .Include(x => x.Opinions)
                .Where(x => x.Status == PermissionStatusEnum.Pending &&
                            x.User.TeamleaderId == teamLeaderUserId &&
                            !x.Opinions.Any(o => o.UserId == teamLeaderUserId))
                .CountAsync();

            var totalPendingsForTm = pendingLeaveRequestsForTm + pendingPermissionRequestsForTm;

            await _hubContext.Clients.User(teamLeaderUserId.ToString()).SendAsync("UpdatePendings", new
            {
                pendings = totalPendingsForTm,
                isNewRequest = newLeaveRequestId.HasValue || newPermissionId.HasValue,
                newLeaveRequestId = newLeaveRequestId,
                newPermissionId = newPermissionId
            });
        }

        /// <summary>
        /// Sends SignalR updates to relevant parties (Owner, Project Managers, Team Leaders)
        /// to refresh their pending request counts after an opinion is given on a leave request.
        /// </summary>
        /// <param name="opinionGiver">The User object of the person who just gave the opinion.</param>
        public async Task SendPendingUpdatesAfterOpinion(User opinionGiver, Permission permissioon)
        {
            if (opinionGiver.Role == UserRoleEnum.Owner)
            {
                var managers = await _dataContext.Users.Where(x => x.Role == UserRoleEnum.ProjectManger).ToListAsync();
                await _hubContext.Clients.User(opinionGiver.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = await _dataContext.LeaveRequests.Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending).CountAsync() +
                                await _dataContext.Permissions.Where(x => x.Status == Models.PermissionStatusEnum.Pending).CountAsync()
                });

                foreach (var manager in managers)
                {
                    await _hubContext.Clients.User(manager.Id.ToString()).SendAsync("UpdatePendings", new
                    {
                        pendings = await _dataContext.LeaveRequests
                                        .Include(x => x.Opinions)
                                        .Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending &&
                                        !x.Opinions.Any(o => o.UserId == manager.Id)).CountAsync() +
                               await _dataContext.Permissions
                                        .Include(x => x.Opinions)
                                        .Where(x => x.Status == Models.PermissionStatusEnum.Pending && 
                                        !x.Opinions.Any(o => o.UserId == manager.Id)).CountAsync()
                    });
                    
                }

                var totalleavesForTm = await _dataContext.LeaveRequests
                                                        .Include(x => x.Opinions)
                                                        .Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending &&
                                                                    x.User == permissioon.User &&
                                                                    !x.Opinions.Any(o => o.UserId == permissioon.User.TeamleaderId))
                                                        .CountAsync();
                var totalPermissionsForTm = await _dataContext.Permissions
                                                        .Where(x => x.Status == Models.PermissionStatusEnum.Pending &&
                                                                   x.User.TeamleaderId == permissioon.User.TeamleaderId
                                                                && !x.Opinions.Any(o => o.UserId == permissioon.User.TeamleaderId)
                                                                )
                                                        .CountAsync();


                await _hubContext.Clients.User(permissioon.User.TeamleaderId.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalleavesForTm + totalPermissionsForTm
                });

            }
            else if (opinionGiver.Role == UserRoleEnum.ProjectManger)
            {
                await _hubContext.Clients.User(opinionGiver.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = await _dataContext.LeaveRequests.Include(x => x.Opinions).Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending && !x.Opinions.Any(o => o.UserId == opinionGiver.Id)).CountAsync() +
                           await _dataContext.Permissions.Include(x => x.Opinions).Where(x => x.Status == Models.PermissionStatusEnum.Pending && !x.Opinions.Any(o => o.UserId == opinionGiver.Id)).CountAsync()
                });
            }
            else
            {

                await _hubContext.Clients.User(permissioon.User.TeamleaderId.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = await _dataContext.LeaveRequests
                                                        .Include(x => x.Opinions)
                                                        .Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending &&
                                                                    x.User.TeamleaderId == permissioon.User.TeamleaderId &&
                                                                    !x.Opinions.Any(o => o.UserId == permissioon.User.TeamleaderId))
                                                        .CountAsync() +
                                        await _dataContext.Permissions
                                                        .Where(x => x.Status == Models.PermissionStatusEnum.Pending &&
                                                                    x.User.TeamleaderId == opinionGiver.Id
                                                                && !x.Opinions.Any(o => o.UserId == opinionGiver.Id)
                                                                )
                                                        .CountAsync()
                });


            }
        }
    }
}
