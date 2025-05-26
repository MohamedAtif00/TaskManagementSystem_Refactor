using System.Linq;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos;
using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.Email;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static AutomatedTaskSystem.DTO.Responses;

namespace AutomatedTaskSystem.Services.Permission
{
    //public class PermissionService : IPermissionService
    //{
    //    private readonly DataContext _context;
    //    private readonly ITokenService _tokenService;

    //    public PermissionService(DataContext context, ITokenService tokenService)
    //    {
    //        _context = context;
    //        _tokenService = tokenService;
    //    }

    //    public async Task<bool> AddPermissionAsync(CreatePermissionDto permission)
    //    {
    //        try
    //        {
    //            var user = _tokenService.GetUserIdFromToken();
    //            if (user == null) return false;

    //            var userId = Convert.ToInt32(user.Data);

    //            // Define your custom month range: 21st - 20th
    //            var today = DateTime.UtcNow;
    //            DateTime periodStart, periodEnd;

    //            if (today.Day >= 21)
    //            {
    //                periodStart = new DateTime(today.Year, today.Month, 21);
    //                periodEnd = periodStart.AddMonths(1).AddDays(-1); // Until 20th of next month
    //            }
    //            else
    //            {
    //                periodEnd = new DateTime(today.Year, today.Month, 20);
    //                periodStart = periodEnd.AddMonths(-1).AddDays(1); // From 21st of previous month
    //            }

    //            // Ensure time and date values are parsable
    //            if (!TimeOnly.TryParse(permission.From, out var fromTime))
    //                return false;

    //            if (!TimeOnly.TryParse(permission.To, out var toTime))
    //                return false;

    //            if (!DateTime.TryParse(permission.Date, out var permissionDate))
    //                return false;

    //            // Limit to 2 permissions within this custom period
    //            var totalPermissions = await _context.Permissions
    //                .Where(p => p.UserId == userId &&
    //                            p.CreatedAt >= periodStart &&
    //                            p.CreatedAt <= periodEnd)
    //                .CountAsync();

    //            if (totalPermissions >= 2)
    //                return false;

    //            // Create and save the new permission
    //            var newPermission = new Models.Permission
    //            {
    //                UserId = userId,
    //                Type = permission.Type,
    //                Reason = permission.Reason,
    //                FromTime = fromTime,
    //                ToTime = toTime,
    //                PermissionDate = permissionDate,
    //                CreatedAt = DateTime.UtcNow
    //            };

    //            await _context.Permissions.AddAsync(newPermission);
    //            await _context.SaveChangesAsync();

    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            // You can add logging here
    //            return false;
    //        }
    //    }




    //    public async Task<List<GetPermissionDto>> GetAllPermissionsAsync()
    //    {
    //        return await _context.Permissions
    //            .Include(p => p.User) // optional if you need user data
    //            .OrderByDescending(p => p.CreatedAt)
    //            .Select(p => new GetPermissionDto
    //            {
    //                Id = p.Id,
    //                Type = p.Type,
    //                Reason = p.Reason,
    //                PermissionDate = p.PermissionDate,
    //                CreatedAt = p.CreatedAt,
    //                User = p.User == null ? null : new Responses.IDName
    //                {
    //                    Id = p.User.Id,
    //                    Name = p.User.Name
    //                }
    //            })
    //            .ToListAsync();
    //    }


    //    public async Task<Models.Permission?> GetPermissionByIdAsync(int id)
    //    {
    //        return await _context.Permissions
    //            .Include(p => p.User) // Optional
    //            .FirstOrDefaultAsync(p => p.Id == id);
    //    }

    //    public async Task<bool> UpdatePermissionAsync(UpdatePermissionDto dto)
    //    {
    //        try
    //        {
    //            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == dto.Id);
    //            if (permission == null) return false;

    //            permission.Type = dto.Type;
    //            permission.Reason = dto.Reason;
    //            permission.UpdatedAt = DateTime.UtcNow;

    //            _context.Permissions.Update(permission);
    //            await _context.SaveChangesAsync();
    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            // Log ex.Message
    //            return false;
    //        }
    //    }

    //    public async Task<bool> DeletePermissionAsync(int id)
    //    {
    //        try
    //        {
    //            var permission = await _context.Permissions.FindAsync(id);
    //            if (permission == null) return false;

    //            _context.Permissions.Remove(permission);
    //            await _context.SaveChangesAsync();
    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            // Log ex.Message
    //            return false;
    //        }
    //    }
    //    public async Task<ResponseService<List<GetPermissionDto>>> GetPermissionsForSameUser()
    //    {
    //        try
    //        {
    //            var userId =Convert.ToInt32( _tokenService.GetUserIdFromToken().Data);

    //            if (userId == null) return new ResponseService<List<GetPermissionDto>> { Error = true, Message = "User not found" };
    //            var permissions = await _context.Permissions
    //                .Where(p => p.UserId == Convert.ToInt32(userId))
    //                .Select(p => new GetPermissionDto
    //                {
    //                    Id = p.Id,
    //                    Type = p.Type,
    //                    Reason = p.Reason,
    //                    PermissionDate = p.PermissionDate,
    //                    CreatedAt = p.CreatedAt,
    //                    From = p.FromTime.ToString(),
    //                    To = p.ToTime.ToString()
    //                })
    //                .ToListAsync();
    //            return new ResponseService<List<GetPermissionDto>> { Error = false, Data = permissions };
    //        }
    //        catch (Exception ex)
    //        {
    //            // Log ex.Message
    //            return new ResponseService<List<GetPermissionDto>> { Error = true, Message = "Internal server error" };
    //        }
    //    }
    //}

    public class PermissionService : IPermissionService
    {
        private readonly DataContext _dataContext;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public PermissionService(DataContext dataContext, ITokenService tokenService, IEmailService emailService)
        {
            _dataContext = dataContext;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        // Create a new permission request
        public async Task<ResponseService<bool>> CreatePermissionRequest(CreatePermissionDto request)
        {
            var response = new ResponseService<bool>();

            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == request.UserId);
            if (user == null)
            {
                response.Error = true;
                response.Message = "المستخدم غير موجود.";
                return response;
            }

            // Parse and validate time ranges
            if (!TimeOnly.TryParse(request.From, out var fromTime) ||
                !TimeOnly.TryParse(request.To, out var toTime))
            {
                response.Error = true;
                response.Message = "الوقت غير صالح.";
                return response;
            }

            if (toTime < fromTime)
            {
                response.Error = true;
                response.Message = "وقت النهاية يجب أن يكون بعد وقت البداية.";
                return response;
            }

            // Check if the user has remaining permissions
            int totalPendingPermissions = await _dataContext.Permissions
                .CountAsync(p => p.UserId == user.Id && p.Status == PermissionStatusEnum.Pending);

            if ((user.Permission_MAX - user.Permission - totalPendingPermissions) < 1)
            {
                response.Error = true;
                response.Message = "لقد تجاوزت الحد الأقصى لطلبات الإذن المتاحة.";
                return response;
            }

            var permission = new Models.Permission
            {
                UserId = user.Id,
                Type = request.Type,
                Reason = request.Reason,
                FromTime = fromTime,
                ToTime = toTime,
                PermissionDate = DateTime.Parse(request.PermissionDate),
                CreatedAt = DateTime.Now,
                Status = PermissionStatusEnum.Pending
            };

            try
            {
                _dataContext.Permissions.Add(permission);
                await _dataContext.SaveChangesAsync();

                // Notify receivers
                var receivers = await GetReceiversAsync(user.Role);
                foreach (var receiver in receivers)
                {
                    Console.WriteLine($"Notify user {receiver.Id} about the new permission request.");
                    // Optional: Add SignalR or notification service here
                }

                response.Data = true;
                response.Message = "تم تقديم طلب الإذن بنجاح.";
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while creating permission request: {ex.Message}");
                response.Error = true;
                response.Message = "حدث خطأ أثناء إنشاء طلب الإذن.";
                return response;
            }
        }


        // Get all permissions with optional filter by user
        public async Task<ResponseService<List<GetPermissionDto>>> GetAllPermissionsAsync(int? userId = null, int? role = null)
        {
            var query = _dataContext.Permissions
                .Include(p => p.User)
                .AsQueryable();

            // If role is TeamLeader, show permissions of their team members
            if (role == (int)UserRoleEnum.TeamLeader && userId.HasValue)
            {
                query = query.Where(p => p.User.TeamleaderId == userId.Value);
            }
            // If userId is provided and not a TeamLeader, show only that user's permissions
            else if (userId.HasValue)
            {
                query = query.Where(p => p.UserId == userId.Value);
            }

            var result = await query
                .Select(p => new GetPermissionDto
                {
                    Id = p.Id,
                    Type = p.Type.ToString(),
                    Reason = p.Reason,
                    FromTime = p.FromTime.ToString("HH:mm"),
                    ToTime = p.ToTime.ToString("HH:mm"),
                    PermissionDate = p.PermissionDate.ToString("yyyy-MM-dd"),
                    Duration = CalculateDurationInMinutes(p.FromTime, p.ToTime),
                    Status = p.Status.ToString(),
                    User = new IDName
                    {
                        Id = p.User.Id,
                        Name = p.User.Name
                    },
                    DateCreated = p.CreatedAt.ToString()
                })
                .ToListAsync();

            return new ResponseService<List<GetPermissionDto>>
            {
                Error = false,
                Message = "Permissions retrieved successfully.",
                Data = result
            };
        }

        // Get a single permission by ID
        public async Task<ActionResult<ResponseService<GetPermissionDto>>> GetPermissionByIdAsync(int id)
        {
            var result = await _dataContext.Permissions
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
                })
                .FirstOrDefaultAsync(p => p.Id == id);

            if (result == null) return new ResponseService<GetPermissionDto>
            {
                Error = true,
                Message = "Permission not found.",
                Data = null
            };

            return new ResponseService<GetPermissionDto>
            {
                Error = false,
                Message = "Permission retrieved successfully.",
                Data = result
            };
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
        public async Task<ResponseService<List<GetPermissionDto>>> GetPermissionsByUserIdAsync(int userId)
        {
            var permissions = await _dataContext.Permissions
                .Where(p => p.UserId == userId)
                .Include(p => p.User)
                .ThenInclude(u => u.Group)
                .ToListAsync();

            var permissionDtos = permissions.Select(p => new GetPermissionDto
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
            }).ToList();

            return new ResponseService<List<GetPermissionDto>>
            {
                Error = false,
                Message = "User permissions retrieved successfully.",
                Data = permissionDtos
            };
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

                // Allow cancel if permission is scheduled in the future or was rejected
                if (permission.PermissionDate.Date > now || permission.Status == PermissionStatusEnum.Rejected)
                {
                    permission.Status = PermissionStatusEnum.Cancelled;
                    permission.UpdatedAt = DateTime.UtcNow;
                    await _dataContext.SaveChangesAsync();

                    var senderUser = permission.User;

                    var message = new EmailMessage
                    {
                        Subject = "إلغاء طلب الإذن",
                        Body = EmailTemplate.CreatePermissionCancellationTemplate(
                                    senderUser.Name,
                                    senderUser.Email,
                                    permission.PermissionDate.ToString("yyyy-MM-dd"),
                                    permission.FromTime.ToString("hh\\:mm"),
                                    permission.ToTime.ToString("hh\\:mm"),
                                    permission.Type),
                        IsHtml = true,
                        CcEmails = new List<string> { senderUser.Email }
                    };

                    // TODO: Send email using your email service

                    return new ResponseService<bool>
                    {
                        Error = false,
                        Message = "Permission cancelled successfully.",
                        Data = true
                    };
                }

                return new ResponseService<bool>
                {
                    Error = true,
                    Message = "You cannot cancel a permission that has already passed or is currently active.",
                    Data = false
                };
            }
            catch (Exception ex)
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
        public async Task<bool> ApproveOrRejectPermissionAsync(int id, bool isApproved, string comment)
        {
            // Get current user
            var userId = _tokenService.GetUserIdFromToken();
            var user = await _dataContext.Users
                .FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(userId.Data));

            if (user == null) throw new Exception("User not found");

            // Get permission request including opinions
            var permission = await _dataContext.Permissions
                .Include(p => p.User)
                .Include(p => p.Opinions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (permission == null) throw new Exception("Permission not found");

            // Authorization check
            if (user.Role != UserRoleEnum.TeamLeader &&
                user.Role != UserRoleEnum.SectionHead &&
                user.Role != UserRoleEnum.ProjectManger &&
                user.Role != UserRoleEnum.Owner)
            {
                throw new UnauthorizedAccessException("You are not authorized to approve or reject permissions");
            }

            // Prevent duplicate opinions
            if (permission.Opinions.Any(o => o.UserId == user.Id))
            {
                throw new Exception("You have already given your opinion on this permission request");
            }

            // Create opinion
            var opinion = new Opinion
            {
                UserId = user.Id,
                PermissionId = permission.Id, // Ensure you have this foreign key in Opinion
                IsApproved = isApproved,
                Comment = comment,
                CreatedAt = DateTime.UtcNow,
                User = user
            };

            _dataContext.Opinions.Add(opinion);

            // Handle Owner logic (final decision)
            if (user.Role == UserRoleEnum.Owner)
            {
                permission.Status = isApproved
                    ? PermissionStatusEnum.Approved
                    : PermissionStatusEnum.Rejected;

                // Adjust permission count if approved
                if (isApproved)
                {
                    permission.User.Permission += 1;
                }

                _dataContext.Permissions.Update(permission);

                var message = new EmailMessage
                {
                    Subject = "طلب أذن",
                    Body =EmailTemplate.CreatePermissionTemplate(permission.User.Name,
                                                                 permission.User.Email,
                                                                 permission.PermissionDate.ToString("yyyy-MM-dd"),
                                                                 permission.FromTime.ToString("hh:mm tt"),
                                                                 permission.ToTime.ToString("hh:mm tt"),
                                                                 permission.Type),
                    IsHtml = true,
                    CcEmails = new List<string> { permission.User.Email }
                };

                var result = await _emailService.SendEmailAsync(message);
                if (!result.Success)
                {
                    // Optional: Log the email failure
                }
            }

            await _dataContext.SaveChangesAsync();
            return true;
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
