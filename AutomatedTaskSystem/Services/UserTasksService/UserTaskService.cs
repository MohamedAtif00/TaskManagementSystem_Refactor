using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.Common;
using AutomatedTaskSystem.Dtos.UserTask;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.AuthService;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.UserTask;
using Microsoft.AspNetCore.Mvc;

public class UserTaskService : IUserTaskService
{
    private readonly IAuthService _authService;
    private readonly DataContext _context;

    public UserTaskService(IAuthService authService, DataContext context)
    {
        _authService = authService;
        _context = context;
    }

    public async Task<ActionResult<ResponseService<List<UserTaskDto>>>> GetAvailableUsersTasks()
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Auth" }
            );

        if (user.Role == UserRoleEnum.ProjectManger)
        {
            var users = await _context.Users
                .Where(u => !u.Archived && u.Role != UserRoleEnum.ProjectManger)
                .Include(u => u.Group)
                .Include(u => u.Tasks)
                .ToListAsync();

            var res = new List<UserTaskDto> { };

            users.ForEach(u =>
            {
                var record = new UserTaskDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Group = new BasicInfoDto { Id = u.GroupId, Name = u.Group.Name },
                    Tasks = new TaskCountDto
                    {
                        Doing = u.Tasks
                            .Where(t => !t.Archived && t.Status == TaskStatusEnum.Doing)
                            .Count(),
                        Todo = u.Tasks
                            .Where(t => !t.Archived && t.Status == TaskStatusEnum.ToDo)
                            .Count()
                    }
                };
                res.Add(record);
            });

            return new ResponseService<List<UserTaskDto>>
            {
                Data = res,
                Error = false,
                Message = "List of User Tasks"
            };
        }

        throw new NotImplementedException();
    }
}
