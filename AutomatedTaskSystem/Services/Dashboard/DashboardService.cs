using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.Dashboard.GetProjectManagerDashboard;
using AutomatedTaskSystem.Dtos.Dashboard.GetTeamLeaderDashboard;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Services.ReportService;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.DashboardService;

public class DashboardService : IDashboardService
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IReportService _reportService;

    public DashboardService(
        DataContext context,
        ITokenService tokenService,
        IReportService reportService
    )
    {
        _context = context;
        _tokenService = tokenService;
        this._reportService = reportService;
    }

    public async Task<
        ActionResult<ResponseService<GetProjectManagerDashboardDto>>
    > GetProjectManagerDashboard()
    {
        var authRes = _tokenService.GetUserIdFromToken();
        if (authRes.Error)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = authRes.Message }
            );

        var statusUid = Int32.TryParse(authRes.Data, out int uid);
        if (!statusUid)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Request" }
            );

        var user = await _context.Users
            .Where(u => u.Id == uid && !u.Archived)
            .Include(u => u.Group)
            .FirstOrDefaultAsync();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );
        if (user.RoleId != 1)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );

        var users = await _context.Users.Where(u => !u.Archived).ToListAsync();
        var groups = await _context.Groups
            .Where(g => !g.Archived)
            .Include(g => g.Users)
            .ToListAsync();
        var schemas = await _context.Schemas.Where(g => !g.Archived).ToListAsync();
        var reports = await _reportService.GetAllProjectsReports(null, null);
        var projects = await _context.Projects
            .Where(
                p =>
                    !p.Archived
                    && p.Status != ProjectStatusEnum.Closed
                    && p.Status != ProjectStatusEnum.Hold
            )
            .ToListAsync();
        var tasks = await _context.Tasks
            .Where(
                t =>
                    !t.Archived
                    && t.Status != TaskStatusEnum.Done
                    && t.Status != TaskStatusEnum.Rollback
                    && t.LearningObjective.Lesson.Unit.Project.Status != ProjectStatusEnum.Closed
                    && t.LearningObjective.Lesson.Unit.Project.Status != ProjectStatusEnum.Hold
            )
            .Include(t => t.LearningObjective)
            .ThenInclude(lo => lo.Lesson)
            .ThenInclude(l => l.Unit)
            .ThenInclude(u => u.Project)
            .ToListAsync();

        if (reports.Value is null || reports.Value.Data is null)
            return new BadRequestObjectResult(
                new BaseResponseService
                {
                    Error = false,
                    Message = "Unable to fetch Project Reports"
                }
            );

        var GroupsCount = new List<GetGroupsWithUserCountDto> { };

        foreach (var group in groups)
        {
            var dto = new GetGroupsWithUserCountDto
            {
                Id = group.Id,
                Name = group.Name,
                UsersCount = 0
            };
            foreach (var u in group.Users)
                if (!u.Archived)
                    dto.UsersCount++;
            if (dto.UsersCount != 0)
                GroupsCount.Add(dto);
        }

        return new ResponseService<GetProjectManagerDashboardDto>
        {
            Error = false,
            Message = "Project Manager Dashboard View",
            Data = new GetProjectManagerDashboardDto
            {
                ProjectsReport = reports.Value.Data,
                NumberOfUsers = users.Count,
                NumberOfActiveTasks = tasks.Count,
                NumberOfProject = projects.Count,
                NumberOfSchemas = schemas.Count,
                GroupsCount = GroupsCount
            }
        };
    }

    public async Task<
        ActionResult<ResponseService<GetTeamLeaderDashboardDto>>
    > GetTeamLeaderDashboard()
    {
        var authRes = _tokenService.GetUserIdFromToken();
        if (authRes.Error)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = authRes.Message }
            );

        var statusUid = Int32.TryParse(authRes.Data, out int uid);
        if (!statusUid)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Request" }
            );

        var user = await _context.Users
            .Where(u => u.Id == uid && !u.Archived)
            .Include(u => u.Group)
            .Include(u => u.Projects)
            .ThenInclude(u => u.Units)
            .ThenInclude(u => u.Lessons)
            .ThenInclude(u => u.LearningObjectives)
            .ThenInclude(u => u.Tasks)
            .FirstOrDefaultAsync();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );
        if (user.RoleId != 3)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );

        var members = await _context.Users
            .Where(u => u.GroupId == user.GroupId && !u.Archived && u.RoleId == 4)
            .Include(u => u.Tasks)
            .ThenInclude(t => t.LearningObjective)
            .ThenInclude(lo => lo.Lesson)
            .ThenInclude(l => l.Unit)
            .ThenInclude(u => u.Project)
            .ToListAsync();

        var ProjectsDetails = new List<GetTasksPerItemDto> { };
        var TasksPerUser = new List<GetTasksPerItemDto> { };

        int activeTasks = 0;

        foreach (var member in members)
        {
            TasksPerUser.Add(
                new GetTasksPerItemDto
                {
                    Id = member.Id,
                    Name = member.Name,
                    TasksCount = member.Tasks
                        .Where(
                            t =>
                                !t.Archived
                                && t.Status != TaskStatusEnum.Done
                                && t.Status != TaskStatusEnum.Rollback
                                && t.LearningObjective.Lesson.Unit.Project.Status
                                    != ProjectStatusEnum.Closed
                                && t.LearningObjective.Lesson.Unit.Project.Status
                                    != ProjectStatusEnum.Hold
                        )
                        .ToList()
                        .Count,
                }
            );
        }

        foreach (var project in user.Projects)
        {
            var s = project.Status;
            if (s == ProjectStatusEnum.Closed || s == ProjectStatusEnum.Hold)
                continue;

            foreach (var unit in project.Units)
                foreach (var lesson in unit.Lessons)
                    foreach (var lo in lesson.LearningObjectives)
                        foreach (var task in lo.Tasks)
                            if (
                                !task.Archived
                                && task.GroupId == user.GroupId
                                && task.Status != TaskStatusEnum.Done
                                && task.Status != TaskStatusEnum.Rollback
                            )
                            {
                                var detail = ProjectsDetails.Find(p => p.Id == project.Id);
                                if (detail is null)
                                {
                                    ProjectsDetails.Add(
                                        new GetTasksPerItemDto
                                        {
                                            Id = project.Id,
                                            Name = project.Name,
                                            TasksCount = 1
                                        }
                                    );
                                }
                                else
                                    detail.TasksCount++;
                                activeTasks++;
                            }
        }

        return new ResponseService<GetTeamLeaderDashboardDto>
        {
            Message = "Team Leader Dashboard",
            Error = false,
            Data = new GetTeamLeaderDashboardDto
            {
                Members = members.Count,
                Projects = user.Projects
                    .Where(
                        p => p.Status == ProjectStatusEnum.Active || p.Status == ProjectStatusEnum.Reopened
                    )
                    .ToList()
                    .Count,
                ActiveTasks = activeTasks,
                ProjectsDetails = ProjectsDetails,
                TasksPerUser = TasksPerUser
            }
        };
    }
}
