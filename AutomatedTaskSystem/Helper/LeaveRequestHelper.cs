using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Models;
using Microsoft.AspNetCore.SignalR;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Hub;

namespace AutomatedTaskSystem.Helper
{
    public class LeaveRequestHelper
    {
        private readonly DataContext _dataContext;
        private readonly IHubContext<UserHub> _hubContext;


        public LeaveRequestHelper(DataContext dataContext, IHubContext<UserHub> hubContext)
        {
            _dataContext = dataContext;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Calculates the number of working days between two dates (inclusive).
        /// Assumes Saturday and Sunday are weekends.
        /// </summary>
        public int CalculateWorkingDays(DateTime startDate, DateTime endDate)
        {
            int workingDays = 0;
            for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
            }
            return workingDays;
        }

        /// <summary>
        /// Returns working days between startDate and endDate (inclusive), in order. Uses same weekend as CalculateWorkingDays (Sat/Sun).
        /// </summary>
        public List<DateTime> GetWorkingDaysInRange(DateTime startDate, DateTime endDate)
        {
            var list = new List<DateTime>();
            for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    list.Add(date);
            }
            return list;
        }

        /// <summary>
        /// Splits the date range into two segments by working days. First segment has firstCount working days, second has the rest.
        /// Returns (start1, end1, start2, end2). If firstCount is 0, second segment is the full range; if firstCount >= total, first segment is the full range.
        /// </summary>
        public (DateTime? start1, DateTime? end1, DateTime? start2, DateTime? end2) SplitDateRangeByWorkingDays(DateTime startDate, DateTime endDate, int firstCount)
        {
            var workingDays = GetWorkingDaysInRange(startDate, endDate);
            if (workingDays.Count == 0)
                return (null, null, null, null);
            if (firstCount <= 0)
                return (null, null, workingDays[0], workingDays[workingDays.Count - 1]);
            if (firstCount >= workingDays.Count)
                return (workingDays[0], workingDays[workingDays.Count - 1], null, null);
            return (workingDays[0], workingDays[firstCount - 1], workingDays[firstCount], workingDays[workingDays.Count - 1]);
        }

        /// <summary>
        /// Retrieves pending leave days for a specific user and leave type.
        /// </summary>
        public async Task<int> GetPendingLeaveDaysAsync(int userId, LeaveRequestType type)
        {
            return await _dataContext.LeaveRequests
                .Where(lr => lr.UserId == userId &&
                             lr.Type == type &&
                             lr.Status == LeaveRequestStatusEnum.Pending)
                .Select(lr => EF.Functions.DateDiffDay(lr.StartDate, lr.EndDate) + 1)
                .SumAsync();
        }

        /// <summary>
        /// Returns total pending working days for a user and leave type (same weekend logic as CalculateWorkingDays). Used for FromNextBalance limit checks.
        /// </summary>
        public async Task<int> GetPendingWorkingDaysAsync(int userId, LeaveRequestType type)
        {
            var pending = await _dataContext.LeaveRequests
                .Where(lr => lr.UserId == userId && lr.Type == type && lr.Status == LeaveRequestStatusEnum.Pending)
                .Select(lr => new { lr.StartDate, lr.EndDate })
                .ToListAsync();
            return pending.Sum(lr => CalculateWorkingDays(lr.StartDate, lr.EndDate));
        }

        /// <summary>
        /// Validates the medical certificate file (e.g., checks file extension, size).
        /// This is a placeholder; implement your actual validation logic here.
        /// </summary>
        public bool IsValidMedicalCertificate(IFormFile medicalCertificate)
        {
            if (medicalCertificate == null)
            {
                return false;
            }

            // Example validation:
            // 1. Check file extension
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(medicalCertificate.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return false;
            }

            // 2. Check file size (e.g., max 5MB)
            const long maxFileSize = 5 * 1024 * 1024; // 5 MB
            if (medicalCertificate.Length > maxFileSize)
            {
                return false;
            }

            // Add more robust validation as needed (e.g., magic number check for file type)

            return true;
        }

        /// <summary>
        /// Saves the medical certificate file to a designated directory.
        /// </summary>
        public async Task<string> SaveMedicalCertificate(IFormFile medicalCertificate, int leaveRequestId, IWebHostEnvironment webHostEnvironment)
        {
            if (medicalCertificate == null)
            {
                return null;
            }

            var uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "MedicalCertificates");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + medicalCertificate.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await medicalCertificate.CopyToAsync(fileStream);
            }

            return Path.Combine("MedicalCertificates", uniqueFileName).Replace("\\", "/"); // Return relative path
        }

        /// <summary>
        /// Sends SignalR update to the specified client with pending requests.
        /// </summary>
        public async System.Threading.Tasks.Task SendPendingUpdatesToClient( int userId,int? newLeaveRequestId = null)
        {
            var pendingLeaveRequests = await _dataContext.LeaveRequests
                .Where(x => x.Status == LeaveRequestStatusEnum.Pending && x.User.TeamleaderId == userId)
                .CountAsync();

            var pendingPermissionRequests = await _dataContext.Permissions
                .Where(x => x.Status == PermissionStatusEnum.Pending && x.User.TeamleaderId == userId)
                .CountAsync();

            var pendingWorkFromHomeRequests = await _dataContext.WorkFromHomeRequests
                .Where(x => x.Status == WorkFromHomeStatusEnum.Pending && x.User.TeamleaderId == userId)
                .CountAsync();

            var totalPendings = pendingLeaveRequests + pendingPermissionRequests;

            await _hubContext.Clients.User(userId.ToString()).SendAsync("UpdatePendings", new
            {
                pendings = totalPendings,
                isNewRequest = newLeaveRequestId.HasValue,
                newLeaveRequestId = newLeaveRequestId
            });
        }


        /// <summary>
        /// Sends SignalR update to the teamleader with pending requests.
        /// </summary>
        public async System.Threading.Tasks.Task SendTeamLeaderPendingUpdates(int teamLeaderUserId, int? newLeaveRequestId = null)
        {
            // Initialize leaveRequest to null
            LeaveRequest? leaveRequest = null;

            // If a newLeaveRequestId is provided, fetch the request to check its status
            if (newLeaveRequestId.HasValue)
            {
                leaveRequest = await _dataContext.LeaveRequests
                    .FirstOrDefaultAsync(lr => lr.Id == newLeaveRequestId.Value);
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

            var pendingWorkFromHomeRequestsForTm = await _dataContext.WorkFromHomeRequests
                .Include(x => x.Opinions)
                .Where(x => x.Status == WorkFromHomeStatusEnum.Pending &&
                            x.User.TeamleaderId == teamLeaderUserId &&
                            !x.Opinions.Any(o => o.UserId == teamLeaderUserId))
                .CountAsync();

            // Sum all pending requests (Leave, Permission, Work From Home)
            var totalPendingsForTm = pendingLeaveRequestsForTm + pendingPermissionRequestsForTm + pendingWorkFromHomeRequestsForTm;

            // Determine if this is a new, relevant request (not cancelled)
            bool isNewRelevantRequest = newLeaveRequestId.HasValue &&
                                        leaveRequest != null &&
                                        leaveRequest.Status != LeaveRequestStatusEnum.Cancelled;

            await _hubContext.Clients.User(teamLeaderUserId.ToString()).SendAsync("UpdatePendings", new
            {
                pendings = totalPendingsForTm,
                isNewRequest = isNewRelevantRequest, // Use the new boolean flag
                newLeaveRequestId = newLeaveRequestId
            });
        }

        public async System.Threading.Tasks.Task SendTeamLeaderPendingUpdatesWithoutNew(int teamLeaderUserId) // No optional request ID
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
        /// Sends SignalR update to the owner with total pending requests.
        /// </summary>
        public async System.Threading.Tasks.Task SendOwnerPendingUpdate( int? newLeaveRequestId = null)
        {
            var owner = await _dataContext.Users.FirstOrDefaultAsync(x => x.Role == UserRoleEnum.Owner);
            if (owner != null)
            {
                var totalPendingLeaveRequests = await _dataContext.LeaveRequests.Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending).CountAsync();
                var totalPendingPermissionRequests = await _dataContext.Permissions.Where(x => x.Status == Models.PermissionStatusEnum.Pending).CountAsync();
                var totalPendingWorkFromhomeRequests = await _dataContext.WorkFromHomeRequests.Where(x => x.Status == Models.WorkFromHomeStatusEnum.Pending).CountAsync();

                await _hubContext.Clients.User(owner.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingLeaveRequests + totalPendingPermissionRequests + totalPendingWorkFromhomeRequests,
                    isNewRequest = newLeaveRequestId.HasValue,
                    newLeaveRequestId = newLeaveRequestId
                });
            }
            else
            {
                Console.WriteLine("Warning: No user with Role.Owner found to send SignalR update.");
            }
        }

        public async System.Threading.Tasks.Task SendOwnerPendingUpdatechange(int? newLeaveRequestId = null)
        {
            var owner = await _dataContext.Users.FirstOrDefaultAsync(x => x.Role == UserRoleEnum.Owner);
            if (owner != null)
            {
                var totalPendingLeaveRequests = await _dataContext.LeaveRequests.Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending).CountAsync();
                var totalPendingPermissionRequests = await _dataContext.Permissions.Where(x => x.Status == Models.PermissionStatusEnum.Pending).CountAsync();
                var totalPendingWorkFromHomeRequests = await _dataContext.WorkFromHomeRequests.Where(x => x.Status == Models.WorkFromHomeStatusEnum.Pending).CountAsync();

                await _hubContext.Clients.User(owner.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingLeaveRequests + totalPendingPermissionRequests + totalPendingWorkFromHomeRequests,
                });
            }
            else
            {
                Console.WriteLine("Warning: No user with Role.Owner found to send SignalR update.");
            }
        }



        /// <summary>
        /// Sends SignalR update to project managers with their respective pending requests.
        /// </summary>
        public async System.Threading.Tasks.Task SendProjectManagersPendingUpdate(int? newLeaveRequestId = null)
        {
            // Initialize leaveRequest to null and requestingUser
            LeaveRequest? leaveRequest = null;
            User? requestingUser = null;

            // If a newLeaveRequestId is provided, fetch the request to check its status and retrieve the requesting user
            if (newLeaveRequestId.HasValue)
            {
                leaveRequest = await _dataContext.LeaveRequests
                    .Include(lr => lr.User) // Include User to get the requesting user
                    .FirstOrDefaultAsync(lr => lr.Id == newLeaveRequestId.Value);

                requestingUser = leaveRequest?.User;
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
            //else
            //{
            //    // Fallback: If no specific new request, or user/group not found for the request,
            //    // include all users designated as ProjectManager based on their PermissionType enum.
            //    relevantProjectManagers = await _dataContext.SectionGroups
            //         .Where(sg => sg.GroupId == requestingUser.GroupId.Value)
            //         .Select(sg => sg.Section.Head)
            //         .Distinct()
            //         .ToListAsync();
            //}

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
                bool isNewRelevantRequest = newLeaveRequestId.HasValue &&
                                            leaveRequest != null &&
                                            leaveRequest.Status != LeaveRequestStatusEnum.Cancelled;

                await _hubContext.Clients.User(projectManager.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingsForPM,
                    isNewRequest = isNewRelevantRequest, // Use the clearer boolean flag
                    newLeaveRequestId = newLeaveRequestId // Pass the new Leave Request ID
                });
            }
        }

        public async System.Threading.Tasks.Task SendProjectManagersPendingUpdateWithoutNew(int newLeavRequest) // No optional request ID or newRequest flag
        {

            User? requestingUser = null;
            LeaveRequest? leaveRequest = null; // Initialize to null

            if (newLeavRequest != null)
            {
                leaveRequest = await _dataContext.LeaveRequests
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == newLeavRequest);

                requestingUser = leaveRequest?.User;
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
                                            leaveRequest != null &&
                                            leaveRequest.Status != LeaveRequestStatusEnum.Cancelled;

                await _hubContext.Clients.User(projectManager.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingsForPM,

                });
            }
        }
        /// <summary>
        /// Sends SignalR updates to relevant parties (Owner, Project Managers, Team Leaders)
        /// to refresh their pending request counts after an opinion is given on a leave request.
        /// </summary>
        /// <param name="opinionGiver">The User object of the person who just gave the opinion.</param>
        /// <param name="leaveRequestId">The ID of the leave request on which the opinion was given.</param>
        /// <param name="leaveRequestStatusChanged">True if the leave request's status changed (e.g., from Pending to Approved/Rejected).</param>
        public async System.Threading.Tasks.Task SendPendingUpdatesAfterOpinion(Models.User opinionGiver, LeaveRequest leaveRequest)
        {
            // Owner role sees all pending requests (already correct)
            if (opinionGiver.Role == UserRoleEnum.Owner)
            {
                await _hubContext.Clients.User(opinionGiver.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = await _dataContext.LeaveRequests.Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending).CountAsync() +
                               await _dataContext.Permissions.Where(x => x.Status == Models.PermissionStatusEnum.Pending).CountAsync() +
                               await _dataContext.WorkFromHomeRequests.Where(x => x.Status == Models.WorkFromHomeStatusEnum.Pending).CountAsync()
                });
            }
            // Project Manager role sees pending requests from users in sections they head
            else if (opinionGiver.Role == UserRoleEnum.ProjectManger)
            {
                // Find IDs of users whose group belongs to a section headed by the opinionGiver (Project Manager)
                var userIdsManagedByThisProjectManager = await _dataContext.Users
                    .Where(u => u.Group != null &&
                                u.Group.sectionGroups.Any(sg =>
                                    sg.Section != null &&
                                    sg.Section.HeadId == opinionGiver.Id
                                ))
                    .Select(u => u.Id)
                    .ToListAsync();

                // Count pending Leave requests for these identified users
                var pendingLeaveRequestsForPM = await _dataContext.LeaveRequests
                    .Include(x => x.Opinions)
                    .Where(lr => lr.Status == Models.LeaveRequestStatusEnum.Pending &&
                                 !lr.Opinions.Any(o => o.UserId == opinionGiver.Id) &&
                                 userIdsManagedByThisProjectManager.Contains(lr.UserId)) // Filter by managed users
                    .CountAsync();

                // Count pending Permission requests for these identified users
                var pendingPermissionRequestsForPM = await _dataContext.Permissions
                    .Include(x => x.Opinions)
                    .Where(p => p.Status == Models.PermissionStatusEnum.Pending &&
                                 !p.Opinions.Any(o => o.UserId == opinionGiver.Id) &&
                                 userIdsManagedByThisProjectManager.Contains(p.UserId)) // Filter by managed users
                    .CountAsync();

                // Count pending Work From Home requests for these identified users
                var pendingWorkFromHomeRequestsForPM = await _dataContext.WorkFromHomeRequests
                    .Include(x => x.Opinions)
                    .Where(wfh => wfh.Status == Models.WorkFromHomeStatusEnum.Pending &&
                                 !wfh.Opinions.Any(o => o.UserId == opinionGiver.Id) &&
                                 userIdsManagedByThisProjectManager.Contains(wfh.UserId)) // Filter by managed users
                    .CountAsync();

                var totalPendingsForPM = pendingLeaveRequestsForPM + pendingPermissionRequestsForPM + pendingWorkFromHomeRequestsForPM;

                await _hubContext.Clients.User(opinionGiver.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingsForPM
                });
            }
            // Team Leader (or other roles whose requests are managed by a Team Leader)
            else
            {
                // Ensure that leaveRequest.User is loaded and has a TeamleaderId.
                // It's crucial for `leaveRequest.User.TeamleaderId` to be accurate for this block.
                // If leaveRequest.User might not be loaded, consider loading it here:
                // await _dataContext.Entry(leaveRequest).Reference(lr => lr.User).LoadAsync();

                if (leaveRequest.User == null || !leaveRequest.User.TeamleaderId.HasValue)
                {
                    // Log an error or handle the case where the user or team leader ID is missing
                    Console.WriteLine($"Cannot send update: Requesting user or TeamleaderId is null for leave request {leaveRequest.Id}.");
                    return; // Exit if no team leader to notify
                }

                // The user who gave the opinion is NOT the TeamLeader whose pending requests we are counting.
                // The opinionGiver is likely a Member or another role. The update needs to go to the TeamLeader.
                int teamLeaderToNotifyId = leaveRequest.User.TeamleaderId.Value;

                var pendingLeaveRequestsForTL = await _dataContext.LeaveRequests
                    .Include(x => x.Opinions)
                    .Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending &&
                                x.User.TeamleaderId == teamLeaderToNotifyId && // Requests from this Team Leader's direct reports
                                !x.Opinions.Any(o => o.UserId == teamLeaderToNotifyId)) // Exclude if TL has already given an opinion
                    .CountAsync();

                var pendingPermissionRequestsForTL = await _dataContext.Permissions
                    .Include(x => x.Opinions)
                    .Where(x => x.Status == Models.PermissionStatusEnum.Pending &&
                                x.User.TeamleaderId == teamLeaderToNotifyId && // Requests from this Team Leader's direct reports
                                !x.Opinions.Any(o => o.UserId == teamLeaderToNotifyId)) // Exclude if TL has already given an opinion
                    .CountAsync();

                var pendingWorkFromHomeRequestsForTL = await _dataContext.WorkFromHomeRequests
                    .Include(x => x.Opinions)
                    .Where(x => x.Status == Models.WorkFromHomeStatusEnum.Pending &&
                                x.User.TeamleaderId == teamLeaderToNotifyId && // Requests from this Team Leader's direct reports
                                !x.Opinions.Any(o => o.UserId == teamLeaderToNotifyId)) // Exclude if TL has already given an opinion
                    .CountAsync();

                var totalPendingsForTL = pendingLeaveRequestsForTL + pendingPermissionRequestsForTL + pendingWorkFromHomeRequestsForTL;

                await _hubContext.Clients.User(teamLeaderToNotifyId.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingsForTL
                });
            }
        }


    }
}
