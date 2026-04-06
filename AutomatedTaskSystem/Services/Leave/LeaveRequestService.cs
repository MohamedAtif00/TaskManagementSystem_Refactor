using System.Net.Mail;
using System.Net.NetworkInformation;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos;
using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Hub;
using AutomatedTaskSystem.Migrations;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.NotificationCategory;
using AutomatedTaskSystem.Models.Enums.NotificationStatus;
using AutomatedTaskSystem.Models.Enums.NotificationType;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.Email;
using AutomatedTaskSystem.Services.Log;
using AutomatedTaskSystem.Services.Notification;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using AutomatedTaskSystem.Services.YearService;
using AutomatedTaskSystem.Models.Configs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Options;
using static AutomatedTaskSystem.DTO.Responses;

namespace AutomatedTaskSystem.Services.Leave
{
    public class LeaveRequestService(DataContext dataContext,
                               ITokenService tokenService,
                               IEmailService emailService,
                               IWebHostEnvironment webHostEnvironment,
                               IHubContext<UserHub> hubContext,
                               INotificationService notificationService,
                               ILogService logger,
                               LeaveRequestHelper leaveRequestHelper,
                               IOptions<EmailRecipientSettings> emailRecipientsOptions,
                               IOptionsSnapshot<LeaveSettings> leaveSettings) : ILeaveRequestService
    {
        private readonly EmailRecipientSettings _emailRecipients = emailRecipientsOptions.Value;
        private readonly LeaveSettings _leaveSettings = leaveSettings?.Value ?? new LeaveSettings();

        // Enhanced service method
        public async Task<ResponseService<bool>> CreateLeaveRequest(CreateLeaveRequestDto request)
        {
            var response = new ResponseService<bool>();

            var user = await dataContext.Users.FirstOrDefaultAsync(x => x.Id == request.UserId);
            var alreadyUsedFromNext = user.FromNextBalanceDaysUsed;
            if (user == null)
            {
                response.Error = true;
                response.Message = "User not found.";
                response.Data = false;
                return response;
            }

            // Parse and validate dates
            if (!DateTime.TryParse(request.StartDate, out var startDate) || !DateTime.TryParse(request.EndDate, out var endDate))
            {
                response.Error = true;
                response.Message = "Invalid start or end date.";
                response.Data = false;
                return response;
            }

            if (endDate < startDate)
            {
                response.Error = true;
                response.Message = "End date cannot be earlier than start date.";
                response.Data = false;
                return response;
            }

            // Use helper for calculation
            int requestedDays = leaveRequestHelper.CalculateWorkingDays(startDate, endDate);
            var today = DateTime.Today;
            int currentYear = today.Year;

            // --- Emergency blackout: after cutoff date, block emergency until reset date ---
            if (request.type == LeaveRequestType.Emergency)
            {
                var cutoff = LeaveSettings.ParseDateForYear(_leaveSettings.EmergencyBlackoutCutoffDate, currentYear);
                var resetDate = LeaveSettings.ParseDateForYear(_leaveSettings.ResetDate, currentYear);
                bool emergencyAllowed = (cutoff == null && resetDate == null) ||
                    (cutoff != null && today <= cutoff.Value) ||
                    (resetDate != null && today >= resetDate.Value);
                if (!emergencyAllowed)
                {
                    response.Error = true;
                    response.Message = "Emergency leave requests are temporarily disabled until the annual leave reset. You can submit other leave types.";
                    response.Data = false;
                    return response;
                }
            }

            // --- Start of Revised Leave Type Specific Logic ---
            int fromNextBalanceDaysToRecord = 0;
            int pendingFromNextBalanceDays = 0;
            if (request.type == LeaveRequestType.Annual || request.type == LeaveRequestType.FromNextBalance)
                pendingFromNextBalanceDays = await leaveRequestHelper.GetPendingWorkingDaysAsync(user.Id, LeaveRequestType.FromNextBalance);

            // FromNextBalance (direct or via Annual confirmation) is not available after the reset date
            if ((request.type == LeaveRequestType.FromNextBalance || request.ConfirmFromNextBalance) &&
                LeaveSettings.ParseDateForYear(_leaveSettings.ResetDate, currentYear) is { } resetDateVal && today >= resetDateVal)
            {
                response.Error = true;
                response.Message = "From next balance is not available after the annual leave reset.";
                response.Data = false;
                return response;
            }

            // Annual: always check balance; when request exceeds remaining, require confirmation and set fromNextBalanceDaysToRecord so we can split into two requests
            if (request.type == LeaveRequestType.Annual)
            {
                var pendingAnnualLeaveDays = await leaveRequestHelper.GetPendingLeaveDaysAsync(user.Id, LeaveRequestType.Annual);
                int remainingAnnualLeave = user.Annual_leave_MAX - user.Annual_leave;
                int neededFromNext = 0;

                if ((pendingAnnualLeaveDays + requestedDays) > remainingAnnualLeave)
                {
                    neededFromNext = (pendingAnnualLeaveDays + requestedDays) - remainingAnnualLeave;
                    var windowStart = LeaveSettings.ParseDateForYear(_leaveSettings.FromNextBalanceStartDate, currentYear);
                    var windowEnd = LeaveSettings.ParseDateForYear(_leaveSettings.FromNextBalanceEndDate, currentYear);
                    bool inFromNextWindow = windowStart != null && windowEnd != null && today >= windowStart.Value && today <= windowEnd.Value;

                    if (!inFromNextWindow)
                    {
                        response.Error = true;
                        response.Message = $"Requested annual leave exceeds available balance. Remaining: {remainingAnnualLeave - pendingAnnualLeaveDays} days. Using next balance is only allowed between {_leaveSettings.FromNextBalanceStartDate} and {_leaveSettings.FromNextBalanceEndDate}.";
                        response.Data = false;
                        return response;
                    }

                    int totalFromNextAfterRequest = alreadyUsedFromNext + pendingFromNextBalanceDays + neededFromNext;
                    if (neededFromNext > _leaveSettings.FromNextBalanceMaxDays || totalFromNextAfterRequest > _leaveSettings.FromNextBalanceMaxDays)
                    {
                        response.Error = true;
                        response.Message = $"You can use at most {_leaveSettings.FromNextBalanceMaxDays} days from next balance (already used: {alreadyUsedFromNext}, pending: {pendingFromNextBalanceDays}). This request would need {neededFromNext} from next balance.";
                        response.Data = false;
                        return response;
                    }

                    if (request.ConfirmFromNextBalance != true)
                    {
                        response.Error = true;
                        response.Message = "Please confirm that you accept using days from next year balance.";
                        response.Data = false;
                        return response;
                    }
                    fromNextBalanceDaysToRecord = neededFromNext;
                }
            }
            else if (request.type == LeaveRequestType.Emergency)
            {
                var pendingEmergencyLeaveDays = await leaveRequestHelper.GetPendingLeaveDaysAsync(user.Id, LeaveRequestType.Emergency);
                int remainingEmergencyLeave = user.Emergency_leave_MAX - user.Emergency_leave;

                if ((pendingEmergencyLeaveDays + requestedDays) > remainingEmergencyLeave)
                {
                    response.Error = true;
                    response.Message = $"Requested emergency leave exceeds available emergency leave balance. Remaining: {remainingEmergencyLeave - pendingEmergencyLeaveDays} days.";
                    response.Data = false;
                    return response;
                }
            }
            else if (request.type == LeaveRequestType.UnpaidLeave)
            {
                // No balance check for unpaid leave.
            }
            else if (request.type == LeaveRequestType.FromNextBalance)
            {
                var windowStart = LeaveSettings.ParseDateForYear(_leaveSettings.FromNextBalanceStartDate, currentYear);
                var windowEnd = LeaveSettings.ParseDateForYear(_leaveSettings.FromNextBalanceEndDate, currentYear);
                bool inWindow = windowStart != null && windowEnd != null && today >= windowStart.Value && today <= windowEnd.Value;
                if (!inWindow)
                {
                    response.Error = true;
                    response.Message = $"From next balance is only available between {_leaveSettings.FromNextBalanceStartDate} and {_leaveSettings.FromNextBalanceEndDate}.";
                    response.Data = false;
                    return response;
                }
                int totalFromNextAfterRequest = alreadyUsedFromNext + pendingFromNextBalanceDays + requestedDays;
                if (requestedDays > _leaveSettings.FromNextBalanceMaxDays || totalFromNextAfterRequest > _leaveSettings.FromNextBalanceMaxDays)
                {
                    response.Error = true;
                    int availableFromNext = Math.Max(0, _leaveSettings.FromNextBalanceMaxDays - alreadyUsedFromNext - pendingFromNextBalanceDays);
                    response.Message = $"From next balance is limited to {_leaveSettings.FromNextBalanceMaxDays} days (already used: {alreadyUsedFromNext}, pending: {pendingFromNextBalanceDays}). You have {availableFromNext} days available. This request is for {requestedDays} days.";
                    response.Data = false;
                    return response;
                }
            }
            else if (request.type == LeaveRequestType.Sick)
            {
                if (requestedDays > 3 && request.MedicalCertificate == null)
                {
                    response.Error = true;
                    response.Message = "Medical certificate is required for sick leave longer than 3 days.";
                    response.Data = false;
                    return response;
                }

                // Use helper for medical certificate validation
                if (request.MedicalCertificate != null && !leaveRequestHelper.IsValidMedicalCertificate(request.MedicalCertificate))
                {
                    response.Error = true;
                    response.Message = "Invalid medical certificate file.";
                    response.Data = false;
                    return response;
                }
            }
            // --- End of Revised Leave Type Specific Logic ---

            // When user has partial annual balance and confirms from-next, split into two leave requests: Annual + FromNextBalance
            var segments = new List<(DateTime Start, DateTime End, LeaveRequestType Type)>();
            bool doSplit = request.type == LeaveRequestType.Annual && request.ConfirmFromNextBalance && fromNextBalanceDaysToRecord > 0;
            if (doSplit)
            {
                int annualDays = requestedDays - fromNextBalanceDaysToRecord;
                if (annualDays > 0)
                {
                    var (s1, e1, s2, e2) = leaveRequestHelper.SplitDateRangeByWorkingDays(startDate, endDate, annualDays);
                    if (s1.HasValue && e1.HasValue) segments.Add((s1.Value, e1.Value, LeaveRequestType.Annual));
                    if (s2.HasValue && e2.HasValue) segments.Add((s2.Value, e2.Value, LeaveRequestType.FromNextBalance));
                }
                else
                    segments.Add((startDate, endDate, LeaveRequestType.FromNextBalance));
            }
            else
                segments.Add((startDate, endDate, request.ConfirmFromNextBalance ? LeaveRequestType.FromNextBalance : request.type));

            var initialStatus = (user.Role == UserRoleEnum.Owner) ? LeaveRequestStatusEnum.Approved : LeaveRequestStatusEnum.Pending;
            var createdRequests = new List<LeaveRequest>();

            int? sectionHeadId = null;
            if (user.Role == UserRoleEnum.TeamLeader)
            {
                var section = await dataContext.Sections
                    .Where(s => s.SectionGroups.Any(g => g.GroupId == user.GroupId))
                    .FirstOrDefaultAsync();
                if (section != null)
                    sectionHeadId = section.HeadId;
            }

            foreach (var (segStart, segEnd, segType) in segments)
            {
                var leaveRequest = new LeaveRequest
                {
                    UserId = user.Id,
                    StartDate = segStart,
                    EndDate = segEnd,
                    Reason = request.Reason,
                    Status = initialStatus,
                    Type = segType,
                    NoteForManager = request.NoteForManager,
                    DateCreated = DateTime.Now,
                    SectionheadId = sectionHeadId
                };
                dataContext.LeaveRequests.Add(leaveRequest);
                createdRequests.Add(leaveRequest);
            }

		    try
		    {
		        await dataContext.SaveChangesAsync();
		        	
		        if (request.type == LeaveRequestType.Sick && request.MedicalCertificate != null && createdRequests.Count == 1)
		        {
		        	var leaveRequest = createdRequests[0];
		        	var medicalCertPath = await leaveRequestHelper.SaveMedicalCertificate(request.MedicalCertificate, leaveRequest.Id, webHostEnvironment);
		        	leaveRequest.MedicalCertificatePath = medicalCertPath;
		        	leaveRequest.MedicalCertificateFileName = request.MedicalCertificate.FileName;
		        	dataContext.LeaveRequests.Update(leaveRequest);
		        	await dataContext.SaveChangesAsync();
		        }
		        	
		        // --- Auto-approval and email for Owner (each created request) ---
		        if (user.Role == UserRoleEnum.Owner)
		        {
		        	foreach (var leaveRequest in createdRequests)
		        	{
		        	    int approvedDays = CalculateWorkingDays(leaveRequest.StartDate, leaveRequest.EndDate);
		        	    switch (leaveRequest.Type)
		        	    {
		        	        case LeaveRequestType.Annual:
		        	            user.Annual_leave += approvedDays;
		        	            break;
		        	        case LeaveRequestType.Emergency:
		        	            user.Emergency_leave += approvedDays;
		        	            break;
		        	        case LeaveRequestType.Sick:
		        	            user.Sick_leave += approvedDays;
		        	            break;
		        	        case LeaveRequestType.UnpaidLeave:
		        	            break;
		        	        case LeaveRequestType.FromNextBalance:
		        	            user.FromNextBalanceDaysUsed += approvedDays;
		        	            var resetDateOwner = LeaveSettings.ParseDateForYear(_leaveSettings.ResetDate, DateTime.Today.Year);
		        	            if (resetDateOwner != null && DateTime.Today >= resetDateOwner.Value)
		        	                user.Annual_leave += approvedDays;
		        	            break;
		        	    }
		        	}
		        	dataContext.Users.Update(user);
		        	await dataContext.SaveChangesAsync();
		        	
		        	foreach (var leaveRequest in createdRequests)
		        	{
		        	    var message = new EmailMessage
		        	    {
		        	        Subject = "طلب أجازة ",
		        	        Body = EmailTemplate.CreateTemplate(user.Name,
		        	            user.Email,
		        	            leaveRequest.StartDate.ToString("yyyy-MM-dd"),
		        	            leaveRequest.EndDate.ToString("yyyy-MM-dd"),
		        	            leaveRequestHelper.CalculateWorkingDays(leaveRequest.StartDate, leaveRequest.EndDate),
		        	            leaveRequest.Type,
		        	            user.HR_code),
		        	        IsHtml = true,
		        	        CcEmails = new List<string> { _emailRecipients.CEO, user.Email }
		        	    };
		        	    if (!string.IsNullOrEmpty(leaveRequest.MedicalCertificatePath))
		        	    {
		        	        var fullFilePath = Path.Combine(webHostEnvironment.WebRootPath, leaveRequest.MedicalCertificatePath);
		        	        if (File.Exists(fullFilePath))
		        	        {
		        	            var attachment = new System.Net.Mail.Attachment(fullFilePath);
		        	            attachment.Name = leaveRequest.MedicalCertificateFileName;
		        	            message.Attachments.Add(attachment);
		        	        }
		        	    }
		        	    var emailResult = await emailService.SendEmailAsync(message);
		        	    if (!emailResult.Success)
		        	        Console.WriteLine($"Error sending auto-approval email for Owner's LeaveRequest {leaveRequest.Id}: {emailResult.Message}");
		        	
		        	    await notificationService.CreateNotification(
		        	        user.Id,
		        	        "Leave Request Automatically Approved",
		        	        $"{leaveRequest.Type} leave from {leaveRequest.StartDate:yyyy-MM-dd} to {leaveRequest.EndDate:yyyy-MM-dd} has been automatically approved.",
		        	        NotificationCategoryEnum.Leaves,
		        	        NotificationTypeEnum.Leave,
		        	        relatedEntityId: leaveRequest.Id,
		        	        hasActions: false,
		        	        status: NotificationStatusEnum.Accepted);
		        	}
		        	
		        	await hubContext.Clients.User(user.Id.ToString()).SendAsync("LeaveRequestOpinion", new
		        	{
		        	    isApproved = true,
		        	    message = "Your leave request(s) have been automatically approved."
		        	});
		        }
		        else
		        {
		        	var firstRequestId = createdRequests[0].Id;
		        	if (user.Role == UserRoleEnum.ProjectManger)
		        	    await leaveRequestHelper.SendOwnerPendingUpdate(firstRequestId);
		        	else if (user.Role == UserRoleEnum.TeamLeader)
		        	{
		        	    await leaveRequestHelper.SendOwnerPendingUpdate(firstRequestId);
		        	    await leaveRequestHelper.SendProjectManagersPendingUpdate(firstRequestId);
		        	}
		        	else
		        	{
		        	    await leaveRequestHelper.SendOwnerPendingUpdate(firstRequestId);
		        	    await leaveRequestHelper.SendProjectManagersPendingUpdate(firstRequestId);
		        	    if (user.TeamleaderId.HasValue)
		        	    {
		        	        var teamLeader = await dataContext.Users.FirstOrDefaultAsync(u => u.Id == user.TeamleaderId.Value);
		        	        if (teamLeader != null)
		        	            await leaveRequestHelper.SendTeamLeaderPendingUpdates(teamLeader.Id, firstRequestId);
		        	        else
		        	            Console.WriteLine($"Warning: Team leader with ID {user.TeamleaderId.Value} not found for user {user.Id}.");
		        	    }
		        	    else
		        	        Console.WriteLine($"Warning: User {user.Id} does not have a TeamleaderId.");
		        	}
		        	
		        	foreach (var leaveRequest in createdRequests)
		        	{
		        	    var workingDays = CalculateWorkingDays(leaveRequest.StartDate, leaveRequest.EndDate);
		        	    await notificationService.CreateNotification(
		        	        user.Id,
		        	        "Leave Request Submitted",
		        	        $"{leaveRequest.Type} leave from {leaveRequest.StartDate:yyyy-MM-dd} to {leaveRequest.EndDate:yyyy-MM-dd} ({workingDays} working days) has been submitted and is pending review.",
		        	        NotificationCategoryEnum.Leaves,
		        	        NotificationTypeEnum.Leave,
		        	        relatedEntityId: leaveRequest.Id,
		        	        hasActions: false,
		        	        status: NotificationStatusEnum.Pending);
		        	
		        	    var approvers = new List<Models.User>();
		        	    var owner = await dataContext.Users.FirstOrDefaultAsync(x => x.Role == UserRoleEnum.Owner);
		        	    if (owner != null && owner.Id != user.Id) approvers.Add(owner);
		        	    if (user.GroupId.HasValue)
		        	    {
		        	        var projectManagers = await dataContext.SectionGroups
		        	            .Where(sg => sg.GroupId == user.GroupId.Value)
		        	            .Select(sg => sg.Section.Head)
		        	            .Distinct()
		        	            .ToListAsync();
		        	        approvers.AddRange(projectManagers.Where(pm => pm != null && pm.Id != user.Id));
		        	    }
		        	    if (user.TeamleaderId.HasValue && user.TeamleaderId.Value != user.Id)
		        	    {
		        	        var teamLeaderUser = await dataContext.Users.FirstOrDefaultAsync(u => u.Id == user.TeamleaderId.Value);
		        	        if (teamLeaderUser != null) approvers.Add(teamLeaderUser);
		        	    }
		        	    var approverIds = approvers.Where(a => a != null).Select(a => a.Id).Distinct().ToList();
		        	    var approverMessage = $"{user.Name} submitted a {leaveRequest.Type} leave request from {leaveRequest.StartDate:yyyy-MM-dd} to {leaveRequest.EndDate:yyyy-MM-dd} ({workingDays} working days).";
		        	    foreach (var approverId in approverIds)
		        	        await notificationService.CreateNotification(
		        	            approverId,
		        	            "New Leave Request Pending Review",
		        	            approverMessage,
		        	            NotificationCategoryEnum.Leaves,
		        	            NotificationTypeEnum.Leave,
		        	            relatedEntityId: leaveRequest.Id,
		        	            hasActions: true,
		        	            status: NotificationStatusEnum.Pending);
		        	}
		        }
		        
		        response.Data = true;
		        response.Message = "Leave request created successfully." + (createdRequests.Count > 1 ? " Your request was split into " + createdRequests.Count + " entries (annual + from next balance)." : "") + (user.Role == UserRoleEnum.Owner ? " It has been automatically approved." : "");
		    }
            catch (Exception ex)
            {
                response.Error = true;
                response.Message = $"An error occurred while saving the leave request: {ex.Message}";
                response.Data = false;
            }

            return response;
        }
        public class OperationResult
        {
            public bool error { get; set; }
            public string Message { get; set; }
            public bool data { get; set; }

            public static OperationResult Succeeded(string message = "Operation completed successfully.")
            {
                return new OperationResult { error = false, Message = message,data =true };
            }

            public static OperationResult Failed(string message = "Operation failed.")
            {
                return new OperationResult { error = true, Message = message };
            }
        }
        // Update the return type of the method
        public async Task<OperationResult> GiveOpinion(CreateOpinionDto request)
        {
            try
            {
                var userIdResult = tokenService.GetUserIdFromToken();
                if (userIdResult.Data == null)
                {
                    logger.LogError(null, "Token service returned null userId.Data for request: {@Request}", request);
                    await notificationService.ErrorNotification(userIdResult.ToString(), "Could not retrieve user ID from token.");
                    // Return with a specific reason
                    return OperationResult.Failed("Could not retrieve user ID from token.");
                }

                var user = await dataContext.Users
                    .FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(userIdResult.Data));

                if (user == null)
                {
                    logger.LogError(null, "User with ID {UserId} not found when giving opinion for LeaveRequestId {LeaveRequestId}.", userIdResult.Data, request.LeaveRequestId);
                    await notificationService.ErrorNotification(userIdResult.ToString(), "This user does not exist.");
                    // Return with a specific reason
                    return OperationResult.Failed("User does not exist.");
                }

                var leaveRequest = await dataContext.LeaveRequests
                    .Include(x => x.User)
                    .Include(lr => lr.Opinions)
                    .FirstOrDefaultAsync(lr => lr.Id == request.LeaveRequestId);

                if (leaveRequest == null)
                {
                    logger.LogError(null, "Leave request with ID {LeaveRequestId} not found for user {UserId}.", request.LeaveRequestId, user.Id);
                    await notificationService.ErrorNotification(userIdResult.ToString(), "Leave request not found.");
                    // Return with a specific reason
                    return OperationResult.Failed("Leave request not found.");
                }

                if (leaveRequest.Status == LeaveRequestStatusEnum.Cancelled)
                {
                    logger.LogWarning("User {UserId} attempted to give opinion on cancelled leave request {LeaveRequestId}.", user.Id, request.LeaveRequestId);
                    await notificationService.ErrorNotification(userIdResult.ToString(), "Cannot give opinion on a cancelled leave request.");
                    // Return with a specific reason
                    return OperationResult.Failed("Cannot give opinion on a cancelled leave request.");
                }

                if (leaveRequest.Opinions.Any(o => o.UserId == user.Id))
                {
                    logger.LogWarning("User {UserId} already gave opinion on leave request {LeaveRequestId}.", user.Id, request.LeaveRequestId);
                    await notificationService.ErrorNotification(userIdResult.ToString(), "You have already given your opinion on this request.");
                    // Return with a specific reason
                    return OperationResult.Failed("You have already given your opinion on this request.");
                }

                var opinion = new Opinion
                {
                    UserId = user.Id,
                    LeaveRequestId = request.LeaveRequestId,
                    IsApproved = request.IsApproved,
                    Comment = request.Comment,
                    CreatedAt = DateTime.UtcNow,
                };

                switch (user.Role)
                {
                    case UserRoleEnum.Owner:
                        if (request.IsApproved)
                        {
                            var senderUser = leaveRequest.User;
                            var message = new EmailMessage
                            {
                                Subject = "أجازة",
                                Body = EmailTemplate.CreateTemplate(senderUser.Name,
                                                                    senderUser.Email,
                                                                    leaveRequest.StartDate.ToString("yyyy-MM-dd"),
                                                                    leaveRequest.EndDate.ToString("yyyy-MM-dd"),
                                                                    CalculateWorkingDays(leaveRequest.StartDate, leaveRequest.EndDate),
                                                                    leaveRequest.Type,
                                                                    leaveRequest.User.HR_code),
                                IsHtml = true,
                                CcEmails = _emailRecipients.SectionHead != null?  new List<string> { _emailRecipients.SectionHead??null, leaveRequest.User.Email}:
                                 new List<string> { leaveRequest.User.Email }// Note: Your EmailService doesn't send CCs if commented out
                            };

                            if (!string.IsNullOrEmpty(leaveRequest.MedicalCertificatePath))
                            {
                                var fullFilePath = Path.Combine(webHostEnvironment.WebRootPath, leaveRequest.MedicalCertificatePath);
                                if (File.Exists(fullFilePath))
                                {
                                    var attachment = new System.Net.Mail.Attachment(fullFilePath);
                                    attachment.Name = leaveRequest.MedicalCertificateFileName;
                                    message.Attachments.Add(attachment);
                                }
                                else
                                {
                                    logger.LogWarning("Medical certificate file not found at {FilePath} for LeaveRequest {LeaveRequestId}.", fullFilePath, leaveRequest.Id);
                                }
                            }

                            var emailResult = await emailService.SendEmailAsync(message);

                            if (emailResult.Success)
                            {
                                leaveRequest.Status = LeaveRequestStatusEnum.Approved;

                                int approvedDays = CalculateWorkingDays(leaveRequest.StartDate, leaveRequest.EndDate);

                                switch (leaveRequest.Type)
                                {
                                    case LeaveRequestType.Annual:
                                        senderUser.Annual_leave += approvedDays;
                                        break;
                                    case LeaveRequestType.Emergency:
                                        senderUser.Emergency_leave += approvedDays;
                                        break;
                                    case LeaveRequestType.Sick:
                                        senderUser.Sick_leave += approvedDays;
                                        // --- ADDED LOGIC HERE ---
                                        if (!string.IsNullOrEmpty(leaveRequest.MedicalCertificatePath))
                                        {
                                            var fullFilePath = Path.Combine(webHostEnvironment.WebRootPath, leaveRequest.MedicalCertificatePath);
                                            try
                                            {
                                                if (File.Exists(fullFilePath))
                                                {
                                                    File.Delete(fullFilePath);
                                                    logger.LogInformation("Deleted medical certificate file: {FilePath} for LeaveRequest {LeaveRequestId}.", fullFilePath, leaveRequest.Id);
                                                }
                                            }
                                            catch (IOException ioEx)
                                            {
                                                logger.LogError(ioEx, "Error deleting medical certificate file: {FilePath} for LeaveRequest {LeaveRequestId}.", fullFilePath, leaveRequest.Id);
                                            }
                                        }
                                        // --- END ADDED LOGIC ---
                                        senderUser.Sick_leave += approvedDays; // This line seems to be a duplicate. If it's intended to increase sick leave, you'd only need one. I'm leaving it as is in your original code.
                                        break;
                                    case LeaveRequestType.UnpaidLeave:
                                        break;
                                    case LeaveRequestType.FromNextBalance:
                                        senderUser.FromNextBalanceDaysUsed += approvedDays;
                                        // After reset: count FromNextBalance days as used annual leave for current year
                                        var resetDateOpinion = LeaveSettings.ParseDateForYear(_leaveSettings.ResetDate, DateTime.Today.Year);
                                        if (resetDateOpinion != null && DateTime.Today >= resetDateOpinion.Value)
                                            senderUser.Annual_leave += approvedDays;
                                        break;
                                }

                                dataContext.Opinions.Add(opinion);
                                dataContext.Users.Update(senderUser);
                                dataContext.LeaveRequests.Update(leaveRequest);

                                await hubContext.Clients.User(leaveRequest.UserId.ToString()).SendAsync("LeaveRequestOpinion", new
                                {
                                    isApproved = true,
                                    message = "Your leave request has been approved."
                                });
		
		                                // Persistent notification for requester when Owner approves
		                                var approveTitle = "Leave Request Approved";
		                                var approveMessage = $"Your {leaveRequest.Type} leave request from {leaveRequest.StartDate:yyyy-MM-dd} to {leaveRequest.EndDate:yyyy-MM-dd} has been approved.";
		                                await notificationService.CreateNotification(
		                                    leaveRequest.UserId,
		                                    approveTitle,
		                                    approveMessage,
		                                    NotificationCategoryEnum.Leaves,
		                                    NotificationTypeEnum.Leave,
		                                    relatedEntityId: leaveRequest.Id,
		                                    hasActions: false,
		                                    status: NotificationStatusEnum.Accepted);
                            }
                            else
                            {
                                logger.LogError(emailResult.Exception, // Pass the actual exception here
                                            "Error sending approval email for LeaveRequest {LeaveRequestId} to user {RequesterUserId}: {ErrorMessage}",
                                            leaveRequest.Id,
                                            leaveRequest.UserId,
                                            emailResult.Message);
                                await notificationService.ErrorNotification(userIdResult.ToString(), "Leave request could not be fully processed due to email delivery failure. Please try again or contact support.");
                                // Return with specific reason from email service
                                return OperationResult.Failed($"Leave request could not be fully processed due to email delivery failure: {emailResult.Message}");
                            }
                        }
                        else // Owner rejects
                        {
                            leaveRequest.Status = LeaveRequestStatusEnum.Rejected;
                            dataContext.Opinions.Add(opinion);
                            dataContext.LeaveRequests.Update(leaveRequest);

                            await hubContext.Clients.User(leaveRequest.UserId.ToString()).SendAsync("LeaveRequestOpinion", new
                            {
                                isApproved = false,
                                message = "Your leave request has been rejected."
                            });
		
		                            // Persistent notification for requester when Owner rejects
		                            var rejectTitle = "Leave Request Rejected";
		                            var rejectMessage = $"Your {leaveRequest.Type} leave request from {leaveRequest.StartDate:yyyy-MM-dd} to {leaveRequest.EndDate:yyyy-MM-dd} has been rejected.";
		                            await notificationService.CreateNotification(
		                                leaveRequest.UserId,
		                                rejectTitle,
		                                rejectMessage,
		                                NotificationCategoryEnum.Leaves,
		                                NotificationTypeEnum.Leave,
		                                relatedEntityId: leaveRequest.Id,
		                                hasActions: false,
		                                status: NotificationStatusEnum.Declined);
                        }
                        break;

                    case UserRoleEnum.TeamLeader:
                    case UserRoleEnum.ProjectManger:
                    case UserRoleEnum.SectionHead:
                        dataContext.Opinions.Add(opinion);
                        break;

                    default:
                        logger.LogWarning("Unauthorized user {UserId} with role {UserRole} attempted to give opinion on leave request {LeaveRequestId}.", user.Id, user.Role, request.LeaveRequestId);
                        await notificationService.ErrorNotification(userIdResult.ToString(), "You are not authorized to give opinions on this leave request.");
                        // Return with a specific reason
                        return OperationResult.Failed("You are not authorized to give opinions on this leave request.");
                }

                await dataContext.SaveChangesAsync();

                await leaveRequestHelper.SendPendingUpdatesAfterOpinion(user, leaveRequest);
                if (user.Role == UserRoleEnum.Owner)
                {
                    await leaveRequestHelper.SendProjectManagersPendingUpdateWithoutNew(leaveRequest.Id);
                    if(leaveRequest.User.TeamleaderId != null)
                    await leaveRequestHelper.SendTeamLeaderPendingUpdatesWithoutNew(leaveRequest.User.TeamleaderId!.Value); 
                }

              
                    logger.LogInformation("Opinion successfully given for LeaveRequest {LeaveRequestId} by user {UserId}. IsApproved: {IsApproved}", request.LeaveRequestId, user.Id, request.IsApproved);
                // Return success with a generic success message
                return OperationResult.Succeeded("Opinion successfully recorded.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unhandled error occurred in GiveOpinion for LeaveRequestId {LeaveRequestId} by user {UserId}.", request.LeaveRequestId);
                // Return a general failure message for unhandled exceptions
                return OperationResult.Failed("An unexpected error occurred while processing your opinion. Please try again.");
            }
        }


        /// <summary>
        /// Gives opinions (approve/reject) on multiple leave requests in bulk.
        /// Each individual opinion is processed by the existing GiveOpinion method.
        /// </summary>
        /// <param name="request">A DTO containing a list of leave request IDs and the desired status.</param>
        /// <returns>An OperationResult summarizing the overall outcome of the bulk operation.</returns>
        public async Task<OperationResult> GiveBulkOpinion(CreateBulkOpinionDto request)
        {
            logger.LogInformation("Attempting to process {Count} bulk opinions.", request.Ids.Length);

            if (request.Ids == null || !request.Ids.Any())
            {
                logger.LogWarning("No leave request IDs provided for bulk opinion operation.");
                return OperationResult.Failed("No leave request IDs provided for bulk operation.");
            }

            // Determine IsApproved based on the provided Status
            bool? isApproved = null;
            if (request.Status == LeaveRequestStatusEnum.Approved)
            {
                isApproved = true;
            }
            else if (request.Status == LeaveRequestStatusEnum.Rejected)
            {
                isApproved = false;
            }
            else
            {
                logger.LogError(null, "Invalid status '{Status}' provided for bulk opinion. Only Approved or Rejected are allowed.", request.Status);
                return OperationResult.Failed("Invalid status for bulk opinion. Only 'Approved' or 'Rejected' statuses are allowed.");
            }

            bool overallSuccess = true;
            int successfulCount = 0;
            int failedCount = 0;
            var failedMessages = new List<string>();

            foreach (var leaveRequestId in request.Ids)
            {
                var opinionDto = new CreateOpinionDto
                {
                    LeaveRequestId = leaveRequestId,
                    IsApproved = isApproved.Value, // Use the determined approval status
                };

                var individualResult = await GiveOpinion(opinionDto);

                if (!individualResult.error)
                {
                    successfulCount++;
                }
                else
                {
                    failedCount++;
                    overallSuccess = false; // Mark overall operation as failed if any individual one fails
                    failedMessages.Add($"Leave Request ID {leaveRequestId}: {individualResult.Message}");
                    logger.LogError(null, "Individual opinion for LeaveRequest {LeaveRequestId} failed: {Message}", leaveRequestId, individualResult.Message);
                }
            }

            string finalMessage;
            if (overallSuccess)
            {
                finalMessage = $"Successfully processed all {successfulCount} opinions.";
            }
            else
            {
                finalMessage = $"Processed {successfulCount} opinions successfully, but {failedCount} failed. Details: {string.Join("; ", failedMessages)}";
            }

            logger.LogInformation("Bulk opinion operation completed. {Message}", finalMessage);

            return overallSuccess
                ? OperationResult.Succeeded(finalMessage)
                : OperationResult.Failed(finalMessage);
        }


        public async Task<List<GetOpinion>> GetAllOpinionsForLeaveRequest(int leaveRequestId) { 
            return await dataContext.Opinions.Where(x => x.LeaveRequestId == leaveRequestId)
                .Select(x => new GetOpinion
                {
                    Id = x.Id,
                    LeaveRequestId = x.LeaveRequestId,
                    IsApproved = x.IsApproved,
                    Comment = x.Comment,
                    DateCreated = x.CreatedAt.ToString("M/d/yyyy h:mm:ss tt")
                })
                .ToListAsync();
        }
        public async Task<ResponseService<GetOpinion>> GetSingleOpinion(int id) { 
            var op = await dataContext.Opinions
                .Select(x => new GetOpinion
                {
                    Id = x.Id,
                    LeaveRequestId = x.LeaveRequestId,
                    IsApproved = x.IsApproved,
                    Comment = x.Comment,
                    DateCreated = x.CreatedAt.ToString("M/d/yyyy h:mm:ss tt")
                })
                .FirstOrDefaultAsync(x=> x.Id == id);

            return new ResponseService<GetOpinion>
            {
                Error = false,
                Message = "Opinion retrieved successfully.",
                Data = op
            };
        }

        public async Task<ResponseService<PageList<GetLeaveRequestDto>>> GetAllVacationsAsync(int page = 1, int pageSize = 10, string? searchTerm = null, string? fromDate = null, string? toDate = null, string? status = null, string? myStatus = null, string? type = null, bool disablePagination = false)
        {
        // 1. Get the ResponseService<string> from the token service
        var tokenResponse = tokenService.GetUserIdFromToken();

        // 2. Check if the token response itself indicates an error or has no data
        if (tokenResponse == null || tokenResponse.Error || string.IsNullOrWhiteSpace(tokenResponse.Data))
        {
            return new ResponseService<PageList<GetLeaveRequestDto>>
            {
                Error = true,
                Message = tokenResponse?.Message ?? "Failed to retrieve user ID from token.",
                Data = null
            };
        }

        // 3. Safely try to parse the string data from the tokenResponse.Data
        if (!int.TryParse(tokenResponse.Data, out var currentUserId)) // Renamed to currentUserId for clarity
        {
            return new ResponseService<PageList<GetLeaveRequestDto>>
            {
                Error = true,
                Message = "Invalid user ID format in token.",
                Data = null
            };
        }

        var user = await dataContext.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);
        if (user == null)
        {
            return new ResponseService<PageList<GetLeaveRequestDto>>
            {
                Error = true,
                Message = "User associated with the token not found.",
                Data = null
            };
        }

        var query = dataContext.LeaveRequests.Where(x => x.Status != LeaveRequestStatusEnum.Cancelled)
            .Include(v => v.User)
            .Include(v => v.Opinions) 
            .AsQueryable();

        if (user.Role == UserRoleEnum.ProjectManger)
            query = query.Where(x => x.User.Id != user.Id);

        if (user.Role == UserRoleEnum.TeamLeader && dataContext.Users.Any(x => x.TeamleaderId == user.Id))
        {
            query = query.Where(v => v.User.GroupId == user.GroupId && v.User.Id != user.Id);
        }
        else if (user.Role == UserRoleEnum.SectionHead)
        {
            query = query.Where(v => v.User.Group != null && 
                                v.User.Group.sectionGroups.Any(sg =>
                                    sg.Section != null && 
                                    sg.Section.HeadId == currentUserId));
        }
        else if(user.Role == UserRoleEnum.TeamLeader && !dataContext.Users.Any(x => x.TeamleaderId == user.Id))
        {
            return new ResponseService<PageList<GetLeaveRequestDto>>
            {
                Error = false,
                Message = "there is no members in your team.",
                Data = null
            };
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(v => v.User.Name.Contains(searchTerm) ||
                                         v.Reason.Contains(searchTerm));
            }

        // 3. Apply Date Range Filtering
        if (!string.IsNullOrWhiteSpace(fromDate) && DateTime.TryParse(fromDate, out DateTime parsedFromDate))
        {
            query = query.Where(v => v.StartDate.Date >= parsedFromDate.Date);
        }

        if (!string.IsNullOrWhiteSpace(toDate) && DateTime.TryParse(toDate, out DateTime parsedToDate))
        {
            query = query.Where(v => v.EndDate.Date <= parsedToDate.Date);
        }

        // 4. Apply Status Filtering (for the overall request status)
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse(typeof(LeaveRequestStatusEnum), status, true, out var parsedStatus))
        {
            query = query.Where(v => v.Status == (LeaveRequestStatusEnum)parsedStatus);
        }

        // 5. Apply Type Filtering
        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse(typeof(LeaveRequestType), type, true, out var parsedType))
        {
            query = query.Where(v => v.Type == (LeaveRequestType)parsedType);
        }

        // 6. Apply MyStatus Filtering (based on current user's opinion status)
        if (!string.IsNullOrWhiteSpace(myStatus))
        {
            var normalizedMyStatusFilter = myStatus.ToLowerInvariant(); // For case-insensitive comparison

            switch (normalizedMyStatusFilter)
            {
                case "approved": // This can mean "Opinion Approved" OR "Request Approved (user didn't opine)"
                    query = query.Where(x =>
                        (x.Opinions.Any(o => o.UserId == currentUserId && o.IsApproved)) || // User opined and it's approved
                        (!x.Opinions.Any(o => o.UserId == currentUserId) && x.Status == LeaveRequestStatusEnum.Approved) // User didn't opine and request is approved
                    );
                    break;
                case "rejected": // This can mean "Opinion Rejected" OR "Request Rejected (user didn't opine)"
                    query = query.Where(x =>
                        (x.Opinions.Any(o => o.UserId == currentUserId && !o.IsApproved)) || // User opined and it's not approved
                        (!x.Opinions.Any(o => o.UserId == currentUserId) && x.Status == LeaveRequestStatusEnum.Rejected) // User didn't opine and request is rejected
                    );
                    break;
                case "pending": // This means user hasn't opined AND the request is pending
                    query = query.Where(x => !x.Opinions.Any(o => o.UserId == currentUserId) && x.Status == LeaveRequestStatusEnum.Pending);
                    break;
                case "cancelled": // User hasn't opined and request is cancelled
                    query = query.Where(x => !x.Opinions.Any(o => o.UserId == currentUserId) && x.Status == LeaveRequestStatusEnum.Cancelled);
                    break;
                default:
                    // Handle unsupported myStatus values or ignore
                    break;
            }
        }

        // --- ORDERING IS CRUCIAL FOR PAGINATION/CONSISTENCY ---
        query = query.OrderByDescending(v => v.DateCreated);

        IQueryable<GetLeaveRequestDto> projectedQuery;

        // The projection logic for MyStatus
        // This logic needs to be applied consistently, regardless of role.
        // The role-based filtering happens *before* the projection.
        projectedQuery = query.Select(x => new GetLeaveRequestDto
        {
            Id = x.Id,
            // Ensure DTO properties are 'string' for date-only output
            StartDate = x.StartDate.ToString("yyyy-MM-dd"),
            EndDate = x.EndDate.ToString("yyyy-MM-dd"),
            Reason = x.Reason,
            Status = x.Status.ToString(),
            // NEW MyStatus Logic:
            MyStatus = x.Opinions.Any(o => o.UserId == currentUserId) // Check if the current user (userId) has an opinion
                       ? ( // If YES (user has an opinion):
                             x.Opinions.FirstOrDefault(o => o.UserId == currentUserId).IsApproved // Get their opinion and check its approval status
                             ? LeaveRequestStatusEnum.Approved.ToString() // If their opinion is Approved
                             : LeaveRequestStatusEnum.Rejected.ToString() // If their opinion is NOT Approved (Rejected)
                         )
                       : ( // If NO (user has NOT added an opinion):
                             x.Status == LeaveRequestStatusEnum.Pending // AND the request's overall status is Pending
                             ? LeaveRequestStatusEnum.Pending.ToString() // Then MyStatus is "Pending" (for them to act)
                             : x.Status.ToString() // Else (request is NOT Pending, e.g., Approved/Rejected/Cancelled by others), MyStatus is the request's actual Status
                         ),
            Duration = CalculateWorkingDays(x.StartDate, x.EndDate),
            Type = x.Type.ToString(),
            user = new IDName
            {
                Id = x.User.Id,
                Name = x.User.Name
            },
            // Format DateCreated if it's DateTime? in your model and you want only the date string
            DateCreated = x.DateCreated.HasValue ? x.DateCreated.Value.ToString("yyyy-MM-dd") : null
        });


        PageList<GetLeaveRequestDto> pagedResult;

        if (disablePagination)
        {
            var allItems = await projectedQuery.ToListAsync();
            pagedResult = new PageList<GetLeaveRequestDto>(allItems, 1, allItems.Count > 0 ? allItems.Count : 1, allItems.Count);
        }
        else
        {
            pagedResult = await PageList<GetLeaveRequestDto>.CreateAsync(
                projectedQuery,
                page,
                pageSize
            );
        }

        return new ResponseService<PageList<GetLeaveRequestDto>>
        {
            Error = false,
            Message = "Vacations retrieved successfully.",
            Data = pagedResult
        };
    }



        //public async Task<ResponseService<List<GetLeaveRequestDto>>> GetBelongToTm()
    //{

    //} 

    // ✅ Read Single
        public async Task<ActionResult<ResponseService<GetLeaveRequestDto>>> GetVacationByIdAsync(int id)
        {
            var result = await dataContext.LeaveRequests
                .Include(v => v.User)
                 .Select(x => new GetLeaveRequestDto
                 {
                     Id = x.Id,
                     StartDate = x.StartDate.ToString(),
                     EndDate = x.EndDate.ToString(),
                     Reason = x.Reason,
                     Status = x.Status.ToString()
                 })
                .FirstOrDefaultAsync(v => v.Id == id);

            if (result == null) return new ResponseService<GetLeaveRequestDto>
            {
                Error = true,
                Message = "Vacation not found.",
                Data = null
            };

            return new ResponseService<GetLeaveRequestDto>
            {
                Error = false,
                Message = "Vacation retrieved successfully.",
                Data = result
            };
        }


        public async Task<ResponseService<GetSingleLeaveRequestDto>> GetLeaveRequestByUserId(int requestId)
        {
            var id = tokenService.GetUserIdFromToken().Data;
            var user = await dataContext.Users.FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(id));

            // Get the leave request with all necessary includes
            var leaveRequest = await dataContext.LeaveRequests
                .Where(x => x.Id == requestId)
                .Include(x => x.User)
                    .ThenInclude(u => u.Group)
                .Include(x => x.User)
                    .ThenInclude(u => u.Teamleader)
                        .ThenInclude(tl => tl.Group)
                .Include(x => x.Opinions)
                    .ThenInclude(o => o.User)
                .FirstOrDefaultAsync();

            if (leaveRequest == null)
            {
                return new ResponseService<GetSingleLeaveRequestDto>
                {
                    Error = true,
                    Message = "Leave request not found.",
                    Data = null
                };
            }

            var requestDetails = new GetSingleLeaveRequestDto
            {
                Id = leaveRequest.Id,
                StartDate = leaveRequest.StartDate.ToString("M/d/yyyy h:mm:ss tt"),
                EndDate = leaveRequest.EndDate.ToString("M/d/yyyy h:mm:ss tt"),
                Duration = CalculateWorkingDays(leaveRequest.StartDate, leaveRequest.EndDate), // Exclude end date
                Reason = leaveRequest.Reason,
                NoteToManager = user.Role == UserRoleEnum.Owner ? leaveRequest.NoteForManager : null,
                Status = leaveRequest.Status.ToString(),
                Type = leaveRequest.Type.ToString(),
                DateCreated = leaveRequest.DateCreated?.ToString("M/d/yyyy h:mm:ss tt"),
                user = new UserDTO
                {
                    Id = leaveRequest.User.Id,
                    Name = leaveRequest.User.Name,
                    Archived = leaveRequest.User.Archived,
                    Role = leaveRequest.User.Role,
                    GroupId = leaveRequest.User.GroupId,
                    Group = leaveRequest.User.Group != null
                        ? new IDName { Id = leaveRequest.User.Group.Id, Name = leaveRequest.User.Group.Name }
                        : null,
                    TeamleaderId = leaveRequest.User.TeamleaderId,
                    Teamleader = leaveRequest.User.Teamleader != null
                        ? new UserDTO
                        {
                            Id = leaveRequest.User.Teamleader.Id,
                            Name = leaveRequest.User.Teamleader.Name,
                            Role = leaveRequest.User.Teamleader.Role,
                            GroupId = leaveRequest.User.Teamleader.GroupId,
                            Group = leaveRequest.User.Teamleader.Group != null
                                ? new IDName
                                {
                                    Id = leaveRequest.User.Teamleader.Group.Id,
                                    Name = leaveRequest.User.Teamleader.Group.Name
                                }
                                : null
                        }
                        : null,
                    HrCode = leaveRequest.User.HR_code,
                    Email = leaveRequest.User.Email,
                    Phone = leaveRequest.User.Phone,
                    Title = leaveRequest.User.Title,
                    AccountType = leaveRequest.User.AccountType,
                    Annual_leave = leaveRequest.User.Annual_leave,
                    Annual_leave_MAX = leaveRequest.User.Annual_leave_MAX,
                    Sick_leave = leaveRequest.User.Sick_leave,
                    Emergency_leave = leaveRequest.User.Emergency_leave,
                    Emergency_leave_MAX = leaveRequest.User.Emergency_leave_MAX,
                    Permission = leaveRequest.User.Permission,
                    Permission_MAX = leaveRequest.User.Permission_MAX,
                    Vacation = new VacationDto
                    {
                        Annual = leaveRequest.User.Annual_leave,
                        Sick = leaveRequest.User.Sick_leave,
                        Emergency = leaveRequest.User.Emergency_leave,
                        Annual_MAX = leaveRequest.User.Annual_leave_MAX,
                        Emergency_MAX = leaveRequest.User.Emergency_leave_MAX,
                    },
                    OnBoard = leaveRequest.User.OnBoard
                },
                Opinions = leaveRequest.Opinions.Select(o => new GetOpinion
                {
                    Id = o.Id,
                    LeaveRequestId = o.LeaveRequestId,
                    IsApproved = o.IsApproved,
                    Comment = o.Comment,
                    DateCreated = o.CreatedAt.ToString("M/d/yyyy h:mm:ss tt"),
                    User = o.User != null ? new IDNameWithRole
                    {
                        Id = o.User.Id,
                        Name = o.User.Name,
                        Role = o.User.Role
                    } : null
                }).ToList()
            };

            return new ResponseService<GetSingleLeaveRequestDto>
            {
                Error = false,
                Message = "Leave request retrieved successfully.",
                Data = requestDetails
            };
        }


        public async Task<ResponseService<PageList<GetLeaveRequestDto>>> GetLeaveRequestsByUserId(int userId, int page, int pageSize, string? searchTerm, string? fromDate, string? toDate, string? status, string? type, bool disablePagination)
        {
            try
            {

                // 1. Build the IQueryable for LeaveRequests, filtered by UserId and including related entities
                var query = dataContext.LeaveRequests
                    .Where(x => x.UserId == userId)
                    .Include(x => x.User)
                    .ThenInclude(x => x.Group)
                    .AsQueryable(); // Start with AsQueryable to allow for conditional filtering

                // Apply filters conditionally
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(x =>
                        x.Reason.Contains(searchTerm) ||
                        x.User.Name.Contains(searchTerm) ||
                        x.Type.ToString().Contains(searchTerm) ||
                        x.Status.ToString().Contains(searchTerm)
                    );
                }

                if (!string.IsNullOrWhiteSpace(fromDate) && DateTime.TryParse(fromDate, out DateTime parsedFromDate))
                {
                    query = query.Where(x => x.StartDate >= parsedFromDate);
                }

                if (!string.IsNullOrWhiteSpace(toDate) && DateTime.TryParse(toDate, out DateTime parsedToDate))
                {
                    // Add one day to include the entire 'toDate'
                    query = query.Where(x => x.EndDate <= parsedToDate.AddDays(1));
                }

                if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse(typeof(LeaveRequestStatusEnum), status, true, out var parsedStatus))
                {
                    query = query.Where(x => x.Status == (LeaveRequestStatusEnum)parsedStatus);
                }

                if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse(typeof(LeaveRequestType), type, true, out var parsedType))
                {
                    query = query.Where(x => x.Type == (LeaveRequestType)parsedType);
                }

                // Project the query to GetLeaveRequestDto *after* filtering
                var projectedQuery = query.Select(x => new GetLeaveRequestDto
                {
                    Id = x.Id,
                    StartDate = x.StartDate.ToString("yyyy-MM-dd"),
                    EndDate = x.EndDate.ToString("yyyy-MM-dd"),
                    Duration = CalculateWorkingDays(x.StartDate, x.EndDate), // Assuming CalculateWorkingDays is accessible
                    Reason = x.Reason,
                    Status = x.Status.ToString(),
                    Type = x.Type.ToString(),
                    DateCreated = x.DateCreated.HasValue ? x.DateCreated.Value.ToString("M/d/yyyy h:mm:ss tt") : null,
                    user = new IDName
                    {
                        Id = x.User.Id,
                        Name = x.User.Name
                    }
                });

                PageList<GetLeaveRequestDto> paginatedLeaveRequests;

                if (disablePagination)
                {
                    var allItems = await projectedQuery.ToListAsync();
                    paginatedLeaveRequests = new PageList<GetLeaveRequestDto>(allItems, allItems.Count, 1, allItems.Count);
                }
                else
                {
                    // Use PageList.CreateAsync to paginate the query
                    paginatedLeaveRequests = await PageList<GetLeaveRequestDto>.CreateAsync(projectedQuery, page, pageSize);
                }

                if (paginatedLeaveRequests.items == null || !paginatedLeaveRequests.items.Any())
                {
                    return new ResponseService<PageList<GetLeaveRequestDto>>
                    {
                        Error = false,
                        Message = "No leave requests found for this user with the applied filters.",
                        Data = paginatedLeaveRequests
                    };
                }

                return new ResponseService<PageList<GetLeaveRequestDto>>
                {
                    Error = false,
                    Message = "Leave requests retrieved successfully.",
                    Data = paginatedLeaveRequests
                };
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logging framework like Serilog, NLog, or built-in ILogger)
                Console.WriteLine($"Error in GetLeaveRequestsByUserId: {ex.Message}");
                return new ResponseService<PageList<GetLeaveRequestDto>>
                {
                    Error = true,
                    Message = $"An error occurred while retrieving leave requests: {ex.Message}",
                    Data = null
                };
            }
        }
        // ✅ Update
        public async Task<bool> UpdateVacationAsync(int id, DateTime startDate, DateTime endDate, string? reason, LeaveRequestStatusEnum status)
        {
            var vacation = await dataContext.LeaveRequests.FindAsync(id);
            if (vacation == null)
                return false;

            vacation.StartDate = startDate;
            vacation.EndDate = endDate;
            vacation.Reason = reason;
            vacation.Status = status;

            await dataContext.SaveChangesAsync();
            return true;
        }

        // ✅ Delete
        public async Task<bool> DeleteVacationAsync(int id)
        {
            var vacation = await dataContext.LeaveRequests.FindAsync(id);
            if (vacation == null)
                return false;

            dataContext.LeaveRequests.Remove(vacation);
            await dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<ResponseService<bool>> CancelleLeave(int id)
        {
            try
            {
                var leaveRequest = await dataContext.LeaveRequests
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (leaveRequest == null)
                    return new ResponseService<bool> { Error = true, Message = "Leave request not found.", Data = false };

                var now = DateTime.Now;

                if ((leaveRequest.Status == LeaveRequestStatusEnum.Pending) ||
                    (leaveRequest.Status == LeaveRequestStatusEnum.Approved && leaveRequest.StartDate > now))
                {
                    var senderUser = leaveRequest.User;
                    bool wasApproved = leaveRequest.Status == LeaveRequestStatusEnum.Approved;

                    leaveRequest.Status = LeaveRequestStatusEnum.Cancelled;

                    if (wasApproved)
                    {
                        var leaveDays = CalculateWorkingDays(leaveRequest.StartDate, leaveRequest.EndDate);

                        // Refund leave days (only the part that was deducted from current balance)
                        switch (leaveRequest.Type)
                        {
                            case LeaveRequestType.Annual:
                                senderUser.Annual_leave -= leaveDays;
                                break;

                            case LeaveRequestType.Emergency:
                                senderUser.Emergency_leave -= leaveDays;
                                break;

                            case LeaveRequestType.Sick:
                                senderUser.Sick_leave -= leaveDays;
                                break;

                            case LeaveRequestType.UnpaidLeave:
                                break;
                            case LeaveRequestType.FromNextBalance:
                                senderUser.FromNextBalanceDaysUsed -= leaveDays;
                                // If this leave was approved after reset (leave starts on or after reset date), we had added to Annual_leave; refund it on cancel
                                var resetDateCancel = LeaveSettings.ParseDateForYear(_leaveSettings.ResetDate, leaveRequest.StartDate.Year);
                                if (resetDateCancel != null && leaveRequest.StartDate >= resetDateCancel.Value)
                                    senderUser.Annual_leave -= leaveDays;
                                break;
                        }

                        // Prepare email
                        var message = new EmailMessage
                        {
                            Subject = "إلغاء طلب الأجازة",
                            Body = EmailTemplate.CreateLeaveCancellationTemplate(
                                        senderUser.Name,
                                        senderUser.Email,
                                        leaveRequest.StartDate.ToString("yyyy-MM-dd"),
                                        leaveRequest.EndDate.ToString("yyyy-MM-dd"),
                                        leaveDays,
                                        leaveRequest.Type,
                                        senderUser.HR_code),
                            IsHtml = true,
                            CcEmails = new List<string>() // Initialize an empty list first
                        };

                        // --- MODIFIED LOGIC HERE ---

                        // If the sender (the one who requested the leave) is an Owner, add CEO FIRST
                        if (senderUser.Role == UserRoleEnum.Owner)
                        {
                            if (!string.IsNullOrEmpty(_emailRecipients.CEO))
                            {
                                message.CcEmails.Add(_emailRecipients.CEO);
                            }
                        }

                        // Always add the sender's email
                        message.CcEmails.Add(senderUser.Email);

                        // Add SectionHead email if it exists and is not null (this will come after CEO if owner, or after sender)
                        if (!string.IsNullOrEmpty(_emailRecipients.SectionHead))
                        {
                            message.CcEmails.Add(_emailRecipients.SectionHead);
                        }

                        // --- END MODIFIED LOGIC ---

                        var result = await emailService.SendEmailAsync(message);
                       

                        if (!result.Success)
                        {
                            logger.LogError(result.Exception, $"Error sending leave cancellation email for request {id}. Message: {result.Message}");
                            return new ResponseService<bool>
                            {
                                Error = true,
                                Message = "The email wasn't sent. Please contact support.",
                                Data = false
                            };
                        }
                    }


		                    await dataContext.SaveChangesAsync();

		                    // Persistent notification for requester when leave is cancelled
		                    var cancelTitle = "Leave Request Cancelled";
		                    var cancelMessage = $"Your {leaveRequest.Type} leave request from {leaveRequest.StartDate:yyyy-MM-dd} to {leaveRequest.EndDate:yyyy-MM-dd} has been cancelled.";
		                    await notificationService.CreateNotification(
		                        leaveRequest.UserId,
		                        cancelTitle,
		                        cancelMessage,
		                        NotificationCategoryEnum.Leaves,
		                        NotificationTypeEnum.Leave,
		                        relatedEntityId: leaveRequest.Id,
		                        hasActions: false,
		                        status: null);

		                    if (senderUser.Role != UserRoleEnum.Owner)
                    {
                        await leaveRequestHelper.SendOwnerPendingUpdate();
                        if(senderUser.Role != UserRoleEnum.ProjectManger)
                        await leaveRequestHelper.SendProjectManagersPendingUpdate();

                        if (senderUser.TeamleaderId != null)
                        {
                            await leaveRequestHelper.SendTeamLeaderPendingUpdates(senderUser.TeamleaderId.Value);

                        }
                    }
                    return new ResponseService<bool>
                    {
                        Error = false,
                        Message = "Leave cancelled successfully.",
                        Data = true
                    };
                }

                return new ResponseService<bool>
                {
                    Error = true,
                    Message = "You cannot cancel a leave that has already passed or is currently active.",
                    Data = false
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An unhandled error occurred while cancelling leave request with ID {id}.");
                return new ResponseService<bool> { Error = true, Message = "An error occurred.", Data = false };
            }
        }

        private async Task<List<Models.User>> GetReceiversAsync(UserRoleEnum userRole)
        {
            List<UserRoleEnum> rolesToNotify = userRole switch
            {
                UserRoleEnum.Member => new List<UserRoleEnum> { UserRoleEnum.TeamLeader, UserRoleEnum.SectionHead, UserRoleEnum.Owner },
                UserRoleEnum.TeamLeader => new List<UserRoleEnum> { UserRoleEnum.SectionHead, UserRoleEnum.Owner },
                UserRoleEnum.SectionHead => new List<UserRoleEnum> { UserRoleEnum.Owner },
                UserRoleEnum.Owner => new List<UserRoleEnum>(), // No one to notify
                _ => new List<UserRoleEnum>()
            };

            if (!rolesToNotify.Any())
                return new List<Models.User>();

            var users = await dataContext.Users
                .Where(u => rolesToNotify.Contains(u.Role))
                .ToListAsync();

            return users;
        }

        public async Task<ResponseService<LeavePreviewDto>> PreviewAnnualLeave(int userId, string startDateStr, string endDateStr)
        {
            var response = new ResponseService<LeavePreviewDto>();
            if (!DateTime.TryParse(startDateStr, out var startDate) || !DateTime.TryParse(endDateStr, out var endDate))
            {
                response.Error = true;
                response.Message = "Invalid start or end date.";
                response.Data = new LeavePreviewDto { ErrorMessage = "Invalid start or end date." };
                return response;
            }
            if (endDate < startDate)
            {
                response.Error = true;
                response.Message = "End date cannot be earlier than start date.";
                response.Data = new LeavePreviewDto { ErrorMessage = response.Message };
                return response;
            }

            var user = await dataContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                response.Error = true;
                response.Message = "User not found.";
                response.Data = new LeavePreviewDto { ErrorMessage = response.Message };
                return response;
            }

            int requestedDays = leaveRequestHelper.CalculateWorkingDays(startDate, endDate);
            var pendingAnnualLeaveDays = await leaveRequestHelper.GetPendingLeaveDaysAsync(user.Id, LeaveRequestType.Annual);
            int remainingAnnualLeave = user.Annual_leave_MAX - user.Annual_leave;
            int availableAnnual = remainingAnnualLeave - pendingAnnualLeaveDays;
            if (availableAnnual < 0) availableAnnual = 0;

            int neededFromNext = 0;
            bool needsConfirmation = false;
            int alreadyUsedFromNext = user.FromNextBalanceDaysUsed;
            int pendingFromNextBalanceDays = await leaveRequestHelper.GetPendingWorkingDaysAsync(user.Id, LeaveRequestType.FromNextBalance);
            var today = DateTime.Today;
            int currentYear = today.Year;

            if ((pendingAnnualLeaveDays + requestedDays) > remainingAnnualLeave)
            {
                neededFromNext = (pendingAnnualLeaveDays + requestedDays) - remainingAnnualLeave;
                var windowStart = LeaveSettings.ParseDateForYear(_leaveSettings.FromNextBalanceStartDate, currentYear);
                var windowEnd = LeaveSettings.ParseDateForYear(_leaveSettings.FromNextBalanceEndDate, currentYear);
                bool inWindow = windowStart != null && windowEnd != null && today >= windowStart.Value && today <= windowEnd.Value;

                if (!inWindow)
                {
                    response.Error = true;
                    response.Message = $"Using next balance is only allowed between {_leaveSettings.FromNextBalanceStartDate} and {_leaveSettings.FromNextBalanceEndDate}.";
                    response.Data = new LeavePreviewDto { ErrorMessage = response.Message, RequestedDays = requestedDays, AvailableAnnual = availableAnnual, NeededFromNext = neededFromNext, FromNextBalanceMaxDays = _leaveSettings.FromNextBalanceMaxDays, PendingFromNext = pendingFromNextBalanceDays };
                    return response;
                }

                int totalFromNextAfterRequest = alreadyUsedFromNext + pendingFromNextBalanceDays + neededFromNext;
                if (neededFromNext > _leaveSettings.FromNextBalanceMaxDays || totalFromNextAfterRequest > _leaveSettings.FromNextBalanceMaxDays)
                {
                    response.Error = true;
                    response.Message = $"You can use at most {_leaveSettings.FromNextBalanceMaxDays} days from next balance (already used: {alreadyUsedFromNext}, pending: {pendingFromNextBalanceDays}).";
                    response.Data = new LeavePreviewDto { ErrorMessage = response.Message, RequestedDays = requestedDays, AvailableAnnual = availableAnnual, NeededFromNext = neededFromNext, FromNextBalanceMaxDays = _leaveSettings.FromNextBalanceMaxDays, AlreadyUsedFromNext = alreadyUsedFromNext, PendingFromNext = pendingFromNextBalanceDays };
                    return response;
                }
                needsConfirmation = true;
            }

            response.Data = new LeavePreviewDto
            {
                RequestedDays = requestedDays,
                AvailableAnnual = availableAnnual,
                NeededFromNext = neededFromNext,
                FromNextBalanceMaxDays = _leaveSettings.FromNextBalanceMaxDays,
                AlreadyUsedFromNext = alreadyUsedFromNext,
                PendingFromNext = pendingFromNextBalanceDays,
                NeedsConfirmation = needsConfirmation
            };
            response.Message = needsConfirmation ? "Part of this request would use next year balance. Please confirm." : "OK";
            return response;
        }

        private static int CalculateWorkingDays(DateTime startDate, DateTime endDate)
        {
            //if (startDate > endDate)
            //    return 0;

            int workingDays = 0;

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (IsWorkingDay(date))
                {
                    workingDays++;
                }
            }

            return workingDays;
        }

        private static bool IsWorkingDay(DateTime date)
        {
            // Define working days (e.g., Sunday to Thursday)
            return date.DayOfWeek != DayOfWeek.Friday &&
                   date.DayOfWeek != DayOfWeek.Saturday;
        }

        private bool IsValidMedicalCertificate(IFormFile file)
        {
            // Check file size (max 10MB)
            const long maxFileSize = 10 * 1024 * 1024;
            if (file.Length > maxFileSize)
                return false;

            // Check file extension
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                return false;

            // Check MIME type
            var allowedMimeTypes = new[]
            {
                "application/pdf",
                "image/jpeg",
                "image/jpg",
                "image/png"
            };

            return allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant());
        }
        // Save medical certificate
        private async Task<string> SaveMedicalCertificate(IFormFile file, int requestId)
        {
            var uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "medical-certificates");
            Directory.CreateDirectory(uploadsFolder);

            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"medical_req_{requestId}_{DateTime.Now:yyyyMMdd_HHmmss}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Path.Combine("uploads", "medical-certificates", fileName);
        }


        //private string GetContentType(string fileName)
        //{
        //    var extension = Path.GetExtension(fileName).ToLowerInvariant();
        //    return extension switch
        //    {
        //        ".pdf" => "application/pdf",
        //        ".jpg" or ".jpeg" => "image/jpeg",
        //        ".png" => "image/png",
        //        ".gif" => "image/gif",
        //        ".doc" => "application/msword",
        //        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        //        _ => "application/octet-stream"
        //    };
        //}
    }


    // EmailAttachment class:
    //public class EmailAttachment
    //{
    //    public string FileName { get; set; }
    //    public byte[] Content { get; set; }
    //    public string ContentType { get; set; }
    //}
}
