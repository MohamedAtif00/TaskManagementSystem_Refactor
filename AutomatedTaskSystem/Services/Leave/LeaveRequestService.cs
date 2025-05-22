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
using static AutomatedTaskSystem.DTO.Responses;

namespace AutomatedTaskSystem.Services.Leave
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly DataContext _dataContext;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public LeaveRequestService(DataContext dataContext, ITokenService tokenService, IEmailService emailService)
        {
            _dataContext = dataContext;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        // ✅ Create
        public async Task<bool> CreateLeaveRequest(CreateLeaveRequestDto request)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == request.UserId);
            if (user == null) return false;

            // Parse and validate dates
            if (!DateTime.TryParse(request.StartDate, out var startDate) ||
                !DateTime.TryParse(request.EndDate, out var endDate))
                return false;

            if (endDate < startDate)
                return false;

            //if(await _dataContext.LeaveRequests.AnyAsync(x => x.UserId == user.Id && x.Status == LeaveRequestStatusEnum.Pending))
            //    return false;

            // Calculate leave days (inclusive)
            int requestedDays = (endDate - startDate).Days + 1;

            if ((user.Annual_leave_MAX - user.Annual_leave) < requestedDays)
                return false;

            var leaveRequest = new LeaveRequest
            {
                UserId = user.Id,
                StartDate = startDate,
                EndDate = endDate,
                Reason = request.Reason,
                Status = Models.LeaveRequestStatusEnum.Pending,
                Type = request.type,
                NoteForManager = request.NoteForManager,
                DateCreated = DateTime.Now
            };

            // TeamLeader logic
            if (user.Role == UserRoleEnum.TeamLeader)
            {
                leaveRequest.TeamleaderId = user.Id;

                // Assign SectionHeadId based on user's group
                var section = await _dataContext.Sections
                    .Where(s => s.SectionGroups.Any(g => g.GroupId == user.GroupId))
                    .FirstOrDefaultAsync();

                if (section != null)
                    leaveRequest.SectionheadId = section.HeadId;
            }

            _dataContext.LeaveRequests.Add(leaveRequest);

            // Deduct leave days
            //user.Annual_leave -= requestedDays;
            //_dataContext.Users.Update(user);

            // Notify receivers
            var receivers = await GetReceiversAsync(user.Role);
            foreach (var receiver in receivers)
            {
                Console.WriteLine($"Notify user {receiver.Id} about the new vacation request.");
                // Optional: implement actual notification (e.g., SignalR)
            }

            await _dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> GiveOpinion(CreateOpinionDto request)
        {
            // Get current user
            var userId = _tokenService.GetUserIdFromToken();
            var user = await _dataContext.Users
                .FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(userId.Data));

            if (user == null) throw new Exception("User not found");

            // Get the leave request with existing opinions
            var leaveRequest = await _dataContext.LeaveRequests
                .Include(x => x.User)
                .Include(lr => lr.Opinions)
                .FirstOrDefaultAsync(lr => lr.Id == request.LeaveRequestId);

            if (leaveRequest == null) throw new Exception("Leave request not found");

            // Check if user already gave opinion on this request
            if (leaveRequest.Opinions.Any(o => o.UserId == user.Id))
            {
                throw new Exception("You have already given your opinion on this request");
            }

            // Create new opinion
            var opinion = new Opinion
            {
                UserId = user.Id,
                LeaveRequestId = request.LeaveRequestId,
                IsApproved = request.IsApproved,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
            };

            _dataContext.Opinions.Add(opinion);

            // Handle different roles
            switch (user.Role)
            {
                case UserRoleEnum.Owner:
                    // Owner's decision is final
                    leaveRequest.Status = request.IsApproved
                        ? LeaveRequestStatusEnum.Approved
                        : LeaveRequestStatusEnum.Rejected;
                    var message = new EmailMessage { Subject = "أجازة", Body = "body", IsHtml = true,CcEmails = new List<string> { leaveRequest.User.Email} };
                    var result = await _emailService.SendEmailAsync(message);

                    if (result.Success)
                    { 
                    
                        _dataContext.Opinions.Add(opinion);
                        _dataContext.LeaveRequests.Update(leaveRequest);
                    }
                    break;

                case UserRoleEnum.TeamLeader:
                case UserRoleEnum.ProjectManger:
                    _dataContext.Opinions.Add(opinion);
                    // Just record the opinion, don't change status
                    // Optional: You could add logic to check if all required TL/PMs have approved
                    break;

                default:
                    throw new UnauthorizedAccessException("You are not authorized to give opinions on leave requests");
            }

            await _dataContext.SaveChangesAsync();
            return true;
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

        // ✅ Read All (optionally filter by user)
        public async Task<ResponseService<List<GetLeaveRequestDto>>> GetAllVacationsAsync(int? userId = null, int? role = null)
        {
            var query = _dataContext.LeaveRequests
                .Include(v => v.User)
                .AsQueryable();

            // If role is TeamLeader (e.g., 2), show leave requests of their team members
            if (role == (int)UserRoleEnum.TeamLeader && userId.HasValue)
            {
                query = query.Where(v => v.User.TeamleaderId == userId.Value);
            }
            // If role is Coordinator (e.g., 1), show all users – do nothing
            //else (role == (int)UserRoleEnum.ProjectManger && userId.HasValue)
            //else
            //{
            //    // If normal user, show only their own leave requests
            //    query = query.Where(v => v.UserId == userId.Value);
            //}

            var result = await query
                .Select(x => new GetLeaveRequestDto
                {
                    Id = x.Id,
                    StartDate = x.StartDate.ToString("yyyy-MM-dd"),
                    EndDate = x.EndDate.ToString("yyyy-MM-dd"),
                    Reason = x.Reason,
                    Status = x.Status.ToString(),
                    Duration = CalculateWorkingDays(x.StartDate,x.EndDate),
                    Type = x.Type.ToString(),
                    user = new IDName
                    {
                        Id = x.User.Id,
                        Name = x.User.Name
                    },
                    DateCreated = x.DateCreated.ToString()
                })
                .ToListAsync();

            return new ResponseService<List<GetLeaveRequestDto>>
            {
                Error = false,
                Message = "Vacations retrieved successfully.",
                Data = result
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


        public async Task<ResponseService<List<GetLeaveRequestDto>>> GetLeaveRequestsByUserId(int userId)
        {
            var leaveRequests = await _dataContext.LeaveRequests
                .Where(x => x.UserId == userId)
                .Include(x => x.User)
                .ThenInclude(x => x.Group)
                .ToListAsync();

            var vacations = leaveRequests.Select(x => new GetLeaveRequestDto
            {
                Id = x.Id,
                StartDate = x.StartDate.ToString("M/d/yyyy h:mm:ss tt"),
                EndDate = x.EndDate.ToString("M/d/yyyy h:mm:ss tt"),
                Duration = CalculateWorkingDays(x.StartDate, x.EndDate.AddDays(-1)),
                Reason = x.Reason,
                Status = x.Status.ToString(),
                Type = x.Type.ToString(),
                DateCreated = x.DateCreated?.ToString("M/d/yyyy h:mm:ss tt"),
                user = new IDName
                {
                    Id = x.User.Id,
                    Name = x.User.Name
                }
            }).ToList();


            return new ResponseService<List<GetLeaveRequestDto>>
            {
                Error = false,
                Message = "Leave requests retrieved successfully.",
                Data = vacations
            };
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
            int workingDays = 0;
            DateTime date = startDate;

            while (date <= endDate)
            {
                if (date.DayOfWeek != DayOfWeek.Saturday &&
                    date.DayOfWeek != DayOfWeek.Friday)
                {
                    workingDays++;
                }
                date = date.AddDays(1);
            }

            return workingDays;
        }
    }
}
