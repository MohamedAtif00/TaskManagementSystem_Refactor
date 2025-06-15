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

            // Calculate pending leave requests for the given user (e.g., TL's direct reports)
            var pendingWorkFromHomeRequests = await _dataContext.WorkFromHomeRequests
                .Where(x => x.Status == WorkFromHomeStatusEnum.Pending && x.User.TeamleaderId == userId)
                .CountAsync();

            var totalPendings = pendingPermissionRequests + pendingLeaveRequests + pendingWorkFromHomeRequests;

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
                var totalPendingWorkFromHomeRequests = await _dataContext.WorkFromHomeRequests
                   .Where(x => x.Status == WorkFromHomeStatusEnum.Pending)
                   .CountAsync();

                await _hubContext.Clients.User(owner.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingLeaveRequests + totalPendingPermissionRequests + totalPendingWorkFromHomeRequests,
                    isNewRequest = newPermissionRequestId.HasValue,
                    newPermissionId = newPermissionRequestId // Pass the new permission ID
                });
            }
            else
            {
                Console.WriteLine("Warning: No user with Role.Owner found to send SignalR update for permission.");
            }
        }

        public async System.Threading.Tasks.Task SendProjectManagersPendingUpdate(int? newPermissionRequestId = null)
        {
            Permission? permissionRequest = null; // Initialize to null
            User? requestingUser = null;

            // Consolidate requesting user and permission request fetching
            if (newPermissionRequestId.HasValue)
            {
                permissionRequest = await _dataContext.Permissions
                    .Include(p => p.User) // Include User to get the requesting user
                    .FirstOrDefaultAsync(p => p.Id == newPermissionRequestId.Value);

                requestingUser = permissionRequest?.User;
            }

            List<User> relevantProjectManagers = new List<User>();

            // Determine relevant project managers based on the new request's user's group, if applicable
            if (requestingUser != null && requestingUser.GroupId.HasValue)
            {
                relevantProjectManagers = await _dataContext.SectionGroups
                    .Where(sg => sg.GroupId == requestingUser.GroupId.Value)
                    .Select(sg => sg.Section.Head)
                    .Distinct()
                    .ToListAsync();
            }
            else
            {
                // Fallback: If no specific new request, or user/group not found for the request,
                // include all users designated as ProjectManager based on their PermissionType enum.
                relevantProjectManagers = await _dataContext.SectionGroups
                    .Where(sg => sg.GroupId == requestingUser.GroupId.Value)
                    .Select(sg => sg.Section.Head)
                    .Distinct()
                    .ToListAsync();
            }

            foreach (var projectManager in relevantProjectManagers)
            {
                // Skip if project manager object is null or has no ID
                if (projectManager == null || projectManager.Id == 0) continue;

                // Find IDs of users whose group belongs to a section headed by the current projectManager
                var userIdsManagedByThisProjectManager = await _dataContext.Users
                    .Where(u => u.Group != null &&
                                u.Group.sectionGroups.Any(sg =>
                                    sg.Section != null &&
                                    sg.Section.HeadId == projectManager.Id
                                ))
                    .Select(u => u.Id)
                    .ToListAsync();

                // Count pending Leave requests for these identified users
                var pendingLeaveRequestsForPM = await _dataContext.LeaveRequests
                    .Where(lr => lr.Status == LeaveRequestStatusEnum.Pending &&
                                 !lr.Opinions.Any(o => o.UserId == projectManager.Id) &&
                                 userIdsManagedByThisProjectManager.Contains(lr.UserId)) // Filter by managed users
                    .CountAsync();

                // Count pending Permission requests for these identified users
                var pendingPermissionRequestsForPM = await _dataContext.Permissions
                    .Where(p => p.Status == PermissionStatusEnum.Pending &&
                                 !p.Opinions.Any(o => o.UserId == projectManager.Id) &&
                                 userIdsManagedByThisProjectManager.Contains(p.UserId)) // Filter by managed users
                    .CountAsync();

                // Count pending Work From Home requests for these identified users
                var pendingWorkFromHomeRequestsForPM = await _dataContext.WorkFromHomeRequests
                    .Where(wfh => wfh.Status == WorkFromHomeStatusEnum.Pending &&
                                 !wfh.Opinions.Any(o => o.UserId == projectManager.Id) &&
                                 userIdsManagedByThisProjectManager.Contains(wfh.UserId)) // Filter by managed users
                    .CountAsync();

                var totalPendingsForPM = pendingLeaveRequestsForPM + pendingPermissionRequestsForPM + pendingWorkFromHomeRequestsForPM;

                // Determine if this is a new, relevant request (not cancelled)
                bool isNewRelevantRequest = newPermissionRequestId.HasValue &&
                                            permissionRequest != null &&
                                            permissionRequest.Status != PermissionStatusEnum.Cancelled;

                await _hubContext.Clients.User(projectManager.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingsForPM,
                    isNewRequest = isNewRelevantRequest, // Use the clearer boolean flag
                    newPermissionId = newPermissionRequestId // Pass the new Permission Request ID
                });
            }
        }
        /// <summary>
        /// Sends SignalR update to project managers with their respective pending requests.
        /// </summary>
        /// <param name="newPermissionRequestId">Optional ID of the newly created permission request.</param>
        public async Task SendProjectManagersPendingUpdatechange(int? newPermissionRequestId = null)
        {
            // 1. Find the user associated with the newPermissionRequestId (if provided)
            var request = await _dataContext.Permissions.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == newPermissionRequestId);
            User? requestingUser = request.User;
            if (newPermissionRequestId.HasValue)
            {
                var permissionRequest = await _dataContext.Permissions
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == newPermissionRequestId.Value);

                requestingUser = permissionRequest?.User;
            }

            List<User> relevantProjectManagers = new List<User>();

            if (requestingUser != null && requestingUser.GroupId.HasValue)
            {
                // 2. Find all Sections related to that user's Group
                // 3. Identify the Head of those Sections
                relevantProjectManagers = await _dataContext.SectionGroups
                    .Where(sg => sg.GroupId == requestingUser.GroupId.Value)
                    .Select(sg => sg.Section.Head)
                    .Distinct()
                    .ToListAsync();
            }
            //else
            //{
            //    // If no newPermissionRequestId or requesting user/group,
            //    // you might fall back to the original logic of finding all "ProjectManager" type permissions
            //    // or decide not to send updates in this specific scenario.
            //    // For now, I'll keep the original broad "ProjectManager" retrieval if no specific request is given.
            //    relevantProjectManagers = await _dataContext.Permissions
            //        .Include(x => x.User)
            //        .ThenInclude(x => x.Group)
            //        .ThenInclude(x => x.sectionGroups)
            //        .ThenInclude(x => x.Section)
            //            .ThenInclude(x => x.Head)
            //        .Where(x => x.Type == "ProjectManager") // Assuming 'Type' property in your Permission entity
            //        .Select(x => x.User)
            //        .Distinct()
            //        .ToListAsync();
            //}


            foreach (var projectManager in relevantProjectManagers)
            {
                // Now 'projectManager' is a Section Head (and potentially also a "ProjectManager" type permission holder)
                // Ensure that the projectManager exists and has an Id
                if (projectManager == null || projectManager.Id == 0) continue;

                var pendingLeaveRequestsForPM = await _dataContext.LeaveRequests
                    .Include(x => x.User)
                    .Include(x => x.Opinions)
                    .Where(x => x.Status == LeaveRequestStatusEnum.Pending &&
                                !x.Opinions.Any(o => o.UserId == projectManager.Id) &&
                                x.User.Group.sectionGroups.Any(sg => sg.Section.HeadId == projectManager.Id)) // Add this line
                    .CountAsync();

                var pendingPermissionRequestsForPM = await _dataContext.Permissions
                    .Include(x => x.User)
                    .Include(x => x.Opinions)
                    .Where(x => x.Status == PermissionStatusEnum.Pending &&
                                !x.Opinions.Any(o => o.UserId == projectManager.Id) &&
                                x.User.Group.sectionGroups.Any(sg => sg.Section.HeadId == projectManager.Id)) // Add this line
                    .CountAsync();

                var pendingWorkFromHomeRequestsForPM = await _dataContext.WorkFromHomeRequests
                   .Include(x => x.User)
                   .Include(x => x.Opinions)
                   .Where(x => x.Status == WorkFromHomeStatusEnum.Pending &&
                                !x.Opinions.Any(o => o.UserId == projectManager.Id) &&
                                x.User.Group.sectionGroups.Any(sg => sg.Section.HeadId == projectManager.Id)) // Add this line
                   .CountAsync();

                var totalPendingsForPM = pendingLeaveRequestsForPM + pendingPermissionRequestsForPM + pendingWorkFromHomeRequestsForPM;

                await _hubContext.Clients.User(projectManager.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingsForPM,
                    isNewRequest = newPermissionRequestId.HasValue,
                    newPermissionId = newPermissionRequestId
                });
            }
        }


        public async System.Threading.Tasks.Task SendProjectManagersPendingUpdateWithoutNew(int newPermission) // No optional request ID or newRequest flag
        {

            User? requestingUser = null;
            Permission? permission = null; // Initialize to null

            if (newPermission != null)
            {
                permission = await _dataContext.Permissions
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == newPermission);

                requestingUser = permission?.User;
            }

            List<User> relevantProjectManagers = new List<User>();

            if (requestingUser != null && requestingUser.GroupId.HasValue)
            {
                relevantProjectManagers = await _dataContext.SectionGroups
                    .Where(sg => sg.GroupId == requestingUser.GroupId.Value)
                    .Select(sg => sg.Section.Head)
                    .Distinct()
                    .ToListAsync();
            }

            foreach (var projectManager in relevantProjectManagers)
            {
                if (projectManager == null || projectManager.Id == 0) continue;

                // Find IDs of users whose group belongs to a section headed by the current projectManager
                var userIdsManagedByThisProjectManager = await _dataContext.Users
                    .Where(u => u.Group != null &&
                                u.Group.sectionGroups.Any(sg =>
                                    sg.Section != null &&
                                    sg.Section.HeadId == projectManager.Id
                                ))
                    .Select(u => u.Id)
                    .ToListAsync();

                // Count pending Leave requests for these identified users
                var pendingLeaveRequestsForPM = await _dataContext.LeaveRequests
                    .Where(lr => lr.Status == LeaveRequestStatusEnum.Pending &&
                                 !lr.Opinions.Any(o => o.UserId == projectManager.Id) &&
                                 userIdsManagedByThisProjectManager.Contains(lr.UserId))
                    .CountAsync();

                // Count pending Permission requests for these identified users
                var pendingPermissionRequestsForPM = await _dataContext.Permissions
                    .Where(p => p.Status == PermissionStatusEnum.Pending &&
                                !p.Opinions.Any(o => o.UserId == projectManager.Id) &&
                                userIdsManagedByThisProjectManager.Contains(p.UserId))
                    .CountAsync();

                // Count pending Work From Home requests for these identified users
                var pendingWorkFromHomeRequestsForPM = await _dataContext.WorkFromHomeRequests
                    .Where(wfh => wfh.Status == WorkFromHomeStatusEnum.Pending &&
                                 !wfh.Opinions.Any(o => o.UserId == projectManager.Id) &&
                                 userIdsManagedByThisProjectManager.Contains(wfh.UserId))
                    .CountAsync();


                var totalPendingsForPM = pendingLeaveRequestsForPM + pendingPermissionRequestsForPM + pendingWorkFromHomeRequestsForPM;

                // --- IMPROVED ISNEWREQUEST LOGIC ---
                // A request is considered "new" and relevant if an ID was provided,
                // the request was found, and its status is not Cancelled.
                bool isNewRelevantRequest =
                                            permission != null &&
                                            permission.Status != PermissionStatusEnum.Cancelled;

                await _hubContext.Clients.User(projectManager.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingsForPM,

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

            var pendingWorkFromHomeRequestsForTm = await _dataContext.WorkFromHomeRequests
                .Include(x => x.Opinions)
                .Where(x => x.Status == WorkFromHomeStatusEnum.Pending &&
                            x.User.TeamleaderId == teamLeaderUserId &&
                            !x.Opinions.Any(o => o.UserId == teamLeaderUserId))
                .CountAsync();

            var totalPendingsForTm = pendingLeaveRequestsForTm + pendingPermissionRequestsForTm + pendingWorkFromHomeRequestsForTm;

            await _hubContext.Clients.User(teamLeaderUserId.ToString()).SendAsync("UpdatePendings", new
            {
                pendings = totalPendingsForTm,
                isNewRequest = newLeaveRequestId.HasValue || newPermissionId.HasValue,
                newLeaveRequestId = newLeaveRequestId,
                newPermissionId = newPermissionId
            });
        }

        public async Task SendTeamLeaderPendingUpdatesWithoutNew(int teamLeaderUserId) // No optional request ID
        {
            var pending = await _dataContext.LeaveRequests
                .Include(x => x.Opinions)
                .Where(x => x.Status == LeaveRequestStatusEnum.Pending &&
                            x.User.TeamleaderId == teamLeaderUserId &&
                            !x.Opinions.Any(o => o.UserId == teamLeaderUserId))
                .CountAsync() + await _dataContext.Permissions
                .Include(x => x.Opinions)
                .Where(x => x.Status == PermissionStatusEnum.Pending &&
                            x.User.TeamleaderId == teamLeaderUserId &&
                            !x.Opinions.Any(o => o.UserId == teamLeaderUserId))
                .CountAsync() + await _dataContext.WorkFromHomeRequests
                .Include(x => x.Opinions)
                .Where(x => x.Status == WorkFromHomeStatusEnum.Pending &&
                            x.User.TeamleaderId == teamLeaderUserId &&
                            !x.Opinions.Any(o => o.UserId == teamLeaderUserId))
                .CountAsync();


            await _hubContext.Clients.User(teamLeaderUserId.ToString()).SendAsync("UpdatePendings", new
            {
                pendings = pending,
                isNewRequest = false,             // Explicitly false
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
                                await _dataContext.Permissions.Where(x => x.Status == Models.PermissionStatusEnum.Pending).CountAsync()+
                                await _dataContext.WorkFromHomeRequests.Where(x => x.Status == Models.WorkFromHomeStatusEnum.Pending).CountAsync()
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
                                        !x.Opinions.Any(o => o.UserId == manager.Id)).CountAsync() +
                                   await _dataContext.WorkFromHomeRequests
                                        .Include(x => x.Opinions)
                                        .Where(x => x.Status == Models.WorkFromHomeStatusEnum.Pending &&
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
                var totalWorkFromHomeForTm = await _dataContext.WorkFromHomeRequests
                                                        .Where(x => x.Status == Models.WorkFromHomeStatusEnum.Pending &&
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
                           await _dataContext.Permissions.Include(x => x.Opinions).Where(x => x.Status == Models.PermissionStatusEnum.Pending && !x.Opinions.Any(o => o.UserId == opinionGiver.Id)).CountAsync()+
                           await _dataContext.WorkFromHomeRequests.Include(x => x.Opinions).Where(x => x.Status == Models.WorkFromHomeStatusEnum.Pending && !x.Opinions.Any(o => o.UserId == opinionGiver.Id)).CountAsync()
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
                                                        .CountAsync() +
                               await _dataContext.WorkFromHomeRequests
                                                        .Where(x => x.Status == Models.WorkFromHomeStatusEnum.Pending &&
                                                                    x.User.TeamleaderId == opinionGiver.Id
                                                                && !x.Opinions.Any(o => o.UserId == opinionGiver.Id)
                                                                )
                                                        .CountAsync()
                });


            }
        }
    }
}
