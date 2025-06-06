using System.Net.Mail;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos;
using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Hub;
using AutomatedTaskSystem.Migrations;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.Email;
using AutomatedTaskSystem.Services.Log;
using AutomatedTaskSystem.Services.Notification;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using AutomatedTaskSystem.Services.YearService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static AutomatedTaskSystem.DTO.Responses;

namespace AutomatedTaskSystem.Services.Leave
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly DataContext _dataContext;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<UserHub> _hubContext;
        private readonly INotificationService _notificationService;
        private readonly ILogService _logService; // <--- ADD THIS
        private readonly LeaveRequestHelper _leaveRequestHelper;
        public LeaveRequestService(DataContext dataContext,
                                   ITokenService tokenService,
                                   IEmailService emailService,
                                   IWebHostEnvironment webHostEnvironment,
                                   IHubContext<UserHub> hubContext,
                                   INotificationService notificationService,
                                   ILogService logger,
                                   LeaveRequestHelper leaveRequestHelper)
        {
            _dataContext = dataContext;
            _tokenService = tokenService;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
            _notificationService = notificationService;
            _logService = logger;
            _leaveRequestHelper = leaveRequestHelper;
        }
        // Enhanced service method
        public async Task<ResponseService<bool>> CreateLeaveRequest(CreateLeaveRequestDto request)
        {
            var response = new ResponseService<bool>();

            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == request.UserId);
            if (user == null)
            {
                response.Error = true;
                response.Message = "User not found.";
                response.Data = false;
                return response;
            }

            // Parse and validate dates
            if (!DateTime.TryParse(request.StartDate, out var startDate) ||
                !DateTime.TryParse(request.EndDate, out var endDate))
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
            int requestedDays = _leaveRequestHelper.CalculateWorkingDays(startDate, endDate);

            // --- Start of Revised Leave Type Specific Logic ---

            if (request.type == LeaveRequestType.Annual)
            {
                var pendingAnnualLeaveDays = await _leaveRequestHelper.GetPendingLeaveDaysAsync(user.Id, LeaveRequestType.Annual);
                int remainingAnnualLeave = user.Annual_leave_MAX - user.Annual_leave;

                if ((pendingAnnualLeaveDays + requestedDays) > remainingAnnualLeave)
                {
                    response.Error = true;
                    response.Message = $"Requested annual leave exceeds available annual leave balance. Remaining: {remainingAnnualLeave - pendingAnnualLeaveDays} days.";
                    response.Data = false;
                    return response;
                }
            }
            else if (request.type == LeaveRequestType.Emergency)
            {
                var pendingEmergencyLeaveDays = await _leaveRequestHelper.GetPendingLeaveDaysAsync(user.Id, LeaveRequestType.Emergency);
                int remainingEmergencyLeave = user.Emergency_leave_MAX - user.Emergency_leave;

                if ((pendingEmergencyLeaveDays + requestedDays) > remainingEmergencyLeave)
                {
                    response.Error = true;
                    response.Message = $"Requested emergency leave exceeds available emergency leave balance. Remaining: {remainingEmergencyLeave - pendingEmergencyLeaveDays} days.";
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
                if (request.MedicalCertificate != null && !_leaveRequestHelper.IsValidMedicalCertificate(request.MedicalCertificate))
                {
                    response.Error = true;
                    response.Message = "Invalid medical certificate file.";
                    response.Data = false;
                    return response;
                }
            }
            // --- End of Revised Leave Type Specific Logic ---

            var initialStatus = (user.Role == UserRoleEnum.Owner) ? LeaveRequestStatusEnum.Approved : LeaveRequestStatusEnum.Pending;

            var leaveRequest = new LeaveRequest
            {
                UserId = user.Id,
                StartDate = startDate,
                EndDate = endDate,
                Reason = request.Reason,
                Status = initialStatus,
                Type = request.type,
                NoteForManager = request.NoteForManager,
                DateCreated = DateTime.Now
            };

            if (user.Role == UserRoleEnum.TeamLeader)
            {
                var section = await _dataContext.Sections
                    .Where(s => s.SectionGroups.Any(g => g.GroupId == user.GroupId))
                    .FirstOrDefaultAsync();

                if (section != null)
                    leaveRequest.SectionheadId = section.HeadId;
            }

            try
            {
                _dataContext.LeaveRequests.Add(leaveRequest);
                await _dataContext.SaveChangesAsync();

                if (request.type == LeaveRequestType.Sick && request.MedicalCertificate != null)
                {
                    // Use helper to save medical certificate
                    var medicalCertPath = await _leaveRequestHelper.SaveMedicalCertificate(request.MedicalCertificate, leaveRequest.Id, _webHostEnvironment);
                    leaveRequest.MedicalCertificatePath = medicalCertPath;
                    leaveRequest.MedicalCertificateFileName = request.MedicalCertificate.FileName;

                    _dataContext.LeaveRequests.Update(leaveRequest);
                    await _dataContext.SaveChangesAsync();
                }

                // --- Auto-approval and email for Owner ---
                if (user.Role == UserRoleEnum.Owner)
                {
                    int approvedDays = _leaveRequestHelper.CalculateWorkingDays(leaveRequest.StartDate, leaveRequest.EndDate);

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
                    }
                    _dataContext.Users.Update(user);
                    await _dataContext.SaveChangesAsync();

                    var message = new EmailMessage
                    {
                        Subject = "أجازة - طلب إجازة معتمد",
                        Body = EmailTemplate.CreateTemplate(user.Name,
                                                        user.Email,
                                                        leaveRequest.StartDate.ToString("yyyy-MM-dd"),
                                                        leaveRequest.EndDate.ToString("yyyy-MM-dd"),
                                                        _leaveRequestHelper.CalculateWorkingDays(leaveRequest.StartDate, leaveRequest.EndDate),
                                                        leaveRequest.Type,
                                                        leaveRequest.User.HR_code),
                        IsHtml = true,
                        CcEmails = new List<string> { user.Email }
                    };

                    if (!string.IsNullOrEmpty(leaveRequest.MedicalCertificatePath))
                    {
                        var fullFilePath = Path.Combine(_webHostEnvironment.WebRootPath, leaveRequest.MedicalCertificatePath);
                        if (File.Exists(fullFilePath))
                        {
                            var attachment = new System.Net.Mail.Attachment(fullFilePath);
                            attachment.Name = leaveRequest.MedicalCertificateFileName;
                            message.Attachments.Add(attachment);
                        }
                    }

                    var emailResult = await _emailService.SendEmailAsync(message);

                    if (!emailResult.Success)
                    {
                        Console.WriteLine($"Error sending auto-approval email for Owner's LeaveRequest {leaveRequest.Id}: {emailResult.Message}");
                    }

                    await _hubContext.Clients.User(user.Id.ToString()).SendAsync("LeaveRequestOpinion", new
                    {
                        isApproved = true,
                        message = "Your leave request has been automatically approved."
                    });
                }
                else if (user.Role == UserRoleEnum.ProjectManger)
                {
                    await _leaveRequestHelper.SendOwnerPendingUpdate( leaveRequest.Id);
                }
                else if (user.Role == UserRoleEnum.TeamLeader)
                {
                    await _leaveRequestHelper.SendOwnerPendingUpdate( leaveRequest.Id);
                    await _leaveRequestHelper.SendProjectManagersPendingUpdate(leaveRequest.Id);
                }
                else // Normal flow for non-Owner/PM/TL roles: Notify Owner/Admin and Team Leader about new pending request
                {
                    await _leaveRequestHelper.SendOwnerPendingUpdate( leaveRequest.Id);
                    await _leaveRequestHelper.SendProjectManagersPendingUpdate(leaveRequest.Id);

                    if (user.TeamleaderId.HasValue)
                    {
                        // Fetch the Team Leader's user object
                        var teamLeader = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == user.TeamleaderId.Value);

                        if (teamLeader != null)
                        {
                            await _leaveRequestHelper.SendPendingUpdatesToClient( teamLeader.Id, leaveRequest.Id);
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Team leader with ID {user.TeamleaderId.Value} not found for user {user.Id}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Warning: User {user.Id} does not have a TeamleaderId.");
                    }
                }

                response.Data = true;
                response.Message = "Leave request created successfully." + (user.Role == UserRoleEnum.Owner ? " It has been automatically approved." : "");
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
            public bool Success { get; set; }
            public string Message { get; set; }

            public static OperationResult Succeeded(string message = "Operation completed successfully.")
            {
                return new OperationResult { Success = true, Message = message };
            }

            public static OperationResult Failed(string message = "Operation failed.")
            {
                return new OperationResult { Success = false, Message = message };
            }
        }
        // Update the return type of the method
        public async Task<OperationResult> GiveOpinion(CreateOpinionDto request)
        {
            try
            {
                var userIdResult = _tokenService.GetUserIdFromToken();
                if (userIdResult.Data == null)
                {
                    _logService.LogError(null, "Token service returned null userId.Data for request: {@Request}", request);
                    await _notificationService.ErrorNotification(userIdResult.ToString(), "Could not retrieve user ID from token.");
                    // Return with a specific reason
                    return OperationResult.Failed("Could not retrieve user ID from token.");
                }

                var user = await _dataContext.Users
                    .FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(userIdResult.Data));

                if (user == null)
                {
                    _logService.LogError(null, "User with ID {UserId} not found when giving opinion for LeaveRequestId {LeaveRequestId}.", userIdResult.Data, request.LeaveRequestId);
                    await _notificationService.ErrorNotification(userIdResult.ToString(), "This user does not exist.");
                    // Return with a specific reason
                    return OperationResult.Failed("User does not exist.");
                }

                var leaveRequest = await _dataContext.LeaveRequests
                    .Include(x => x.User)
                    .Include(lr => lr.Opinions)
                    .FirstOrDefaultAsync(lr => lr.Id == request.LeaveRequestId);

                if (leaveRequest == null)
                {
                    _logService.LogError(null, "Leave request with ID {LeaveRequestId} not found for user {UserId}.", request.LeaveRequestId, user.Id);
                    await _notificationService.ErrorNotification(userIdResult.ToString(), "Leave request not found.");
                    // Return with a specific reason
                    return OperationResult.Failed("Leave request not found.");
                }

                if (leaveRequest.Status == LeaveRequestStatusEnum.Cancelled)
                {
                    _logService.LogWarning("User {UserId} attempted to give opinion on cancelled leave request {LeaveRequestId}.", user.Id, request.LeaveRequestId);
                    await _notificationService.ErrorNotification(userIdResult.ToString(), "Cannot give opinion on a cancelled leave request.");
                    // Return with a specific reason
                    return OperationResult.Failed("Cannot give opinion on a cancelled leave request.");
                }

                if (leaveRequest.Opinions.Any(o => o.UserId == user.Id))
                {
                    _logService.LogWarning("User {UserId} already gave opinion on leave request {LeaveRequestId}.", user.Id, request.LeaveRequestId);
                    await _notificationService.ErrorNotification(userIdResult.ToString(), "You have already given your opinion on this request.");
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
                                CcEmails = new List<string> { leaveRequest.User.Email } // Note: Your EmailService doesn't send CCs if commented out
                            };

                            if (!string.IsNullOrEmpty(leaveRequest.MedicalCertificatePath))
                            {
                                var fullFilePath = Path.Combine(_webHostEnvironment.WebRootPath, leaveRequest.MedicalCertificatePath);
                                if (File.Exists(fullFilePath))
                                {
                                    var attachment = new System.Net.Mail.Attachment(fullFilePath);
                                    attachment.Name = leaveRequest.MedicalCertificateFileName;
                                    message.Attachments.Add(attachment);
                                }
                                else
                                {
                                    _logService.LogWarning("Medical certificate file not found at {FilePath} for LeaveRequest {LeaveRequestId}.", fullFilePath, leaveRequest.Id);
                                }
                            }

                            var emailResult = await _emailService.SendEmailAsync(message);

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
                                        break;
                                }

                                _dataContext.Opinions.Add(opinion);
                                _dataContext.Users.Update(senderUser);
                                _dataContext.LeaveRequests.Update(leaveRequest);

                                await _hubContext.Clients.User(leaveRequest.UserId.ToString()).SendAsync("LeaveRequestOpinion", new
                                {
                                    isApproved = true,
                                    message = "Your leave request has been approved."
                                });
                            }
                            else
                            {
                                _logService.LogError(emailResult.Exception, // Pass the actual exception here
                                            "Error sending approval email for LeaveRequest {LeaveRequestId} to user {RequesterUserId}: {ErrorMessage}",
                                            leaveRequest.Id,
                                            leaveRequest.UserId,
                                            emailResult.Message);
                                await _notificationService.ErrorNotification(userIdResult.ToString(), "Leave request could not be fully processed due to email delivery failure. Please try again or contact support.");
                                // Return with specific reason from email service
                                return OperationResult.Failed($"Leave request could not be fully processed due to email delivery failure: {emailResult.Message}");
                            }
                        }
                        else // Owner rejects
                        {
                            leaveRequest.Status = LeaveRequestStatusEnum.Rejected;
                            _dataContext.Opinions.Add(opinion);
                            _dataContext.LeaveRequests.Update(leaveRequest);

                            await _hubContext.Clients.User(leaveRequest.UserId.ToString()).SendAsync("LeaveRequestOpinion", new
                            {
                                isApproved = false,
                                message = "Your leave request has been rejected."
                            });
                        }
                        break;

                    case UserRoleEnum.TeamLeader:
                    case UserRoleEnum.ProjectManger:
                        _dataContext.Opinions.Add(opinion);
                        break;

                    default:
                        _logService.LogWarning("Unauthorized user {UserId} with role {UserRole} attempted to give opinion on leave request {LeaveRequestId}.", user.Id, user.Role, request.LeaveRequestId);
                        await _notificationService.ErrorNotification(userIdResult.ToString(), "You are not authorized to give opinions on this leave request.");
                        // Return with a specific reason
                        return OperationResult.Failed("You are not authorized to give opinions on this leave request.");
                }

                await _dataContext.SaveChangesAsync();

                await _leaveRequestHelper.SendPendingUpdatesAfterOpinion(user, leaveRequest);

              
                    _logService.LogInformation("Opinion successfully given for LeaveRequest {LeaveRequestId} by user {UserId}. IsApproved: {IsApproved}", request.LeaveRequestId, user.Id, request.IsApproved);
                // Return success with a generic success message
                return OperationResult.Succeeded("Opinion successfully recorded.");
            }
            catch (Exception ex)
            {
                _logService.LogError(ex, "An unhandled error occurred in GiveOpinion for LeaveRequestId {LeaveRequestId} by user {UserId}.", request.LeaveRequestId);
                // Return a general failure message for unhandled exceptions
                return OperationResult.Failed("An unexpected error occurred while processing your opinion. Please try again.");
            }
        }
       



        public async Task<List<GetOpinion>> GetAllOpinionsForLeaveRequest(int leaveRequestId) { 
            return await _dataContext.Opinions.Where(x => x.LeaveRequestId == leaveRequestId)
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
            var op = await _dataContext.Opinions
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

        public async Task<ResponseService<PageList<GetLeaveRequestDto>>> GetAllVacationsAsync(
                  int? userId = null,
                  int? role = null,
                  int page = 1,
                  int pageSize = 10,
                  string? searchTerm = null,
                  string? fromDate = null,
                  string? toDate = null,
                  string? status = null,
                  string? type = null,
                  bool disablePagination = false // New parameter: Set to true to get all data
              )
        {
            var query = _dataContext.LeaveRequests
                .Include(v => v.User)
                .AsQueryable();

            // 1. Apply Role-Based Filtering
            if (role == (int)UserRoleEnum.TeamLeader && userId.HasValue)
            {
                query = query.Where(v => v.User.TeamleaderId == userId.Value);
            }


            // 2. Apply Search Term Filtering
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

            // 4. Apply Status Filtering
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse(typeof(LeaveRequestStatusEnum), status, true, out var parsedStatus))
            {
                query = query.Where(v => v.Status == (LeaveRequestStatusEnum)parsedStatus);
            }

            // 5. Apply Type Filtering
            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse(typeof(LeaveRequestType), type, true, out var parsedType))
            {
                query = query.Where(v => v.Type == (LeaveRequestType)parsedType);
            }

            // --- ORDERING IS CRUCIAL FOR PAGINATION/CONSISTENCY ---
            // Always order the query before applying Skip/Take or ToListAsync
            query = query.OrderByDescending(v => v.DateCreated);

            // Project to DTO before pagination/materialization for efficiency
            var projectedQuery = query.Select(x => new GetLeaveRequestDto
            {
                Id = x.Id,
                StartDate = x.StartDate.ToString("yyyy-MM-dd"),
                EndDate = x.EndDate.ToString("yyyy-MM-dd"),
                Reason = x.Reason,
                Status = x.Status.ToString(),
                Duration = CalculateWorkingDays(x.StartDate, x.EndDate),
                Type = x.Type.ToString(),
                user = new IDName
                {
                    Id = x.User.Id,
                    Name = x.User.Name
                },
                DateCreated = x.DateCreated.ToString()
            });

            PageList<GetLeaveRequestDto> pagedResult;

            if (disablePagination)
            {
                // If pagination is disabled, get all matching items
                var allItems = await projectedQuery.ToListAsync();
                // Create a PageList with all items, setting page/pageSize/totalCount appropriately
                pagedResult = new PageList<GetLeaveRequestDto>(allItems, 1, allItems.Count > 0 ? allItems.Count : 1, allItems.Count);
            }
            else
            {
                // Apply pagination as usual
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
            var result = await _dataContext.LeaveRequests
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
            var id = _tokenService.GetUserIdFromToken().Data;
            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(id));

            // Get the leave request with all necessary includes
            var leaveRequest = await _dataContext.LeaveRequests
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
                Duration = CalculateWorkingDays(leaveRequest.StartDate, leaveRequest.EndDate.AddDays(-1)), // Exclude end date
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


        public async Task<ResponseService<PageList<GetLeaveRequestDto>>> GetLeaveRequestsByUserId(
            int userId,
            int page,
            int pageSize,
            string? searchTerm,
            string? fromDate,
            string? toDate,
            string? status,
            string? type,
            bool disablePagination)
        {
            try
            {

                // 1. Build the IQueryable for LeaveRequests, filtered by UserId and including related entities
                var query = _dataContext.LeaveRequests
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
                    StartDate = x.StartDate.ToString("M/d/yyyy h:mm:ss tt"),
                    EndDate = x.EndDate.ToString("M/d/yyyy h:mm:ss tt"),
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
            var vacation = await _dataContext.LeaveRequests.FindAsync(id);
            if (vacation == null)
                return false;

            vacation.StartDate = startDate;
            vacation.EndDate = endDate;
            vacation.Reason = reason;
            vacation.Status = status;

            await _dataContext.SaveChangesAsync();
            return true;
        }

        // ✅ Delete
        public async Task<bool> DeleteVacationAsync(int id)
        {
            var vacation = await _dataContext.LeaveRequests.FindAsync(id);
            if (vacation == null)
                return false;

            _dataContext.LeaveRequests.Remove(vacation);
            await _dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<ResponseService<bool>> CancelleLeave(int id)
        {
            try
            {
                var leaveRequest = await _dataContext.LeaveRequests
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

                        // Refund leave days
                        switch (leaveRequest.Type)
                        {
                            case LeaveRequestType.Annual:
                                senderUser.Annual_leave += leaveDays;
                                break;

                            case LeaveRequestType.Emergency:
                                senderUser.Emergency_leave += leaveDays;
                                break;

                            case LeaveRequestType.Sick:
                                senderUser.Sick_leave += leaveDays;
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
                            CcEmails = new List<string> { senderUser.Email }
                        };

                        var result = await _emailService.SendEmailAsync(message);

                        if (!result.Success)
                        {
                            return new ResponseService<bool>
                            {
                                Error = true,
                                Message = "The email wasn't sent. Please contact support.",
                                Data = false
                            };
                        }
                    }

                    await _dataContext.SaveChangesAsync();

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

            var users = await _dataContext.Users
                .Where(u => rolesToNotify.Contains(u.Role))
                .ToListAsync();

            return users;
        }

        private static int CalculateWorkingDays(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                return 0;

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
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "medical-certificates");
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
