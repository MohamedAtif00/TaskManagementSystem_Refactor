using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos;
using AutomatedTaskSystem.Hub;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.Email;
using AutomatedTaskSystem.Services.Log;
using AutomatedTaskSystem.Services.Notification;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using AutomatedTaskSystem.Dtos.LeaveDtos;
using static AutomatedTaskSystem.Services.Leave.LeaveRequestService;
using AutomatedTaskSystem.Helper;
using static AutomatedTaskSystem.DTO.Responses;
using AutomatedTaskSystem.Dtos.WorkFromHomeDtos;
using AutomatedTaskSystem.DTO;

namespace AutomatedTaskSystem.Services.WorkFromHome
{
    public class WorkFromHomeService : IWorkFromHomeService
    {
        private readonly DataContext _dataContext;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<UserHub> _hubContext;
        private readonly INotificationService _notificationService;
        private readonly ILogService _logService; // New logging service
        private readonly WorkFromHomeHelper _workFromHomeHelper; // New helper for WFH specific logic
        private readonly EmailRecipientSettings _emailRecipients;

        public WorkFromHomeService(
            DataContext dataContext,
            ITokenService tokenService,
            IEmailService emailService,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<UserHub> hubContext,
            INotificationService notificationService,
            ILogService logService, // Inject ILogService
            WorkFromHomeHelper workFromHomeHelper, // Inject WorkFromHomeHelper
            IOptions<EmailRecipientSettings> emailRecipientsOptions)
        {
            _dataContext = dataContext;
            _tokenService = tokenService;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
            _notificationService = notificationService;
            _logService = logService; // Assign to field
            _workFromHomeHelper = workFromHomeHelper; // Assign to field
            _emailRecipients = emailRecipientsOptions.Value;
        }

        public async Task<ResponseService<bool>> CreateWorkFromHomeRequest(CreateWorkFromHomeDto request)
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

            if (!DateTime.TryParse(request.Date, out var date))
            {
                response.Error = true;
                response.Message = "Invalid date format.";
                response.Data = false;
                return response;
            }

            // You might want to add checks for existing WFH requests on the same date for the user
            var existingWfh = await _dataContext.WorkFromHomeRequests
                                                .AnyAsync(w => w.UserId == user.Id && w.Date.Date == date.Date);
            if (existingWfh)
            {
                response.Error = true;
                response.Message = "You already have a work from home request for this date.";
                response.Data = false;
                return response;
            }

            // Check for future dated requests
            if (date.Date < DateTime.Now.Date)
            {
                response.Error = true;
                response.Message = "Cannot request work from home for a past date.";
                response.Data = false;
                return response;
            }

            // Determine initial status (similar to LeaveRequestService)
            var initialStatus = (user.Role == UserRoleEnum.Owner) ? WorkFromHomeStatusEnum.Approved : WorkFromHomeStatusEnum.Pending;

            var workFromHomeRequest = new WorkFromHomeRequest
            {
                UserId = user.Id,
                Date = date,
                NoteForManager = request.NoteForManager,
                Status = initialStatus,
                DateCreated = DateTime.Now // Already initialized in the model, but good to be explicit
            };

            // Set TeamleaderId and SectionheadId if applicable
            if (user.Role == UserRoleEnum.TeamLeader)
            {
                var section = await _dataContext.Sections
                    .Where(s => s.SectionGroups.Any(g => g.GroupId == user.GroupId))
                    .FirstOrDefaultAsync();

                if (section != null)
                    workFromHomeRequest.SectionheadId = section.HeadId;
            }
            //else if (user.TeamleaderId.HasValue) // For Members
            //{
            //    workFromHomeRequest.TeamleaderId = user.TeamleaderId.Value;
            //    var teamLeader = await _dataContext.Users.FirstOrDefaultAsync(tl => tl.Id == user.TeamleaderId.Value);
            //    if (teamLeader != null && teamLeader.SectionheadId.HasValue) // Check if team leader has a section head
            //    {
            //        workFromHomeRequest.SectionheadId = teamLeader.SectionheadId.Value;
            //    }
            //}


            try
            {
                _dataContext.WorkFromHomeRequests.Add(workFromHomeRequest);
                await _dataContext.SaveChangesAsync();

                // Auto-approval and email for Owner (similar to leave request)
                if (user.Role == UserRoleEnum.Owner)
                {
                    // No leave balance deduction for WFH
                    // You might want to send a different template or fewer recipients for Owner WFH
                    var message = new EmailMessage
                    {
                        Subject = "طلب عمل من المنزل",
                        Body = EmailTemplate.CreateWorkFromHomeApprovedTemplate(
                            user.Name,
                            user.Email,
                            workFromHomeRequest.Date.ToString("yyyy-MM-dd"),
                            workFromHomeRequest.NoteForManager,
                            user.HR_code
                        ),
                        IsHtml = true,
                        CcEmails = new List<string> { _emailRecipients.CEO, user.Email }
                    };

                    var emailResult = await _emailService.SendEmailAsync(message);

                    if (!emailResult.Success)
                    {
                        _logService.LogError(emailResult.Exception, $"Error sending auto-approval email for Owner's WorkFromHome request {workFromHomeRequest.Id}: {emailResult.Message}");
                    }

                    await _hubContext.Clients.User(user.Id.ToString()).SendAsync("WorkFromHomeOpinion", new
                    {
                        isApproved = true,
                        message = "Your work from home request has been automatically approved."
                    });
                }
                else // For other roles, send notifications to relevant managers
                {
                    await _workFromHomeHelper.SendOwnerPendingUpdate(workFromHomeRequest.Id);
                    await _workFromHomeHelper.SendProjectManagersPendingUpdate(workFromHomeRequest.Id);

                    if (workFromHomeRequest.TeamleaderId.HasValue)
                    {
                        await _workFromHomeHelper.SendTeamLeaderPendingUpdates(workFromHomeRequest.TeamleaderId.Value, workFromHomeRequest.Id);
                    }
                    if (workFromHomeRequest.SectionheadId.HasValue)
                    {
                        await _workFromHomeHelper.SendSectionHeadPendingUpdates(workFromHomeRequest.SectionheadId.Value, workFromHomeRequest.Id);
                    }
                }

                response.Data = true;
                response.Message = "Work from home request created successfully." + (user.Role == UserRoleEnum.Owner ? " It has been automatically approved." : "");
            }
            catch (Exception ex)
            {
                _logService.LogError(ex, "An error occurred while saving the work from home request: {@Request}", request);
                response.Error = true;
                response.Message = $"An error occurred while saving the work from home request: {ex.Message}";
                response.Data = false;
            }

            return response;
        }


        public async Task<OperationResult> GiveWorkFromHomeOpinion(CreateWorkFromHomeOpinionDto request)
        {
            try
            {
                var userIdResult = _tokenService.GetUserIdFromToken();
                if (userIdResult.Data == null)
                {
                    _logService.LogError(null, "Token service returned null userId.Data for WFH opinion request: {@Request}", request);
                    await _notificationService.ErrorNotification(userIdResult.ToString(), "Could not retrieve user ID from token.");
                    return OperationResult.Failed("Could not retrieve user ID from token.");
                }

                var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(userIdResult.Data));
                if (user == null)
                {
                    _logService.LogError(null, "User with ID {UserId} not found when giving opinion for WorkFromHomeId {WorkFromHomeId}.", userIdResult.Data, request.WorkFromHomeId); // Note: Assuming request.WorkFromHomeId
                    await _notificationService.ErrorNotification(userIdResult.ToString(), "This user does not exist.");
                    return OperationResult.Failed("User does not exist.");
                }

                var workFromHomeRequest = await _dataContext.WorkFromHomeRequests
                    .Include(x => x.User)
                    .Include(wfh => wfh.Opinions)
                    .FirstOrDefaultAsync(wfh => wfh.Id == request.WorkFromHomeId); // Make sure your DTO has WorkFromHomeId

                if (workFromHomeRequest == null)
                {
                    _logService.LogError(null, "Work from home request with ID {WorkFromHomeId} not found for user {UserId}.", request.WorkFromHomeId, user.Id);
                    await _notificationService.ErrorNotification(userIdResult.ToString(), "Work from home request not found.");
                    return OperationResult.Failed("Work from home request not found.");
                }

                if (workFromHomeRequest.Status == WorkFromHomeStatusEnum.Cancelled)
                {
                    _logService.LogWarning("User {UserId} attempted to give opinion on cancelled WFH request {WorkFromHomeId}.", user.Id, request.WorkFromHomeId);
                    await _notificationService.ErrorNotification(userIdResult.ToString(), "Cannot give opinion on a cancelled work from home request.");
                    return OperationResult.Failed("Cannot give opinion on a cancelled work from home request.");
                }

                if (workFromHomeRequest.Opinions.Any(o => o.UserId == user.Id))
                {
                    _logService.LogWarning("User {UserId} already gave opinion on WFH request {WorkFromHomeId}.", user.Id, request.WorkFromHomeId);
                    await _notificationService.ErrorNotification(userIdResult.ToString(), "You have already given your opinion on this request.");
                    return OperationResult.Failed("You have already given your opinion on this request.");
                }

                var opinion = new Opinion
                {
                    UserId = user.Id,
                    WorkFromHomeRequestId = request.WorkFromHomeId, // This needs to be a new property in Opinion model
                    IsApproved = request.IsApproved,
                    Comment = request.Comment,
                    CreatedAt = DateTime.UtcNow,
                };

                switch (user.Role)
                {
                    case UserRoleEnum.Owner:
                        if (request.IsApproved)
                        {
                            workFromHomeRequest.Status = WorkFromHomeStatusEnum.Approved;

                            var message = new EmailMessage
                            {
                                Subject = "طلب عمل من المنزل - موافقة",
                                Body = EmailTemplate.CreateWorkFromHomeApprovedTemplate(
                                    workFromHomeRequest.User.Name,
                                    workFromHomeRequest.User.Email,
                                    workFromHomeRequest.Date.ToString("yyyy-MM-dd"),
                                    workFromHomeRequest.NoteForManager,
                                    workFromHomeRequest.User.HR_code
                                ),
                                IsHtml = true,
                                CcEmails = new List<string> { _emailRecipients.CEO, workFromHomeRequest.User.Email }
                            };

                            var emailResult = await _emailService.SendEmailAsync(message);

                            if (!emailResult.Success)
                            {
                                _logService.LogError(emailResult.Exception, "Error sending WFH approval email for request {WorkFromHomeId}: {ErrorMessage}", workFromHomeRequest.Id, emailResult.Message);
                                await _notificationService.ErrorNotification(userIdResult.ToString(), "Work from home request could not be fully processed due to email delivery failure. Please try again or contact support.");
                                return OperationResult.Failed($"Work from home request could not be fully processed due to email delivery failure: {emailResult.Message}");
                            }

                            await _hubContext.Clients.User(workFromHomeRequest.UserId.ToString()).SendAsync("WorkFromHomeOpinion", new
                            {
                                isApproved = true,
                                message = "Your work from home request has been approved."
                            });
                        }
                        else // Owner rejects
                        {
                            workFromHomeRequest.Status = WorkFromHomeStatusEnum.Rejected;
                            await _hubContext.Clients.User(workFromHomeRequest.UserId.ToString()).SendAsync("WorkFromHomeOpinion", new
                            {
                                isApproved = false,
                                message = "Your work from home request has been rejected."
                            });
                        }
                        break;

                    case UserRoleEnum.TeamLeader:
                    case UserRoleEnum.ProjectManger:
                    case UserRoleEnum.SectionHead: // Section Head can also give opinion
                        // No direct status change here, just add opinion
                        break;

                    default:
                        _logService.LogWarning("Unauthorized user {UserId} with role {UserRole} attempted to give opinion on WFH request {WorkFromHomeId}.", user.Id, user.Role, request.WorkFromHomeId);
                        await _notificationService.ErrorNotification(userIdResult.ToString(), "You are not authorized to give opinions on this work from home request.");
                        return OperationResult.Failed("You are not authorized to give opinions on this work from home request.");
                }

                _dataContext.Opinions.Add(opinion);
                _dataContext.WorkFromHomeRequests.Update(workFromHomeRequest);
                await _dataContext.SaveChangesAsync();

                // Send updates based on the current state of opinions (similar to leave request helper)
                await _workFromHomeHelper.SendPendingUpdatesAfterOpinion(user, workFromHomeRequest);

                _logService.LogInformation("Opinion successfully given for WorkFromHomeRequest {WorkFromHomeId} by user {UserId}. IsApproved: {IsApproved}", request.WorkFromHomeId, user.Id, request.IsApproved);
                return OperationResult.Succeeded("Opinion successfully recorded.");
            }
            catch (Exception ex)
            {
                _logService.LogError(ex, "An unhandled error occurred in GiveWorkFromHomeOpinion for WorkFromHomeId {WorkFromHomeId} by user {UserId}.", request.WorkFromHomeId);
                return OperationResult.Failed("An unexpected error occurred while processing your opinion. Please try again.");
            }
        }

        public async Task<ResponseService<PageList<GetWorkFromHomeDto>>> GetAllWorkFromHomeRequestsAsync(
           int page = 1,
           int pageSize = 10,
           string? searchTerm = null,
           string? fromDate = null,
           string? toDate = null,
           string? status = null,
           string? myStatus = null,
           bool disablePagination = false)
        {
            var tokenResponse = _tokenService.GetUserIdFromToken();
            if (tokenResponse == null || tokenResponse.Error || string.IsNullOrWhiteSpace(tokenResponse.Data))
            {
                return new ResponseService<PageList<GetWorkFromHomeDto>>
                {
                    Error = true,
                    Message = tokenResponse?.Message ?? "Failed to retrieve user ID from token.",
                    Data = null
                };
            }

            if (!int.TryParse(tokenResponse.Data, out var currentUserId))
            {
                return new ResponseService<PageList<GetWorkFromHomeDto>>
                {
                    Error = true,
                    Message = "Invalid user ID format in token.",
                    Data = null
                };
            }

            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);
            if (user == null)
            {
                return new ResponseService<PageList<GetWorkFromHomeDto>>
                {
                    Error = true,
                    Message = "User associated with the token not found.",
                    Data = null
                };
            }

            var query = _dataContext.WorkFromHomeRequests
                .Include(wfh => wfh.User)
                .Include(wfh => wfh.Opinions) // Include opinions for MyStatus calculation
                .AsQueryable();

            // Role-Based Filtering (similar to LeaveRequestService)
            if (user.Role == UserRoleEnum.TeamLeader)
            {
                query = query.Where(wfh => wfh.User.TeamleaderId == currentUserId);
            }
            else if (user.Role == UserRoleEnum.Member)
            {
                query = query.Where(wfh => wfh.UserId == currentUserId);
            }
            // Add logic for ProjectManager, SectionHead if they have broader WFH access

            // Apply Search Term Filtering
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(wfh => wfh.User.Name.Contains(searchTerm) ||
                                           wfh.NoteForManager.Contains(searchTerm));
            }

            // Apply Date Range Filtering (for a single date, so StartDate and EndDate will be the same)
            if (!string.IsNullOrWhiteSpace(fromDate) && DateTime.TryParse(fromDate, out DateTime parsedFromDate))
            {
                query = query.Where(wfh => wfh.Date.Date >= parsedFromDate.Date);
            }

            if (!string.IsNullOrWhiteSpace(toDate) && DateTime.TryParse(toDate, out DateTime parsedToDate))
            {
                query = query.Where(wfh => wfh.Date.Date <= parsedToDate.Date);
            }

            // Apply Status Filtering (for the overall request status)
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse(typeof(WorkFromHomeStatusEnum), status, true, out var parsedStatus))
            {
                query = query.Where(wfh => wfh.Status == (WorkFromHomeStatusEnum)parsedStatus);
            }

            // Apply MyStatus Filtering (based on current user's opinion status)
            if (!string.IsNullOrWhiteSpace(myStatus))
            {
                var normalizedMyStatusFilter = myStatus.ToLowerInvariant();

                switch (normalizedMyStatusFilter)
                {
                    case "approved":
                        query = query.Where(x =>
                            (x.Opinions.Any(o => o.UserId == currentUserId && o.IsApproved)) ||
                            (!x.Opinions.Any(o => o.UserId == currentUserId) && x.Status == WorkFromHomeStatusEnum.Approved)
                        );
                        break;
                    case "rejected":
                        query = query.Where(x =>
                            (x.Opinions.Any(o => o.UserId == currentUserId && !o.IsApproved)) ||
                            (!x.Opinions.Any(o => o.UserId == currentUserId) && x.Status == WorkFromHomeStatusEnum.Rejected)
                        );
                        break;
                    case "pending":
                        query = query.Where(x => !x.Opinions.Any(o => o.UserId == currentUserId) && x.Status == WorkFromHomeStatusEnum.Pending);
                        break;
                    case "cancelled":
                        query = query.Where(x => !x.Opinions.Any(o => o.UserId == currentUserId) && x.Status == WorkFromHomeStatusEnum.Cancelled);
                        break;
                    default:
                        break;
                }
            }

            query = query.OrderByDescending(wfh => wfh.DateCreated);

            var projectedQuery = query.Select(x => new GetWorkFromHomeDto
            {
                Id = x.Id,
                Date = x.Date.ToString("yyyy-MM-dd"),
                NoteForManager = x.NoteForManager,
                Status = x.Status.ToString(),
                MyStatus = x.Opinions.Any(o => o.UserId == currentUserId)
                                ? (x.Opinions.FirstOrDefault(o => o.UserId == currentUserId).IsApproved
                                    ? WorkFromHomeStatusEnum.Approved.ToString()
                                    : WorkFromHomeStatusEnum.Rejected.ToString())
                                : (x.Status == WorkFromHomeStatusEnum.Pending
                                    ? WorkFromHomeStatusEnum.Pending.ToString()
                                    : x.Status.ToString()),
                User = new IDName
                {
                    Id = x.User.Id,
                    Name = x.User.Name
                },
                DateCreated = x.DateCreated.HasValue ? x.DateCreated.Value.ToString("yyyy-MM-dd") : null
            });

            PageList<GetWorkFromHomeDto> pagedResult;

            if (disablePagination)
            {
                var allItems = await projectedQuery.ToListAsync();
                pagedResult = new PageList<GetWorkFromHomeDto>(allItems, 1, allItems.Count > 0 ? allItems.Count : 1, allItems.Count);
            }
            else
            {
                pagedResult = await PageList<GetWorkFromHomeDto>.CreateAsync(
                    projectedQuery,
                    page,
                    pageSize
                );
            }

            return new ResponseService<PageList<GetWorkFromHomeDto>>
            {
                Error = false,
                Message = "Work from home requests retrieved successfully.",
                Data = pagedResult
            };
        }

        public async Task<ResponseService<GetSingleWorkFromHomeDto>> GetWorkFromHomeRequestByIdAsync(int id)
        {
            var tokenResponse = _tokenService.GetUserIdFromToken();
            if (tokenResponse.Data == null)
            {
                return new ResponseService<GetSingleWorkFromHomeDto>
                {
                    Error = true,
                    Message = "Could not retrieve user ID from token.",
                    Data = null
                };
            }
            if (!int.TryParse(tokenResponse.Data, out var currentUserId))
            {
                return new ResponseService<GetSingleWorkFromHomeDto>
                {
                    Error = true,
                    Message = "Invalid user ID format in token.",
                    Data = null
                };
            }

            var request = await _dataContext.WorkFromHomeRequests
                .Where(x => x.Id == id)
                .Include(x => x.User)
                    .ThenInclude(u => u.Group)
                .Include(x => x.User)
                    .ThenInclude(u => u.Teamleader)
                        .ThenInclude(tl => tl.Group)
                .Include(x => x.Opinions)
                    .ThenInclude(o => o.User)
                .FirstOrDefaultAsync();

            if (request == null)
            {
                return new ResponseService<GetSingleWorkFromHomeDto>
                {
                    Error = true,
                    Message = "Work from home request not found.",
                    Data = null
                };
            }

            // Determine if the current user is an owner to show NoteForManager
            var currentUser = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            bool isOwner = currentUser?.Role == UserRoleEnum.Owner;

            var requestDetails = new GetSingleWorkFromHomeDto
            {
                Id = request.Id,
                Date = request.Date.ToString("M/d/yyyy h:mm:ss tt"),
                NoteForManager = isOwner ? request.NoteForManager : null, // Only show to Owner
                Status = request.Status.ToString(),
                User = new UserDTO
                {
                    Id = request.User.Id,
                    Name = request.User.Name,
                    Archived = request.User.Archived,
                    Role = request.User.Role,
                    GroupId = request.User.GroupId,
                    Group = request.User.Group != null
                        ? new IDName { Id = request.User.Group.Id, Name = request.User.Group.Name }
                        : null,
                    TeamleaderId = request.User.TeamleaderId,
                    Teamleader = request.User.Teamleader != null
                        ? new UserDTO
                        {
                            Id = request.User.Teamleader.Id,
                            Name = request.User.Teamleader.Name,
                            Role = request.User.Teamleader.Role,
                            GroupId = request.User.Teamleader.GroupId,
                            Group = request.User.Teamleader.Group != null
                                ? new IDName
                                {
                                    Id = request.User.Teamleader.Group.Id,
                                    Name = request.User.Teamleader.Group.Name
                                }
                                : null
                        }
                        : null,
                    HrCode = request.User.HR_code,
                    Email = request.User.Email,
                    Phone = request.User.Phone,
                    Title = request.User.Title,
                    AccountType = request.User.AccountType,
                    Annual_leave = request.User.Annual_leave, // Still include these for user's full profile
                    Annual_leave_MAX = request.User.Annual_leave_MAX,
                    Sick_leave = request.User.Sick_leave,
                    Emergency_leave = request.User.Emergency_leave,
                    Emergency_leave_MAX = request.User.Emergency_leave_MAX,
                    Permission = request.User.Permission,
                    Permission_MAX = request.User.Permission_MAX,
                    Vacation = new VacationDto
                    {
                        Annual = request.User.Annual_leave,
                        Sick = request.User.Sick_leave,
                        Emergency = request.User.Emergency_leave,
                        Annual_MAX = request.User.Annual_leave_MAX,
                        Emergency_MAX = request.User.Emergency_leave_MAX,
                    },
                    OnBoard = request.User.OnBoard
                },
                Opinions = request.Opinions.Select(o => new GetOpinion
                {
                    Id = o.Id,
                    WorkFromHomeRequestId = o.WorkFromHomeRequestId, // Make sure GetOpinion includes this
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

            return new ResponseService<GetSingleWorkFromHomeDto>
            {
                Error = false,
                Message = "Work from home request retrieved successfully.",
                Data = requestDetails
            };
        }

        public async Task<ResponseService<PageList<GetWorkFromHomeDto>>> GetWorkFromHomeRequestsByUserId(
           int userId,
           int page,
           int pageSize,
           string? searchTerm,
           string? fromDate,
           string? toDate,
           string? status,
           bool disablePagination)
        {
            try
            {
                var query = _dataContext.WorkFromHomeRequests
                    .Where(x => x.UserId == userId)
                    .Include(x => x.User)
                    .ThenInclude(x => x.Group)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(x =>
                        x.NoteForManager.Contains(searchTerm) ||
                        x.User.Name.Contains(searchTerm) ||
                        x.Status.ToString().Contains(searchTerm)
                    );
                }

                if (!string.IsNullOrWhiteSpace(fromDate) && DateTime.TryParse(fromDate, out DateTime parsedFromDate))
                {
                    query = query.Where(x => x.Date.Date >= parsedFromDate.Date);
                }

                if (!string.IsNullOrWhiteSpace(toDate) && DateTime.TryParse(toDate, out DateTime parsedToDate))
                {
                    query = query.Where(x => x.Date.Date <= parsedToDate.Date);
                }

                if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse(typeof(WorkFromHomeStatusEnum), status, true, out var parsedStatus))
                {
                    query = query.Where(x => x.Status == (WorkFromHomeStatusEnum)parsedStatus);
                }

                var projectedQuery = query.Select(x => new GetWorkFromHomeDto
                {
                    Id = x.Id,
                    Date = x.Date.ToString("yyyy-MM-dd"),
                    NoteForManager = x.NoteForManager,
                    Status = x.Status.ToString(),
                    User = new IDName
                    {
                        Id = x.User.Id,
                        Name = x.User.Name
                    },
                    DateCreated = x.DateCreated.HasValue ? x.DateCreated.Value.ToString("M/d/yyyy h:mm:ss tt") : null,
                    // MyStatus will be handled at the client side if not directly filtered by user's opinion
                    // For this method, we are fetching for a specific user, so their 'MyStatus' isn't about their opinion, but the request's status relative to them.
                    MyStatus = x.Status.ToString() // For a specific user, their 'MyStatus' is usually just the request's actual status
                });

                PageList<GetWorkFromHomeDto> paginatedWorkFromHomeRequests;

                if (disablePagination)
                {
                    var allItems = await projectedQuery.ToListAsync();
                    paginatedWorkFromHomeRequests = new PageList<GetWorkFromHomeDto>(allItems, allItems.Count, 1, allItems.Count);
                }
                else
                {
                    paginatedWorkFromHomeRequests = await PageList<GetWorkFromHomeDto>.CreateAsync(projectedQuery, page, pageSize);
                }

                if (paginatedWorkFromHomeRequests.items == null || !paginatedWorkFromHomeRequests.items.Any())
                {
                    return new ResponseService<PageList<GetWorkFromHomeDto>>
                    {
                        Error = false,
                        Message = "No work from home requests found for this user with the applied filters.",
                        Data = paginatedWorkFromHomeRequests
                    };
                }

                return new ResponseService<PageList<GetWorkFromHomeDto>>
                {
                    Error = false,
                    Message = "Work from home requests retrieved successfully.",
                    Data = paginatedWorkFromHomeRequests
                };
            }
            catch (Exception ex)
            {
                _logService.LogError(ex, $"An error occurred in GetWorkFromHomeRequestsByUserId for user ID {userId}.");
                return new ResponseService<PageList<GetWorkFromHomeDto>>
                {
                    Error = true,
                    Message = $"An error occurred while retrieving work from home requests: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ResponseService<bool>> CancelWorkFromHome(int id)
        {
            try
            {
                var workFromHomeRequest = await _dataContext.WorkFromHomeRequests
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (workFromHomeRequest == null)
                    return new ResponseService<bool> { Error = true, Message = "Work from home request not found.", Data = false };

                var now = DateTime.Now;

                // Allow cancellation if pending or approved and date is in the future
                if ((workFromHomeRequest.Status == WorkFromHomeStatusEnum.Pending) ||
                    (workFromHomeRequest.Status == WorkFromHomeStatusEnum.Approved && workFromHomeRequest.Date.Date >= now.Date))
                {
                    var senderUser = workFromHomeRequest.User;
                    bool wasApproved = workFromHomeRequest.Status == WorkFromHomeStatusEnum.Approved;

                    workFromHomeRequest.Status = WorkFromHomeStatusEnum.Cancelled;

                    // No leave balance refund for WFH
                    // If it was approved, send cancellation email
                    if (wasApproved)
                    {
                        var message = new EmailMessage
                        {
                            Subject = "إلغاء طلب عمل من المنزل",
                            Body = EmailTemplate.CreateWorkFromHomeCancellationTemplate( // You'll need a new template
                                senderUser.Name,
                                senderUser.Email,
                                workFromHomeRequest.Date.ToString("yyyy-MM-dd"),
                                senderUser.HR_code),
                            IsHtml = true,
                            CcEmails = new List<string>()
                        };

                        if (senderUser.Role == UserRoleEnum.Owner)
                        {
                            if (!string.IsNullOrEmpty(_emailRecipients.CEO))
                            {
                                message.CcEmails.Add(_emailRecipients.CEO);
                            }
                        }
                        message.CcEmails.Add(senderUser.Email); // Always CC the requester

                        if (!string.IsNullOrEmpty(_emailRecipients.SectionHead))
                        {
                            message.CcEmails.Add(_emailRecipients.SectionHead);
                        }

                        var result = await _emailService.SendEmailAsync(message);

                        if (!result.Success)
                        {
                            _logService.LogError(result.Exception, $"Error sending WFH cancellation email for request {id}. Message: {result.Message}");
                            return new ResponseService<bool>
                            {
                                Error = true,
                                Message = "The cancellation email wasn't sent. Please contact support.",
                                Data = false
                            };
                        }
                    }

                    await _dataContext.SaveChangesAsync();

                    // Send SignalR updates to relevant parties
                    if (senderUser.Role != UserRoleEnum.Owner)
                    {
                        await _workFromHomeHelper.SendOwnerPendingUpdate();
                        if (senderUser.Role != UserRoleEnum.ProjectManger)
                            await _workFromHomeHelper.SendProjectManagersPendingUpdate();

                        if (senderUser.TeamleaderId != null)
                        {
                            await _workFromHomeHelper.SendTeamLeaderPendingUpdates(senderUser.TeamleaderId.Value);
                        }
                        //if (senderUser.SectionheadId != null) // If section head is directly assigned to user
                        //{
                        //    await _workFromHomeHelper.SendSectionHeadPendingUpdates(senderUser.SectionheadId.Value);
                        //}
                        // Also notify the section head who might have approved it if the user is a team leader
                        //var section = await _dataContext.Sections
                        //                .Where(s => s.SectionGroups.Any(g => g.GroupId == senderUser.GroupId))
                        //                .FirstOrDefaultAsync();
                        //if (section != null && section.HeadId.HasValue)
                        //{
                        //    await _workFromHomeHelper.SendSectionHeadPendingUpdates(section.HeadId.Value);
                        //}
                    }

                    return new ResponseService<bool>
                    {
                        Error = false,
                        Message = "Work from home request cancelled successfully.",
                        Data = true
                    };
                }

                return new ResponseService<bool>
                {
                    Error = true,
                    Message = "You cannot cancel a work from home request that has already passed or is currently active.",
                    Data = false
                };
            }
            catch (Exception ex)
            {
                _logService.LogError(ex, $"An unhandled error occurred while cancelling work from home request with ID {id}.");
                return new ResponseService<bool> { Error = true, Message = "An error occurred.", Data = false };
            }
        }


    }
}