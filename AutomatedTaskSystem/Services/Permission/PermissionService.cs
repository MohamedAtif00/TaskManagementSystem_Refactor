using System.Linq;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos;
using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Hub;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.Email;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static AutomatedTaskSystem.DTO.Responses;
using static AutomatedTaskSystem.Services.Leave.LeaveRequestService;

namespace AutomatedTaskSystem.Services.Permission
{
   
    public class PermissionService : IPermissionService
    {
        private readonly DataContext _dataContext;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IHubContext<UserHub> _hubContext;
        private readonly PermissionRequestHelper _permissionRequestHelper;
        public PermissionService(DataContext dataContext, ITokenService tokenService, IEmailService emailService, IHubContext<UserHub> hubContext, PermissionRequestHelper permissionRequestHelper)
        {
            _dataContext = dataContext;
            _tokenService = tokenService;
            _emailService = emailService;
            _hubContext = hubContext;
            _permissionRequestHelper = permissionRequestHelper;
        }

        // Create a new permission request
        public async Task<ResponseService<bool>> CreatePermissionRequest(CreatePermissionDto request)
        {
            var response = new ResponseService<bool>();

            try
            {
                var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == request.UserId);
                if (user == null)
                {
                    response.Error = true;
                    response.Message = "User not found.";
                    return response;
                }

                if (!TimeOnly.TryParse(request.From, out var fromTime) ||
                    !TimeOnly.TryParse(request.To, out var toTime))
                {
                    response.Error = true;
                    response.Message = "Invalid time format.";
                    return response;
                }

                if (toTime < fromTime)
                {
                    response.Error = true;
                    response.Message = "End time cannot be earlier than start time.";
                    return response;
                }

                var pendingPermissionCount = await _dataContext.Permissions
                    .Where(p => p.UserId == user.Id && p.Status == PermissionStatusEnum.Pending)
                    .CountAsync();

                var remainingPermissionSlots = Math.Max(0, user.Permission_MAX - (user.Permission + pendingPermissionCount));

                if (remainingPermissionSlots <= 0)
                {
                    return new ResponseService<bool>
                    {
                        Error = true,
                        Message = $"Insufficient permission balance. " +
                                  $"Requested: 1 permission, " + // Each request consumes 1 permission slot
                                  $"Available: {remainingPermissionSlots} permissions",
                        Data = false
                    };
                }

                if (!DateTime.TryParse(request.PermissionDate, out var permissionDate))
                {
                    response.Error = true;
                    response.Message = "Invalid permission date format.";
                    return response;
                }

                var initialStatus = (user.Role == UserRoleEnum.Owner) ? PermissionStatusEnum.Approved : PermissionStatusEnum.Pending;

                var permission = new Models.Permission
                {
                    UserId = user.Id,
                    Type = request.Type,
                    Reason = request.Reason,
                    FromTime = fromTime,
                    ToTime = toTime,
                    PermissionDate = permissionDate,
                    CreatedAt = DateTime.Now,
                    Status = initialStatus // Set initial status based on user role
                };

                // TeamLeader logic (remains the same)
                if (user.Role == UserRoleEnum.TeamLeader)
                {
                    permission.TeamleaderId = user.Id;
                    var section = await _dataContext.Sections
                        .Where(s => s.SectionGroups.Any(g => g.GroupId == user.GroupId))
                        .FirstOrDefaultAsync();

                    if (section != null)
                        permission.SectionheadId = section.HeadId;
                }

                _dataContext.Permissions.Add(permission);
                await _dataContext.SaveChangesAsync();

                // --- Start: Refactored SignalR Notification Logic (similar to CreateLeaveRequest) ---

                // If the user is an Owner, auto-approve and notify them
                if (user.Role == UserRoleEnum.Owner)
                {
                    // Update user's permission balance (assuming 1 permission per request)
                    user.Permission += 1;
                    _dataContext.Users.Update(user);
                    await _dataContext.SaveChangesAsync();

                    // Notify the owner via SignalR that their request is approved
                    await _hubContext.Clients.User(user.Id.ToString()).SendAsync("PermissionRequestOpinion", new
                    {
                        isApproved = true,
                        message = "Your permission request has been automatically approved."
                    });
                }
                else if (user.Role == UserRoleEnum.ProjectManger)
                {
                    // Notify Owner and Project Managers about the new pending permission request
                    await _permissionRequestHelper.SendOwnerPendingUpdate(permission.Id);
                }
                else if (user.Role == UserRoleEnum.TeamLeader)
                {
                    // Notify Owner, Project Managers, and the Team Leader themselves (if they are also a TL)
                    await _permissionRequestHelper.SendOwnerPendingUpdate(permission.Id);
                    await _permissionRequestHelper.SendProjectManagersPendingUpdate(permission.Id);
                }
                else // Normal flow for non-Owner/PM/TL roles: Notify Owner/Admin and Team Leader about new pending request
                {
                    await _permissionRequestHelper.SendOwnerPendingUpdate(permission.Id);
                    await _permissionRequestHelper.SendProjectManagersPendingUpdate(permission.Id);

                    if (user.TeamleaderId.HasValue)
                    {
                        // Fetch the Team Leader's user object and send update
                        var teamLeader = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == user.TeamleaderId.Value);

                        if (teamLeader != null)
                        {
                            await _permissionRequestHelper.SendPendingUpdatesToClient(teamLeader.Id, permission.Id);
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

                // --- End: Refactored SignalR Notification Logic ---

                response.Data = true;
                response.Message = "Permission request created successfully." + (user.Role == UserRoleEnum.Owner ? " It has been automatically approved." : "");
            }
            catch (Exception ex)
            {
                response.Error = true;
                response.Message = $"An error occurred while creating the permission request: {ex.Message}";
                //_logService.LogError(ex, "An unhandled error occurred in CreatePermissionRequest for user {UserId}.", request.UserId); // Uncomment if you have a logging service
            }

            return response;
        }

        private async System.Threading.Tasks.Task NotifyNewPermissionRequest(int permissionId)
        {
            try
            {
                // Get total pending requests count
                var pendingCount = await _dataContext.Permissions
                    .CountAsync(p => p.Status == PermissionStatusEnum.Pending);

                // Notify owner
                var owner = await _dataContext.Users
                    .FirstOrDefaultAsync(x => x.Role == UserRoleEnum.Owner);

                if (owner != null)
                {
                    await _hubContext.Clients.User(owner.Id.ToString())
                        .SendAsync("UpdatePendings", new
                        {
                            pendings = pendingCount,
                            isNewRequest = true,
                            newPermissionId = permissionId
                        });
                }
                return;
            }
            catch (Exception ex)
            {
                // Log error but don't fail the main operation
                Console.WriteLine($"Error notifying new permission: {ex.Message}");
                return;
            }
        }


        // Get all permissions with optional filter by user
        public async Task<ResponseService<PageList<GetPermissionDto>>> GetAllPermissionsAsync(
            int? userId = null,
            int? role = null, // This parameter might become redundant if role is solely determined from token
            int page = 1, // Pagination parameter
            int pageSize = 10, // Pagination parameter
            string? searchTerm = null, // Search term
            string? date = null, // Filter for specific permission date
            string? status = null, // Filter for status
            string? type = null // Filter for type
)
        {
            try
            {
                var query = _dataContext.Permissions
                    .Include(p => p.User)
                    .AsQueryable();

                // 1. Get current user's ID from token
                var userIdtokenResult = _tokenService.GetUserIdFromToken();

                // Check if the user ID was successfully retrieved from the token
                if (userIdtokenResult.Data == null)
                {
                    //_logService.LogError(null, "Authentication error: Could not retrieve user ID from token in GetAllPermissionsAsync.");
                    return new ResponseService<PageList<GetPermissionDto>>
                    {
                        Error = true,
                        Message = "Authentication error: Could not retrieve user ID from token.",
                        Data = null // No data in case of error
                    };
                }

                // Convert the user ID from token to int
                int currentUserId;
                if (!int.TryParse(userIdtokenResult.Data, out currentUserId))
                {
                    //_logService.LogError(null, "Invalid user ID format from token: {UserIdData} in GetAllPermissionsAsync.", userIdtokenResult.Data);
                    return new ResponseService<PageList<GetPermissionDto>>
                    {
                        Error = true,
                        Message = "Authentication error: Invalid user ID format.",
                        Data = null
                    };
                }

                // Retrieve the current user from the database to get their role
                var currentUser = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);

                if (currentUser == null)
                {
                    //_logService.LogError(null, "Current user with ID {UserId} not found in GetAllPermissionsAsync.", currentUserId);
                    return new ResponseService<PageList<GetPermissionDto>>
                    {
                        Error = true,
                        Message = "Authentication error: Current user not found in the database.",
                        Data = null
                    };
                }

                // --- Start of Role-Based Filtering Logic (similar to GetAllVacationsAsync) ---

                // If the current user is a TeamLeader
                if (currentUser.Role == UserRoleEnum.TeamLeader)
                {
                    // Team Leaders should only see permissions of their direct team members.
                    // If a specific userId is provided in the request, check if that userId
                    // is one of the current TeamLeader's team members.
                    if (userId.HasValue)
                    {
                        // Filter by the requested userId, but only if that user is a team member of the current TL.
                        query = query.Where(p => p.UserId == userId.Value && p.User.TeamleaderId == currentUserId);
                    }
                    else
                    {
                        // If no specific userId is requested, show all permissions for current TeamLeader's team members.
                        query = query.Where(p => p.User.TeamleaderId == currentUserId);
                    }
                }

                // --- End of Role-Based Filtering Logic ---

                // 3. Apply Search Term Filtering
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(p => p.User.Name.Contains(searchTerm) ||
                                             p.Reason.Contains(searchTerm));
                }

                // 4. Apply Specific Date Filtering
                if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out DateTime parsedDate))
                {
                    query = query.Where(p => p.PermissionDate.Date == parsedDate.Date);
                }

                // 5. Apply Status Filtering
                if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse(typeof(PermissionStatusEnum), status, true, out var parsedStatus))
                {
                    query = query.Where(p => p.Status == (PermissionStatusEnum)parsedStatus);
                }

                // 6. Apply Type Filtering
                if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse(typeof(PermissionType), type, true, out var parsedType))
                {
                    query = query.Where(p => p.Type == (PermissionType)parsedType);
                }

                // --- ORDERING IS CRUCIAL FOR PAGINATION ---
                query = query.OrderByDescending(p => p.CreatedAt);

                // 7. Apply Pagination
                var pagedResult = await PageList<GetPermissionDto>.CreateAsync(
                    query.Select(p => new GetPermissionDto
                    {
                        Id = p.Id,
                        Type = p.Type.ToString(),
                        Reason = p.Reason,
                        FromTime = p.FromTime.ToString("HH:mm"),
                        ToTime = p.ToTime.ToString("HH:mm"),
                        PermissionDate = p.PermissionDate.ToString("yyyy-MM-dd"),
                        Duration = CalculateDurationInMinutes(p.FromTime, p.ToTime), // Assuming CalculateDurationInMinutes is defined
                        Status = p.Status.ToString(),
                        User = new IDName
                        {
                            Id = p.User.Id,
                            Name = p.User.Name
                        },
                        DateCreated = p.CreatedAt.ToString()
                    }),
                    page,
                    pageSize
                );

                //_logService.LogInformation("Permissions retrieved successfully for user {CurrentUserId}.", currentUserId);
                return new ResponseService<PageList<GetPermissionDto>>
                {
                    Error = false,
                    Message = "Permissions retrieved successfully.",
                    Data = pagedResult
                };
            }
            catch (Exception ex)
            {
                //_logService.LogError(ex, "An unhandled error occurred in GetAllPermissionsAsync for user {CurrentUserId}.", currentUserId); // Use currentUserId for logging
                return new ResponseService<PageList<GetPermissionDto>>
                {
                    Error = true,
                    Message = "An unexpected error occurred while retrieving permissions.",
                    Data = null
                };
            }
        }

        // Get a single permission by ID
        public async Task<ResponseService<PageList<GetPermissionDto>>> GetPermissionByIdAsync(int userId, int page, int pageSize)
        {
            try
            {
                var query = _dataContext.Permissions
                    .Where(p => p.User.Id == userId) // Filter by User.Id
                    .Include(p => p.User)
                    .Select(p => new GetPermissionDto
                    {
                        Id = p.Id,
                        Type = p.Type.ToString(),
                        Reason = p.Reason,
                        FromTime = p.FromTime.ToString("HH:mm"),
                        ToTime = p.ToTime.ToString("HH:mm"),
                        PermissionDate = p.PermissionDate.ToString("yyyy-MM-dd"),
                        Status = p.Status.ToString(),
                        Duration = CalculateDurationInMinutes(p.FromTime, p.ToTime),
                        User = new IDName
                        {
                            Id = p.User.Id,
                            Name = p.User.Name
                        },
                        DateCreated = p.CreatedAt.ToString("M/d/yyyy h:mm:ss tt")
                    });

                var paginatedPermissions = await PageList<GetPermissionDto>.CreateAsync(query, page, pageSize);

                if (paginatedPermissions.items == null || !paginatedPermissions.items.Any())
                {
                    return new ResponseService<PageList<GetPermissionDto>>
                    {
                        Error = false, // Consider if an empty list is an error or not. Often it's not.
                        Message = "No permissions found for this user.",
                        Data = paginatedPermissions
                    };
                }

                return new ResponseService<PageList<GetPermissionDto>>
                {
                    Error = false,
                    Message = "Permissions retrieved successfully.",
                    Data = paginatedPermissions
                };
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logging framework)
                return new ResponseService<PageList<GetPermissionDto>>
                {
                    Error = true,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }

        // Get a detailed permission by ID
        public async Task<ResponseService<GetSinglePermissionDto>> GetPermissionDetailByIdAsync(int permissionId)
        {
            // Get the raw data from database
            var permissions = await _dataContext.Permissions
                .Where(p => p.Id == permissionId)
                .Include(p => p.User)
                    .ThenInclude(u => u.Group)
                .Include(p => p.User)
                    .ThenInclude(u => u.Teamleader)
                        .ThenInclude(tl => tl.Group)
                .Include(x => x.Opinions)
                    .ThenInclude(x => x.User)
                .ToListAsync();

            // Project to DTO with proper calculations
            var permission = permissions.Select(p => new GetSinglePermissionDto
            {
                Id = p.Id,
                Type = p.Type.ToString(),
                Reason = p.Reason,
                FromTime = p.FromTime.ToString("HH:mm"),
                ToTime = p.ToTime.ToString("HH:mm"),
                PermissionDate = p.PermissionDate.ToString("yyyy-MM-dd"),
                Duration = CalculateDurationInMinutes(p.FromTime, p.ToTime),
                Status = p.Status.ToString(),
                DateCreated = p.CreatedAt.ToString("M/d/yyyy h:mm:ss tt"),
                Opinions = p.Opinions.Select(x => new GetOpinion { 
                    Id = x.Id,
                    PermissionId = p.Id,
                    Comment = x.Comment,
                    DateCreated = x.CreatedAt.ToString(),
                    IsApproved = x.IsApproved,
                    User = new IDNameWithRole { Id = x.User.Id,Name = x.User.Name,Role = x.User.Role}
                }).ToList(),
                User = new UserDTO
                {
                    Id = p.User.Id,
                    Name = p.User.Name,
                    Archived = p.User.Archived,
                    Role = p.User.Role,
                    GroupId = p.User.GroupId,
                    Group = p.User.Group != null
                        ? new IDName { Id = p.User.Group.Id, Name = p.User.Group.Name }
                        : null,
                    TeamleaderId = p.User.TeamleaderId,
                    Teamleader = p.User.Teamleader != null
                        ? new UserDTO
                        {
                            Id = p.User.Teamleader.Id,
                            Name = p.User.Teamleader.Name,
                            Role = p.User.Teamleader.Role,
                            GroupId = p.User.Teamleader.GroupId,
                            Group = p.User.Teamleader.Group != null
                                ? new IDName
                                {
                                    Id = p.User.Teamleader.Group.Id,
                                    Name = p.User.Teamleader.Group.Name
                                }
                                : null
                        }
                        : null,
                    HrCode = p.User.HR_code,
                    Email = p.User.Email,
                    Phone = p.User.Phone,
                    Title = p.User.Title,
                    AccountType = p.User.AccountType,
                    OnBoard = p.User.OnBoard,
                    Vacation = new VacationDto
                    {
                        Annual = p.User.Annual_leave,
                        Sick = p.User.Sick_leave,
                        Emergency = p.User.Emergency_leave,
                        Annual_MAX = p.User.Annual_leave_MAX,
                        Emergency_MAX = p.User.Emergency_leave_MAX,
                    },
                    Annual_leave = p.User.Annual_leave,
                    Annual_leave_MAX = p.User.Annual_leave_MAX,
                    Sick_leave = p.User.Sick_leave,
                    Emergency_leave = p.User.Emergency_leave,
                    Emergency_leave_MAX = p.User.Emergency_leave_MAX,
                    Permission = p.User.Permission,
                    Permission_MAX = p.User.Permission_MAX,
                }
            }).FirstOrDefault();

            return new ResponseService<GetSinglePermissionDto>
            {
                Error = false,
                Message = "Permission details retrieved successfully.",
                Data = permission
            };
        }

        // Get all permissions for a specific user
        public async Task<ResponseService<PageList<GetPermissionDto>>> GetPermissionsByUserId(
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
                // 1. Build the IQueryable for Permissions, filtered by UserId and including related entities
                var query = _dataContext.Permissions
                    .Where(p => p.User.Id == userId)
                    .Include(p => p.User)
                    .AsQueryable();

                // Apply filters conditionally
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(p =>
                        p.Reason.Contains(searchTerm) ||
                        p.User.Name.Contains(searchTerm) ||
                        p.Type.ToString().Contains(searchTerm) ||
                        p.Status.ToString().Contains(searchTerm)
                    );
                }

                if (!string.IsNullOrWhiteSpace(fromDate) && DateTime.TryParse(fromDate, out DateTime parsedFromDate))
                {
                    query = query.Where(p => p.PermissionDate >= parsedFromDate);
                }

                if (!string.IsNullOrWhiteSpace(toDate) && DateTime.TryParse(toDate, out DateTime parsedToDate))
                {
                    // Add one day to include the entire 'toDate'
                    query = query.Where(p => p.PermissionDate <= parsedToDate.AddDays(1));
                }

                if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse(typeof(PermissionStatusEnum), status, true, out var parsedStatus))
                {
                    query = query.Where(p => p.Status == (PermissionStatusEnum)parsedStatus);
                }

                if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse(typeof(PermissionType), type, true, out var parsedType))
                {
                    query = query.Where(p => p.Type == (PermissionType)parsedType);
                }

                // Project the query to GetPermissionDto after filtering
                var projectedQuery = query.Select(p => new GetPermissionDto
                {
                    Id = p.Id,
                    Type = p.Type.ToString(),
                    Reason = p.Reason,
                    FromTime = p.FromTime.ToString("HH:mm"),
                    ToTime = p.ToTime.ToString("HH:mm"),
                    PermissionDate = p.PermissionDate.ToString("yyyy-MM-dd"),
                    Status = p.Status.ToString(),
                    Duration = CalculateDurationInMinutes(p.FromTime, p.ToTime),
                    User = new IDName
                    {
                        Id = p.User.Id,
                        Name = p.User.Name
                    },
                    DateCreated = p.CreatedAt.ToString("M/d/yyyy h:mm:ss tt")
                });

                PageList<GetPermissionDto> paginatedPermissions;

                if (disablePagination)
                {
                    var allItems = await projectedQuery.ToListAsync();
                    paginatedPermissions = new PageList<GetPermissionDto>(allItems, allItems.Count, 1, allItems.Count);
                }
                else
                {
                    // Use PageList.CreateAsync to paginate the query
                    paginatedPermissions = await PageList<GetPermissionDto>.CreateAsync(projectedQuery, page, pageSize);
                }

                if (paginatedPermissions.items == null || !paginatedPermissions.items.Any())
                {
                    return new ResponseService<PageList<GetPermissionDto>>
                    {
                        Error = false,
                        Message = "No permissions found for this user with the applied filters.",
                        Data = paginatedPermissions
                    };
                }

                return new ResponseService<PageList<GetPermissionDto>>
                {
                    Error = false,
                    Message = "Permissions retrieved successfully.",
                    Data = paginatedPermissions
                };
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetPermissionsByUserId: {ex.Message}");
                return new ResponseService<PageList<GetPermissionDto>>
                {
                    Error = true,
                    Message = $"An error occurred while retrieving permissions: {ex.Message}",
                    Data = null
                };
            }
        }
        // Update a permission
        public async Task<bool> UpdatePermissionAsync(UpdatePermissionDto request)
        {
            var permission = await _dataContext.Permissions.FindAsync(request.Id);
            if (permission == null)
                return false;

            // Parse and validate times if provided
            if (!string.IsNullOrEmpty(request.FromTime) && !string.IsNullOrEmpty(request.ToTime))
            {
                if (!TimeOnly.TryParse(request.FromTime, out var fromTime) ||
                    !TimeOnly.TryParse(request.ToTime, out var toTime))
                    return false;

                if (toTime < fromTime)
                    return false;

                permission.FromTime = fromTime;
                permission.ToTime = toTime;
            }

            // Update other fields if provided
            if (!string.IsNullOrEmpty(request.PermissionDate))
            {
                if (DateTime.TryParse(request.PermissionDate, out var permissionDate))
                {
                    permission.PermissionDate = permissionDate;
                }
                else
                {
                    return false;
                }
            }

            if (request.Type.HasValue)
            {
                permission.Type = request.Type.Value;
            }

            if (!string.IsNullOrEmpty(request.Reason))
            {
                permission.Reason = request.Reason;
            }

            permission.UpdatedAt = DateTime.UtcNow;

            await _dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<ResponseService<bool>> CancelPermissionAsync(int permissionId)
        {
            try
            {
                var permission = await _dataContext.Permissions
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.Id == permissionId);

                if (permission == null)
                {
                    return new ResponseService<bool>
                    {
                        Error = true,
                        Message = "Permission not found.",
                        Data = false
                    };
                }

                var now = DateTime.Now.Date;

                if (permission.PermissionDate.Date < now )
                {
                    // Past date
                    if (permission.Status == PermissionStatusEnum.Approved || permission.Status == PermissionStatusEnum.Rejected)
                    {
                        return new ResponseService<bool>
                        {
                            Error = true,
                            Message = "You cannot cancel a permission that has already passed and was approved or rejected.",
                            Data = false
                        };
                    }

                    if (permission.Status == PermissionStatusEnum.Pending)
                    {
                        // Allow cancel but no email
                        permission.Status = PermissionStatusEnum.Cancelled;
                        permission.UpdatedAt = DateTime.UtcNow;

                        await _dataContext.SaveChangesAsync();

                        return new ResponseService<bool>
                        {
                            Error = false,
                            Message = "Pending permission cancelled (date already passed).",
                            Data = true
                        };
                    }
                }
                else
                {
                    // Future or today
                    if (permission.Status == PermissionStatusEnum.Pending || permission.Status == PermissionStatusEnum.Approved)
                    {
                        if (permission.Status == PermissionStatusEnum.Approved)
                        {
                            var senderUser = permission.User;

                            var message = new EmailMessage
                            {
                                Subject = "إلغاء طلب الإذن",
                                Body = EmailTemplate.CreatePermissionCancellationTemplate(
                                    senderUser.Name,
                                    senderUser.Email,
                                    permission.PermissionDate.ToString("yyyy-MM-dd"),
                                    permission.FromTime.ToString(@"hh\:mm"),
                                    permission.ToTime.ToString(@"hh\:mm"),
                                    permission.Type),
                                IsHtml = true,
                                CcEmails = new List<string> { senderUser.Email }
                            };

                            var emailResult = await _emailService.SendEmailAsync(message);

                            if (!emailResult.Success)
                            {
                                return new ResponseService<bool>
                                {
                                    Error = true,
                                    Message = "The permission was cancelled, but the email notification failed.",
                                    Data = false
                                };
                            }
                        }
                        permission.Status = PermissionStatusEnum.Cancelled;
                        permission.UpdatedAt = DateTime.UtcNow;

                        // Send email only if it was approved

                        await _dataContext.SaveChangesAsync();

                        return new ResponseService<bool>
                        {
                            Error = false,
                            Message = "Permission cancelled successfully.",
                            Data = true
                        };
                    }
                }

                return new ResponseService<bool>
                {
                    Error = true,
                    Message = "You cannot cancel a permission with the current status and date.",
                    Data = false
                };
            }
            catch (Exception)
            {
                return new ResponseService<bool>
                {
                    Error = true,
                    Message = "An error occurred while cancelling the permission.",
                    Data = false
                };
            }
        }




        // Delete a permission
        public async Task<bool> DeletePermissionAsync(int id)
        {
            var permission = await _dataContext.Permissions.FindAsync(id);
            if (permission == null)
                return false;

            _dataContext.Permissions.Remove(permission);
            await _dataContext.SaveChangesAsync();
            return true;
        }

        // Approve or reject a permission
        public async Task<OperationResult> ApproveOrRejectPermissionAsync(int id, bool isApproved, string? comment)
        {
            try
            {
                // Get current user from token
                var userIdResult = _tokenService.GetUserIdFromToken();
                if (userIdResult.Data == null)
                {
                    //_logService.LogError(null, "Token service returned null userId.Data in ApproveOrRejectPermissionAsync for permission ID {PermissionId}.", id);
                    //await _notificationService.ErrorNotification(userIdResult.ToString(), "Could not retrieve user ID from token.");
                    return OperationResult.Failed("Could not retrieve user ID from token.");
                }

                var user = await _dataContext.Users
                    .FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(userIdResult.Data));

                if (user == null)
                {
                    //_logService.LogError(null, "User with ID {UserId} not found when approving/rejecting permission ID {PermissionId}.", userIdResult.Data, id);
                    //await _notificationService.ErrorNotification(userIdResult.ToString(), "This user does not exist.");
                    return OperationResult.Failed("User does not exist.");
                }

                // Get permission request including opinions and the requesting user
                var permission = await _dataContext.Permissions
                    .Include(p => p.User) // Ensure the requesting user is included
                    .Include(p => p.Opinions)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (permission == null)
                {
                    //_logService.LogError(null, "Permission request with ID {PermissionId} not found for user {UserId}.", id, user.Id);
                    //await _notificationService.ErrorNotification(userIdResult.ToString(), "Permission request not found.");
                    return OperationResult.Failed("Permission request not found.");
                }

                // Prevent action on already finalized requests
                if (permission.Status != PermissionStatusEnum.Pending)
                {
                    //_logService.LogWarning("User {UserId} attempted to give opinion on non-pending permission request {PermissionId} (Status: {Status}).", user.Id, id, permission.Status);
                    //await _notificationService.ErrorNotification(userIdResult.ToString(), $"Cannot give opinion on a permission request that is already {permission.Status}.");
                    return OperationResult.Failed($"Cannot give opinion on a permission request that is already {permission.Status}.");
                }

                // Prevent duplicate opinions
                if (permission.Opinions.Any(o => o.UserId == user.Id))
                {
                    //_logService.LogWarning("User {UserId} already gave opinion on permission request {PermissionId}.", user.Id, id);
                    //await _notificationService.ErrorNotification(userIdResult.ToString(), "You have already given your opinion on this permission request.");
                    return OperationResult.Failed("You have already given your opinion on this permission request.");
                }

                // Create opinion
                var opinion = new Opinion
                {
                    UserId = user.Id,
                    PermissionId = permission.Id,
                    IsApproved = isApproved,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow,
                };

                bool statusChanged = false; // Flag to track if permission status was updated

                // Authorization and Role-based Logic
                switch (user.Role)
                {
                    case UserRoleEnum.Owner:
                        permission.Status = isApproved ? PermissionStatusEnum.Approved : PermissionStatusEnum.Rejected;
                        permission.UpdatedAt = DateTime.UtcNow;
                        statusChanged = true; // Status was definitively changed by the owner

                        // Adjust permission count if approved
                        if (isApproved)
                        {
                            permission.User.Permission += 1;
                            _dataContext.Users.Update(permission.User); // Mark user for update
                            var message = new EmailMessage
                            {
                                Subject = "طلب أذن",
                                Body = EmailTemplate.CreatePermissionTemplate(
                                                            permission.User.Name,
                                                            permission.User.Email,
                                                            permission.PermissionDate.ToString("yyyy-MM-dd"),
                                                            permission.FromTime.ToString("hh:mm tt"),
                                                            permission.ToTime.ToString("hh:mm tt"),
                                                            permission.Type),
                                IsHtml = true,
                                CcEmails = new List<string> { permission.User.Email }
                            };

                            var emailResult = await _emailService.SendEmailAsync(message);
                            if (!emailResult.Success)
                            {
                                //_logService.LogError(emailResult.Exception,
                                //                    "Error sending permission decision email for Permission ID {PermissionId} to user {RequesterUserId}: {ErrorMessage}",
                                //                    permission.Id, permission.UserId, emailResult.Message);
                                //await _notificationService.ErrorNotification(userIdResult.ToString(), "Permission request could not be fully processed due to email delivery failure. Please try again or contact support.");
                                return OperationResult.Failed($"Permission request processed but email delivery failed: {emailResult.Message}");
                            }
                        }

                        _dataContext.Permissions.Update(permission); // Mark permission for update
                        _dataContext.Opinions.Add(opinion); // Add owner's opinion

                        // Email structure remains unchanged as requested

                        // Notify the requesting user via SignalR
                        await _hubContext.Clients.User(permission.UserId.ToString()).SendAsync("PermissionRequestOpinion", new
                        {
                            isApproved = isApproved,
                            message = isApproved ? "Your permission request has been approved." : "Your permission request has been rejected."
                        });
                        break;

                    case UserRoleEnum.TeamLeader:
                    case UserRoleEnum.ProjectManger:
                    case UserRoleEnum.SectionHead: // Added SectionHead here for consistency
                        _dataContext.Opinions.Add(opinion); // Simply add the opinion
                                                            // statusChanged remains false as their opinion is not the final decision
                        break;

                    default:
                        // This case should ideally not be reached if the initial authorization check is comprehensive.
                        // However, as a safeguard, it prevents unauthorized roles from performing any action.
                        //_logService.LogWarning("Unauthorized user {UserId} with role {UserRole} attempted to approve/reject permission ID {PermissionId}. This should have been caught earlier.", user.Id, user.Role, id);
                        //await _notificationService.ErrorNotification(userIdResult.ToString(), "You are not authorized to approve or reject permissions.");
                        return OperationResult.Failed("You are not authorized to approve or reject permissions.");
                }

                await _dataContext.SaveChangesAsync(); // Save all changes after role-based logic

                // --- Centralized Pending Notification Logic ---
                await _permissionRequestHelper.SendPendingUpdatesAfterOpinion(user, permission);
                // --- End: Centralized Pending Notification Logic ---

                //_logService.LogInformation("Opinion successfully given for Permission ID {PermissionId} by user {UserId}. IsApproved: {IsApproved}", id, user.Id, isApproved);
                return OperationResult.Succeeded("Permission opinion successfully recorded.");
            }
            catch (Exception ex)
            {
                //_logService.LogError(ex, "An unhandled error occurred in ApproveOrRejectPermissionAsync for Permission ID {PermissionId} by user {UserId}.", id, user?.Id);
                //await _notificationService.ErrorNotification(userIdResult.ToString(), "An unexpected error occurred while processing your permission opinion. Please try again.");
                return OperationResult.Failed("An unexpected error occurred while processing your permission opinion. Please try again.");
            }
        }


        private static decimal CalculateDurationInHours(DateTime fromTime, DateTime toTime)
        {
            // Calculate total hours with decimal precision
            return (decimal)(toTime - fromTime).TotalHours;
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

        private static string CalculateDurationInMinutes(TimeOnly fromTime, TimeOnly toTime)
        {
            var duration = toTime - fromTime;
            int totalMinutes = (int)duration.TotalMinutes;

            // Handle negative durations (e.g., if toTime is the next day)
            if (totalMinutes < 0)
                totalMinutes += 24 * 60;

            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            return $"{hours:D2}:{minutes:D2}";
        }

    }
}
