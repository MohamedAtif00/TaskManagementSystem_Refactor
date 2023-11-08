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
        if (user.Role == UserRoleEnum.SectionHead)
        {
            var section = await _context.Sections
                .Where(s => s.HeadId == user.Id && !s.Archived)
                .Include(s => s.Groups)
                .FirstOrDefaultAsync();

            if (section is null)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = false, Message = "Section is not found" }
                );

            var groups = new List<int> { };

            foreach (var g in section.Groups)
                groups.Add(g.Id);

            var users = await _context.Users
                .Where(
                    u =>
                        !u.Archived
                        && u.Role != UserRoleEnum.ProjectManger
                        && u.Role != UserRoleEnum.SectionHead
                        && groups.Contains(u.GroupId)
                )
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
        if (user.Role == UserRoleEnum.TeamLeader)
        {
            var users = await _context.Users
                .Where(
                    u => !u.Archived && u.Role == UserRoleEnum.Member && u.GroupId == user.GroupId
                )
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

    public async Task<ActionResult<ResponseService<UserTaskInfo>>> GetUserTasks(int id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id && !u.Archived)
            .Include(u => u.Tasks)
            .ThenInclude(t => t.LearningObjective)
            .ThenInclude(t => t.Lesson)
            .ThenInclude(t => t.Unit)
            .ThenInclude(t => t.Project)
            .Include(u => u.Group)
            .FirstOrDefaultAsync();

        if (user is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "User is not found!" }
            );

        var res = new UserTaskInfo
        {
            Id = user.Id,
            Name = user.Name,
            Group = new BasicInfoDto { Id = user.Group.Id, Name = user.Group.Name },
        };
        foreach (var task in user.Tasks.Where(t => !t.Archived && t.Status == TaskStatusEnum.ToDo))
            res.TodoTasks.Add(
                new TaskInfoDto
                {
                    Id = task.Id,
                    LearningObjective =
                    {
                        Id = task.LearningObjective.Id,
                        Name = task.LearningObjective.Name
                    },
                    Name = task.Name,
                    ProjectId = task.LearningObjective.Lesson.Unit.ProjectId
                }
            );

        foreach (var task in user.Tasks.Where(t => !t.Archived && t.Status == TaskStatusEnum.Doing))
            res.DoingTasks.Add(
                new TaskInfoDto
                {
                    Id = task.Id,
                    LearningObjective =
                    {
                        Id = task.LearningObjective.Id,
                        Name = task.LearningObjective.Name
                    },
                    Name = task.Name,
                    ProjectId = task.LearningObjective.Lesson.Unit.ProjectId
                }
            );

        await _context
            .Entry(user)
            .Collection(u => u.Projects)
            .Query()
            .Include(p => p.Units)
            .ThenInclude(u => u.Lessons)
            .ThenInclude(l => l.LearningObjectives)
            .ThenInclude(lo => lo.Tasks)
            .LoadAsync();

        foreach (var p in user.Projects.Where(p => !p.Archived))
            foreach (var u in p.Units.Where(p => !p.Archived))
                foreach (var l in u.Lessons.Where(p => !p.Archived))
                    foreach (var lo in l.LearningObjectives.Where(p => !p.Archived))
                        res.BacklogCount += lo.Tasks
                            .Where(
                                t =>
                                    !t.Archived
                                    && t.Status == TaskStatusEnum.Backlog
                                    && t.GroupId == user.GroupId
                            )
                            .Count();

        return new ResponseService<UserTaskInfo>
        {
            Data = res,
            Error = false,
            Message = "User task list"
        };
    }
}
