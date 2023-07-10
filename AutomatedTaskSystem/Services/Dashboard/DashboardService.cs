using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.Dashboard.GetProjectManagerDashboard;
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
        var projects = await _context.Projects.Where(p => !p.Archived).ToListAsync();
        var tasks = await _context.Tasks
            .Where(t => !t.Archived && t.StatusId != 4 && t.StatusId != 5)
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
}
