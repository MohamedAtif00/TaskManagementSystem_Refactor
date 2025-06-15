using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Hub;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace AutomatedTaskSystem.Helper
{
    public class WorkFromHomeHelper
    {
        private readonly DataContext _dataContext;
        private readonly IHubContext<UserHub> _hubContext;

        public WorkFromHomeHelper(DataContext dataContext, IHubContext<UserHub> hubContext)
        {
            _dataContext = dataContext;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Sends SignalR update to the specified client with pending Work From Home requests.
        /// This method is intended for the user who *submitted* the request, not necessarily an approver.
        /// </summary>
        public async Task SendPendingUpdatesToClient(int userId, int? newWorkFromHomeRequestId = null)
        {
            var pendingWorkFromHomeRequests = await _dataContext.WorkFromHomeRequests
                .Where(x => x.UserId == userId && x.Status == WorkFromHomeStatusEnum.Pending)
                .CountAsync();

            await _hubContext.Clients.User(userId.ToString()).SendAsync("UpdatePendings", new
            {
                pendings = pendingWorkFromHomeRequests,
                isNewRequest = newWorkFromHomeRequestId.HasValue,
                newWorkFromHomeRequestId = newWorkFromHomeRequestId
            });
        }

        

        /// <summary>
        /// Sends SignalR update to section heads with their respective pending Work From Home requests.
        /// </summary>
        public async Task SendSectionHeadPendingUpdates(int sectionHeadId, int? newWorkFromHomeRequestId = null)
        {
            var sectionHead = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == sectionHeadId && u.Role == UserRoleEnum.SectionHead);
            if (sectionHead != null)
            {
                // Find IDs of users whose group belongs to a section headed by the current sectionHeadId
                var userIdsManagedByThisSectionHead = await _dataContext.Users
                    .Where(u => u.Group != null && // Ensure the user is associated with a group
                                u.Group.sectionGroups.Any(sg => // Check if any SectionGroup for the user's group...
                                    sg.Section != null && // ...links to an existing Section...
                                    sg.Section.HeadId == sectionHeadId // ...where that Section's Head is the current sectionHead
                                ))
                    .Select(u => u.Id) // Select only the user IDs
                    .ToListAsync();

                // Count pending Work From Home requests for these identified users
                var pendingCount = await _dataContext.WorkFromHomeRequests
                    .Where(wfh => wfh.Status == WorkFromHomeStatusEnum.Pending &&
                                  userIdsManagedByThisSectionHead.Contains(wfh.UserId))
                    .CountAsync() +
                    await _dataContext.LeaveRequests
                    .Where(wfh => wfh.Status == LeaveRequestStatusEnum.Pending &&
                                  userIdsManagedByThisSectionHead.Contains(wfh.UserId))
                    .CountAsync() +
                    await _dataContext.Permissions
                    .Where(wfh => wfh.Status == PermissionStatusEnum.Pending &&
                                  userIdsManagedByThisSectionHead.Contains(wfh.UserId))
                    .CountAsync();

                await _hubContext.Clients.User(sectionHead.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = pendingCount,
                    isNewRequest = newWorkFromHomeRequestId.HasValue,
                    newWorkFromHomeRequestId = newWorkFromHomeRequestId
                });
            }
        }

        /// <summary>
        /// Sends SignalR update to the team leader with pending Work From Home requests that they need to act on.
        /// </summary>
        public async Task SendTeamLeaderPendingUpdates(int teamLeaderUserId, int? newWorkFromHomeRequestId = null)
        {
            WorkFromHomeRequest? workFromHomeRequest = null; // Initialize to null

            if (newWorkFromHomeRequestId.HasValue)
            {
                // Fetch the specific WorkFromHomeRequest to check its status
                workFromHomeRequest = await _dataContext.WorkFromHomeRequests
                    .FirstOrDefaultAsync(wfh => wfh.Id == newWorkFromHomeRequestId.Value);
            }

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

            //// Count pending Work From Home requests for these identified users
            var pendingWorkFromHomeRequestsForTm = await _dataContext.WorkFromHomeRequests
                .Include(x => x.Opinions)
                .Where(x => x.Status == WorkFromHomeStatusEnum.Pending &&
                            x.User.TeamleaderId == teamLeaderUserId &&
                            !x.Opinions.Any(o => o.UserId == teamLeaderUserId))
                .CountAsync();

            // ISSUE: You were adding pendingWorkFromHomeRequestsForTm twice.
            // Corrected to: pendingLeaveRequestsForTm + pendingPermissionRequestsForTm + pendingWorkFromHomeRequestsForTm
            var totalPendingsForTm = pendingLeaveRequestsForTm + pendingPermissionRequestsForTm + pendingWorkFromHomeRequestsForTm;

            // --- Apply the same improved isNewRequest logic here ---
            // A request is considered "new" and relevant if an ID was provided,
            // the request was found, and its status is not Cancelled.
            bool isNewRelevantRequest = newWorkFromHomeRequestId.HasValue &&
                                        workFromHomeRequest != null &&
                                        workFromHomeRequest.Status != WorkFromHomeStatusEnum.Cancelled;


            await _hubContext.Clients.User(teamLeaderUserId.ToString()).SendAsync("UpdatePendings", new
            {
                pendings = totalPendingsForTm,
                isNewRequest = isNewRelevantRequest, // Use the boolean flag
                newWorkFromHomeRequestId = newWorkFromHomeRequestId
            });
        }

        public async Task SendTeamLeaderPendingUpdatesWithoutNew(int teamLeaderUserId) // No optional request ID
        {
            // In this overload, there's no specific new request, so newRequest is implicitly false
            // and newWorkFromHomeRequestId is implicitly null.

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

            // For this overload, isNewRequest is always false, and newWorkFromHomeRequestId is always null
            await _hubContext.Clients.User(teamLeaderUserId.ToString()).SendAsync("UpdatePendings", new
            {
                pendings = totalPendingsForTm,
                isNewRequest = false,             // Explicitly false
                newWorkFromHomeRequestId = (int?)null // Explicitly null
            });
        }

        /// <summary>
        /// Sends SignalR update to project managers with their respective pending Work From Home requests.
        /// This retrieves requests for users whose group belongs to a section headed by the project manager.
        /// </summary>
        public async Task SendProjectManagersPendingUpdate(int? newWorkFromHomeRequestId = null,bool newRequest = true)
        {
            User? requestingUser = null;
            WorkFromHomeRequest? workFromHomeRequest = null; // Initialize to null

            if (newWorkFromHomeRequestId.HasValue)
            {
                workFromHomeRequest = await _dataContext.WorkFromHomeRequests
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == newWorkFromHomeRequestId.Value);

                requestingUser = workFromHomeRequest?.User;
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
                bool isNewRelevantRequest = newWorkFromHomeRequestId.HasValue &&
                                            workFromHomeRequest != null &&
                                            workFromHomeRequest.Status != WorkFromHomeStatusEnum.Cancelled;

                await _hubContext.Clients.User(projectManager.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingsForPM,
                    isNewRequest = isNewRelevantRequest, // Use the clearer boolean flag
                    newWorkFromHomeRequestId = newWorkFromHomeRequestId
                });
            }
        }


        public async Task SendProjectManagersPendingUpdateWithoutNew(int newWorkFromHomeRequestId) // No optional request ID or newRequest flag
        {

            User? requestingUser = null;
            WorkFromHomeRequest? workFromHomeRequest = null; // Initialize to null

            if (newWorkFromHomeRequestId != null)
            {
                workFromHomeRequest = await _dataContext.WorkFromHomeRequests
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == newWorkFromHomeRequestId);

                requestingUser = workFromHomeRequest?.User;
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
                                            workFromHomeRequest != null &&
                                            workFromHomeRequest.Status != WorkFromHomeStatusEnum.Cancelled;

                await _hubContext.Clients.User(projectManager.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingsForPM,

                });
            }
        }

        /// <summary>
        /// Sends SignalR update to the owner with total pending requests (including Work From Home).
        /// </summary>
        public async Task SendOwnerPendingUpdate(int? newWorkFromHomeRequestId = null)
        {
            var owner = await _dataContext.Users.FirstOrDefaultAsync(x => x.Role == UserRoleEnum.Owner);
            if (owner != null)
            {
                var totalPendingLeaveRequests = await _dataContext.LeaveRequests.Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending).CountAsync();
                var totalPendingPermissionRequests = await _dataContext.Permissions.Where(x => x.Status == Models.PermissionStatusEnum.Pending).CountAsync();
                var totalPendingWorkFromHomeRequests = await _dataContext.WorkFromHomeRequests.Where(x => x.Status == WorkFromHomeStatusEnum.Pending).CountAsync();

                await _hubContext.Clients.User(owner.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingLeaveRequests + totalPendingPermissionRequests + totalPendingWorkFromHomeRequests,
                    isNewRequest = newWorkFromHomeRequestId.HasValue,
                    newWorkFromHomeRequestId = newWorkFromHomeRequestId
                });
            }
            else
            {
                Console.WriteLine("Warning: No user with Role.Owner found to send SignalR update.");
            }
        }

        /// <summary>
        /// Sends SignalR updates to relevant parties (Owner, Project Managers, Team Leaders, Section Heads)
        /// to refresh their pending request counts after an opinion is given on a Work From Home request.
        /// </summary>
        /// <param name="opinionGiver">The User object of the person who just gave the opinion.</param>
        /// <param name="workFromHomeRequest">The WorkFromHomeRequest object on which the opinion was given.</param>
        public async Task SendPendingUpdatesAfterOpinion(User opinionGiver, WorkFromHomeRequest workFromHomeRequest)
        {
            // Always update the opinion giver's pending count.
            if (opinionGiver.Role == UserRoleEnum.Owner)
            {
                await SendOwnerPendingUpdate();
            }
            else if (opinionGiver.Role == UserRoleEnum.ProjectManger)
            {
                await SendProjectManagersPendingUpdate();
            }
            else if (opinionGiver.Role == UserRoleEnum.TeamLeader)
            {
                await SendTeamLeaderPendingUpdates(opinionGiver.Id);
            }
            else if (opinionGiver.Role == UserRoleEnum.SectionHead)
            {
                // This will now correctly use the relationship through Group -> Section -> Head
                await SendSectionHeadPendingUpdates(opinionGiver.Id);
            }

            // If the WFH request status is now resolved (Approved/Rejected),
            // update all relevant parties' total pending counts as this request is no longer pending.
            if (workFromHomeRequest.Status != WorkFromHomeStatusEnum.Pending)
            {
                await SendOwnerPendingUpdate();
                await SendProjectManagersPendingUpdate(workFromHomeRequest.Id);

                // Trigger updates for Team Leaders and Section Heads whose users were involved in this WFH request.
                // We need to find the specific Team Leader and Section Head associated with the user of this WFH request.
                var requestUser = await _dataContext.Users
                    .Include(u => u.Group)
                        .ThenInclude(g => g.sectionGroups)
                            .ThenInclude(sg => sg.Section)
                    .FirstOrDefaultAsync(u => u.Id == workFromHomeRequest.UserId);

                if (requestUser != null)
                {
                    // Update the Team Leader of the user who made the WFH request
                    if (requestUser.TeamleaderId.HasValue)
                    {
                        await SendTeamLeaderPendingUpdates(requestUser.TeamleaderId.Value);
                    }

                    // Update the Section Head(s) of the section(s) the user's group belongs to
                    var sectionHeadIds = requestUser.Group?.sectionGroups
                        .Where(sg => sg.Section?.HeadId != null)
                        .Select(sg => sg.Section.HeadId)
                        .Distinct()
                        .ToList();

                    if (sectionHeadIds != null)
                    {
                        foreach (var shId in sectionHeadIds)
                        {
                            await SendSectionHeadPendingUpdates(shId);
                        }
                    }
                }
            }
        }

        public async Task<int> GetPendingWorkFromHomeDaysAsync(int userId)
        {
            return await _dataContext.WorkFromHomeRequests
                .Where(lr => lr.UserId == userId &&
                             lr.Status == WorkFromHomeStatusEnum.Pending)
                .CountAsync();
        }

    }
}