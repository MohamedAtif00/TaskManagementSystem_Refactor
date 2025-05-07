using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.Leave
{
    public class LeaveRequestService
    {
        private readonly DataContext _dataContext;

        public LeaveRequestService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        // ✅ Create
        public async Task<bool> AddVacationAsync(CreateLeaveRequestDto[] request, int userId)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null) return false;

            if (user.Annual_leave < request.Length) return false; // Check before adding

            List<Models.LeaveRequest> vacations = new();
            foreach (var vacation in request)
            {
                var leaveRequest = new Models.LeaveRequest
                {
                    UserId = vacation.UserId,
                    StartDate = DateTime.Parse(vacation.StartDate),
                    EndDate = DateTime.Parse(vacation.EndDate),
                    Reason = vacation.Reason,
                    Status = Models.LeaveRequestStatusEnum.Pending,
                };

                // Assign TeamLeaderId if the current user is a TeamLeader
                if (user.Role == UserRoleEnum.TeamLeader)
                {
                    leaveRequest.TeamleaderId = user.Id; // Assigning the TeamLeader to the request
                }

                // Assign SectionHeadId based on the Group or Section the TeamLeader belongs to
                if (user.Role == UserRoleEnum.TeamLeader)
                {
                    // Get all sections that the user's group is part of
                    var sections = await _dataContext.Sections
                        .Where(s => s.SectionGroups.Any(g => g.Id == user.GroupId)) // Check if the group is part of this section
                        .ToListAsync();

                    // You may want to select a section in a specific way (e.g., the first one, the most recent one, etc.)
                    var section = sections.FirstOrDefault(); // Just selecting the first section for simplicity

                    if (section != null)
                    {
                        leaveRequest.SectionheadId = section.HeadId; // Assign SectionHeadId from the Section entity
                    }
                }

                vacations.Add(leaveRequest);
            }

            _dataContext.Vacations.AddRange(vacations);

            user.Annual_leave -= vacations.Count;
            _dataContext.Users.Update(user);

            // Notify the receivers (separately - not adding vacations!)
            var receivers = await GetReceiversAsync(user.Role);

            foreach (var receiver in receivers)
            {
                // Send notification to receiver (future: SignalR / notification system)
                Console.WriteLine($"Notify user {receiver.Id} about the new vacation request.");
            }

            await _dataContext.SaveChangesAsync();
            return true;
        }





        // ✅ Read All (optionally filter by user)
        public async Task<ActionResult<ResponseService<List<GetLeaveRequestDto>>>> GetAllVacationsAsync(int? userId = null)
        {
            var query = _dataContext.Vacations.AsQueryable();

            if (userId.HasValue)
                query = query.Where(v => v.UserId == userId.Value);

            var result = await query
                .Include(v => v.User)
                .Select(x => new GetLeaveRequestDto
                {
                    Id = x.Id,
                    StartDate = DateOnly.FromDateTime(x.StartDate),
                    EndDate = DateOnly.FromDateTime(x.EndDate),
                    Reason = x.Reason,
                    Status = x.Status.ToString()
                })
                .ToListAsync();

            return new ResponseService<List<GetLeaveRequestDto>>
            {
                Error = false,
                Message = "Vacations retrieved successfully.",
                Data = result
            };
        }

        // ✅ Read Single
        public async Task<ActionResult<ResponseService<GetLeaveRequestDto>>> GetVacationByIdAsync(int id)
        {
            var result = await _dataContext.Vacations
                .Include(v => v.User)
                 .Select(x => new GetLeaveRequestDto
                 {
                     Id = x.Id,
                     StartDate = DateOnly.FromDateTime(x.StartDate),
                     EndDate = DateOnly.FromDateTime(x.EndDate),
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

        // ✅ Update
        public async Task<bool> UpdateVacationAsync(int id, DateTime startDate, DateTime endDate, string? reason, LeaveRequestStatusEnum status)
        {
            var vacation = await _dataContext.Vacations.FindAsync(id);
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
            var vacation = await _dataContext.Vacations.FindAsync(id);
            if (vacation == null)
                return false;

            _dataContext.Vacations.Remove(vacation);
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


    }
}
