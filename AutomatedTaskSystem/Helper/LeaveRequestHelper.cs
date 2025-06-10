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
        /// Sends SignalR update to the owner with total pending requests.
        /// </summary>
        public async System.Threading.Tasks.Task SendOwnerPendingUpdate( int? newLeaveRequestId = null)
        {
            var owner = await _dataContext.Users.FirstOrDefaultAsync(x => x.Role == UserRoleEnum.Owner);
            if (owner != null)
            {
                var totalPendingLeaveRequests = await _dataContext.LeaveRequests.Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending).CountAsync();
                var totalPendingPermissionRequests = await _dataContext.Permissions.Where(x => x.Status == Models.PermissionStatusEnum.Pending).CountAsync();

                await _hubContext.Clients.User(owner.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingLeaveRequests + totalPendingPermissionRequests,
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

                await _hubContext.Clients.User(owner.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = totalPendingLeaveRequests + totalPendingPermissionRequests,
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
        public async System.Threading.Tasks.Task SendProjectManagersPendingUpdate( int? newLeaveRequestId = null)
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
        /// Sends SignalR updates to relevant parties (Owner, Project Managers, Team Leaders)
        /// to refresh their pending request counts after an opinion is given on a leave request.
        /// </summary>
        /// <param name="opinionGiver">The User object of the person who just gave the opinion.</param>
        /// <param name="leaveRequestId">The ID of the leave request on which the opinion was given.</param>
        /// <param name="leaveRequestStatusChanged">True if the leave request's status changed (e.g., from Pending to Approved/Rejected).</param>
        public async System.Threading.Tasks.Task SendPendingUpdatesAfterOpinion(Models.User opinionGiver, LeaveRequest leaveRequest)
        {
            if (opinionGiver.Role == UserRoleEnum.Owner)
            {
                await _hubContext.Clients.User(opinionGiver.Id.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = await _dataContext.LeaveRequests.Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending).CountAsync() +
                                await _dataContext.Permissions.Where(x => x.Status == Models.PermissionStatusEnum.Pending).CountAsync()
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

                await _hubContext.Clients.User(leaveRequest.User.TeamleaderId.ToString()).SendAsync("UpdatePendings", new
                {
                    pendings = await _dataContext.LeaveRequests
                                                        .Include(x => x.Opinions)
                                                        .Where(x => x.Status == Models.LeaveRequestStatusEnum.Pending &&
                                                                    x.User.TeamleaderId == opinionGiver.Id &&
                                                                    !x.Opinions.Any(o => o.UserId == opinionGiver.Id))
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
