using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Hub;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.Email;
using AutomatedTaskSystem.Services.Log;
using AutomatedTaskSystem.Services.Notification;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace AutomatedTaskSystem.Helper
{
    public class WorkFromHomeHelper
    {
        private readonly DataContext _dataContext;
        private readonly IHubContext<UserHub> _hubContext;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly ILogService _logService;
        private readonly EmailRecipientSettings _emailRecipients;

        public WorkFromHomeHelper(
            DataContext dataContext,
            IHubContext<UserHub> hubContext,
            INotificationService notificationService,
            IEmailService emailService,
            ILogService logService,
            IOptions<EmailRecipientSettings> emailRecipientsOptions)
        {
            _dataContext = dataContext;
            _hubContext = hubContext;
            _notificationService = notificationService;
            _emailService = emailService;
            _logService = logService;
            _emailRecipients = emailRecipientsOptions.Value;
        }

        // Methods for sending SignalR notifications (adapted from LeaveRequestHelper)

        public async System.Threading.Tasks.Task SendOwnerPendingUpdate(int? workFromHomeRequestId = null)
        {
            var owner = await _dataContext.Users.FirstOrDefaultAsync(u => u.Role == UserRoleEnum.Owner);
            if (owner != null)
            {
                var pendingCount = await _dataContext.WorkFromHomeRequests
                    .Where(wfh => wfh.Status == WorkFromHomeStatusEnum.Pending)
                    .CountAsync();
                await _hubContext.Clients.User(owner.Id.ToString()).SendAsync("ReceiveWorkFromHomePendingCount", pendingCount);
                if (workFromHomeRequestId.HasValue)
                {
                    await _notificationService.SuccessNotification(owner.Id.ToString(), $"New work from home request (ID: {workFromHomeRequestId.Value}) is pending your review.");
                }
            }
        }

        public async System.Threading.Tasks.Task SendProjectManagersPendingUpdate(int? workFromHomeRequestId = null)
        {
            var projectManagers = await _dataContext.Users.Where(u => u.Role == UserRoleEnum.ProjectManger).ToListAsync();
            foreach (var pm in projectManagers)
            {
                var pendingCount = await _dataContext.WorkFromHomeRequests
                    .Where(wfh => wfh.Status == WorkFromHomeStatusEnum.Pending && wfh.User.TeamleaderId == pm.Id) // Assuming PMs oversee specific teams
                    .CountAsync();
                await _hubContext.Clients.User(pm.Id.ToString()).SendAsync("ReceiveWorkFromHomePendingCount", pendingCount);
                if (workFromHomeRequestId.HasValue)
                {
                    await _notificationService.SuccessNotification(pm.Id.ToString(), $"New work from home request (ID: {workFromHomeRequestId.Value}) is pending your review.");
                }
            }
        }

        public async System.Threading.Tasks.Task SendTeamLeaderPendingUpdates(int teamLeaderId, int? workFromHomeRequestId = null)
        {
            var teamLeader = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == teamLeaderId && u.Role == UserRoleEnum.TeamLeader);
            if (teamLeader != null)
            {
                var pendingCount = await _dataContext.WorkFromHomeRequests
                    .Where(wfh => wfh.Status == WorkFromHomeStatusEnum.Pending && wfh.User.TeamleaderId == teamLeaderId)
                    .CountAsync();
                await _hubContext.Clients.User(teamLeader.Id.ToString()).SendAsync("ReceiveWorkFromHomePendingCount", pendingCount);
                if (workFromHomeRequestId.HasValue)
                {
                    await _notificationService.SuccessNotification(teamLeader.Id.ToString(), $"New work from home request (ID: {workFromHomeRequestId.Value}) is pending your review.");
                }
            }
        }

        public async System.Threading.Tasks.Task SendSectionHeadPendingUpdates(int sectionHeadId, int? workFromHomeRequestId = null)
        {
            var sectionHead = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == sectionHeadId && u.Role == UserRoleEnum.SectionHead);
            if (sectionHead != null)
            {
                var pendingCount = await _dataContext.WorkFromHomeRequests
                    .Where(wfh => wfh.Status == WorkFromHomeStatusEnum.Pending && wfh.SectionheadId == sectionHeadId)
                    .CountAsync();
                await _hubContext.Clients.User(sectionHead.Id.ToString()).SendAsync("ReceiveWorkFromHomePendingCount", pendingCount);
                if (workFromHomeRequestId.HasValue)
                {
                    await _notificationService.SuccessNotification(sectionHead.Id.ToString(), $"New work from home request (ID: {workFromHomeRequestId.Value}) is pending your review.");
                }
            }
        }

        // Logic to decide which notifications to send after an opinion is given
        public async System.Threading.Tasks.Task SendPendingUpdatesAfterOpinion(User user, WorkFromHomeRequest workFromHomeRequest)
        {
            // If the request is now approved or rejected, all pending notifications for it are resolved
            if (workFromHomeRequest.Status != WorkFromHomeStatusEnum.Pending)
            {
                await SendOwnerPendingUpdate(); // Update owner's count
                await SendProjectManagersPendingUpdate(); // Update PM's count
                if (workFromHomeRequest.TeamleaderId.HasValue)
                {
                    await SendTeamLeaderPendingUpdates(workFromHomeRequest.TeamleaderId.Value);
                }
                if (workFromHomeRequest.SectionheadId.HasValue)
                {
                    await SendSectionHeadPendingUpdates(workFromHomeRequest.SectionheadId.Value);
                }
                // Also update the requester's notifications if the request is no longer pending their approvers' opinions
                await _notificationService.SuccessNotification(workFromHomeRequest.UserId.ToString(), $"Your work from home request for {workFromHomeRequest.Date.ToShortDateString()} has been {workFromHomeRequest.Status.ToString().ToLower()}.");
            }
            else // Still pending, send notification to the next approver if any
            {
                // This logic might need refinement based on your exact approval flow
                // Example: If TeamLeader approved, notify SectionHead
                if (user.Role == UserRoleEnum.TeamLeader && workFromHomeRequest.SectionheadId.HasValue)
                {
                    await SendSectionHeadPendingUpdates(workFromHomeRequest.SectionheadId.Value, workFromHomeRequest.Id);
                }
                // If Project Manager approved, notify Section Head / Owner (if not already done)
                else if (user.Role == UserRoleEnum.ProjectManger)
                {
                    // Decide who to notify next based on your hierarchy
                    await SendOwnerPendingUpdate(workFromHomeRequest.Id);
                }
            }
        }

    }
}
