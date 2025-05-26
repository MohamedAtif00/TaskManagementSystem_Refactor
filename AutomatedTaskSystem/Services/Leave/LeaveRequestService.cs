using System.Net.Mail;
using AutomatedTaskSystem.Data;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LeaveRequestService(DataContext dataContext, ITokenService tokenService, IEmailService emailService, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _tokenService = tokenService;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
        }

        // ✅ Create
        //public async Task<bool> CreateLeaveRequest(CreateLeaveRequestDto request)
        //{
        //    var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == request.UserId);
        //    if (user == null) return false;

        //    // Parse and validate dates
        //    if (!DateTime.TryParse(request.StartDate, out var startDate) ||
        //        !DateTime.TryParse(request.EndDate, out var endDate))
        //        return false;

        //    if (endDate < startDate)
        //        return false;

        //    // Calculate leave days (inclusive)
        //    int requestedDays = (endDate - startDate).Days + 1;

        //    if ((user.Annual_leave_MAX - user.Annual_leave) < requestedDays)
        //        return false;

        //    var leaveRequest = new LeaveRequest
        //    {
        //        UserId = user.Id,
        //        StartDate = startDate,
        //        EndDate = endDate,
        //        Reason = request.Reason,
        //        Status = Models.LeaveRequestStatusEnum.Pending,
        //        Type = request.type,
        //        NoteForManager = request.NoteForManager,
        //        DateCreated = DateTime.Now
        //    };

        //    // TeamLeader logic
        //    if (user.Role == UserRoleEnum.TeamLeader)
        //    {
        //        leaveRequest.TeamleaderId = user.Id;

        //        // Assign SectionHeadId based on user's group
        //        var section = await _dataContext.Sections
        //            .Where(s => s.SectionGroups.Any(g => g.GroupId == user.GroupId))
        //            .FirstOrDefaultAsync();

        //        if (section != null)
        //            leaveRequest.SectionheadId = section.HeadId;
        //    }

        //    _dataContext.LeaveRequests.Add(leaveRequest);

        //    // Notify receivers
        //    var receivers = await GetReceiversAsync(user.Role);
        //    foreach (var receiver in receivers)
        //    {
        //        Console.WriteLine($"Notify user {receiver.Id} about the new vacation request.");
        //        // Optional: implement actual notification (e.g., SignalR)
        //    }

        //    await _dataContext.SaveChangesAsync();
        //    return true;
        //}

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

            int requestedDays = CalculateWorkingDays(startDate,endDate);

            if (request.type == LeaveRequestType.Annual || request.type == LeaveRequestType.Emergency)
            {
                var pendingLeaveDays = await _dataContext.LeaveRequests
                    .Where(lr => lr.UserId == user.Id &&
                                 (lr.Type == LeaveRequestType.Annual || lr.Type == LeaveRequestType.Emergency) &&
                                 lr.Status == LeaveRequestStatusEnum.Pending)
                    .Select(lr => EF.Functions.DateDiffDay(lr.StartDate, lr.EndDate) + 1)
                    .SumAsync();

                int remainingLeave = user.Annual_leave_MAX - user.Annual_leave;

                if ((pendingLeaveDays + requestedDays) > remainingLeave)
                {
                    response.Error = true;
                    response.Message = "Requested leave exceeds available leave balance.";
                    response.Data = false;
                    return response;
                }
            }

            if (request.type == LeaveRequestType.Sick)
            {
                if (requestedDays > 3 && request.MedicalCertificate == null)
                {
                    response.Error = true;
                    response.Message = "Medical certificate is required for sick leave longer than 3 days.";
                    response.Data = false;
                    return response;
                }

                if (request.MedicalCertificate != null && !IsValidMedicalCertificate(request.MedicalCertificate))
                {
                    response.Error = true;
                    response.Message = "Invalid medical certificate file.";
                    response.Data = false;
                    return response;
                }
            }

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
                    var medicalCertPath = await SaveMedicalCertificate(request.MedicalCertificate, leaveRequest.Id);
                    leaveRequest.MedicalCertificatePath = medicalCertPath;
                    leaveRequest.MedicalCertificateFileName = request.MedicalCertificate.FileName;

                    _dataContext.LeaveRequests.Update(leaveRequest);
                    await _dataContext.SaveChangesAsync();
                }

                response.Data = true;
                response.Message = "Leave request created successfully.";
            }
            catch (Exception ex)
            {
                response.Error = true;
                response.Message = $"An error occurred while saving the leave request: {ex.Message}";
                response.Data = false;
            }

            return response;
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

                    if (leaveRequest.Status == LeaveRequestStatusEnum.Approved)
                    {
                        var senderUser = leaveRequest.User;
                        var message = new EmailMessage
                        {
                            Subject = "أجازة",
                            Body = EmailTemplate.CreateTemplate(senderUser.Name,
                                                                senderUser.Email,
                                                                leaveRequest.StartDate.ToString("yyyy-MM-dd"),
                                                                leaveRequest.EndDate.ToString("yyyy-MM-dd"),
                                                                CalculateWorkingDays(leaveRequest.StartDate,leaveRequest.EndDate),
                                                                leaveRequest.Type,
                                                                leaveRequest.User.HR_code),
                            IsHtml = true,
                            CcEmails = new List<string> { leaveRequest.User.Email }
                        };

                        // Add medical certificate attachment if it exists
                        if (!string.IsNullOrEmpty(leaveRequest.MedicalCertificatePath))
                        {
                            var fullFilePath = Path.Combine(_webHostEnvironment.WebRootPath, leaveRequest.MedicalCertificatePath);
                            if (File.Exists(fullFilePath))
                            {
                                var fileName = Path.GetFileName(fullFilePath);
                                var attachment = new System.Net.Mail.Attachment(fullFilePath);
                                attachment.Name = fileName;

                                message.Attachments.Add(attachment);
                            }
                        }

                        var result = await _emailService.SendEmailAsync(message);
                        if (result.Success)
                        {
                            _dataContext.Opinions.Add(opinion);
                            
                            _dataContext.Users.Update(user);
                            _dataContext.LeaveRequests.Update(leaveRequest);
                        }
                    }
                    else
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

                if (leaveRequest.StartDate > now || leaveRequest.Status == LeaveRequestStatusEnum.Rejected)
                {
                    leaveRequest.Status = LeaveRequestStatusEnum.Cancelled;
                    //leaveRequest.upda = DateTime.UtcNow;
                    await _dataContext.SaveChangesAsync();

                    var senderUser = leaveRequest.User;

                    var message = new EmailMessage
                    {
                        Subject = "إلغاء طلب الأجازة",
                        Body = EmailTemplate.CreateLeaveCancellationTemplate(
                                    senderUser.Name,
                                    senderUser.Email,
                                    leaveRequest.StartDate.ToString("yyyy-MM-dd"),
                                    leaveRequest.EndDate.ToString("yyyy-MM-dd"),
                                    (int)(leaveRequest.EndDate - leaveRequest.StartDate).TotalDays + 1,
                                    leaveRequest.Type,
                                    senderUser.HR_code),
                        IsHtml = true,
                        CcEmails = new List<string> { senderUser.Email }
                    };

                    // TODO: Send email using your email service

                    return new ResponseService<bool> { Error = false, Message = "Leave cancelled successfully.", Data = true };
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
