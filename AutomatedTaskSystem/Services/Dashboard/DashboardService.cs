using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.Dashboard.GetMemberDashboard;
using AutomatedTaskSystem.Dtos.Dashboard.GetProjectManagerDashboard;
using AutomatedTaskSystem.Dtos.Dashboard.GetSectionHeadDashboard;
using AutomatedTaskSystem.Dtos.Dashboard.GetTeamLeaderDashboard;
using AutomatedTaskSystem.Models.Enums.NotificationCategory;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Models.Enums.TaskActivityType;
using AutomatedTaskSystem.Models.Enums.TaskPriority;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Models.Enums.UserRole;
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
            .FirstOrDefaultAsync();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );
        if (user.Role != UserRoleEnum.ProjectManger && user.Role != UserRoleEnum.Owner)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );

        var groups = await _context.Groups
            .Where(g => !g.Archived)
            .Include(g => g.Users)
            .ToListAsync();
        var activeGroups = groups
            .Where(g => g.Users.Any(u => !u.Archived))
            .ToList();

        var orgUsers = await _context.Users
            .Where(
                u =>
                    !u.Archived
                    && u.Role != UserRoleEnum.ProjectManger
                    && u.Role != UserRoleEnum.Owner
            )
            .CountAsync();

        var activeSubjects = await _context.Subjects
            .Where(
                s =>
                    !s.Archived
                    && s.Status != ProjectStatusEnum.Closed
                    && s.Status != ProjectStatusEnum.Hold
            )
            .Include(s => s.SubjectGroup)
                .ThenInclude(sg => sg.Term)
                    .ThenInclude(t => t.Project)
                        .ThenInclude(p => p.Year)
            .ToListAsync();

        var subjectIds = activeSubjects.Select(s => s.Id).ToList();

        var allTasks = await _context.Tasks
            .Where(
                t =>
                    !t.Archived
                    && t.LearningObjective.Lesson.Unit.Subject.Status
                        != ProjectStatusEnum.Closed
                    && t.LearningObjective.Lesson.Unit.Subject.Status
                        != ProjectStatusEnum.Hold
            )
            .Include(t => t.User)
            .Include(t => t.LearningObjective)
                .ThenInclude(lo => lo.Lesson)
                    .ThenInclude(l => l.Unit)
                        .ThenInclude(u => u.Subject)
            .AsSplitQuery()
            .ToListAsync();

        var learningObjectiveRows = await _context.LearningObjectives
            .Where(
                lo =>
                    !lo.Archived
                    && subjectIds.Contains(lo.Lesson.Unit.SubjectId)
            )
            .Select(
                lo =>
                    new
                    {
                        lo.Id,
                        lo.DoneAt,
                        SubjectId = lo.Lesson.Unit.SubjectId,
                    }
            )
            .ToListAsync();

        var loCompleted = learningObjectiveRows.Count(lo => lo.DoneAt.HasValue);
        var loUncompleted = learningObjectiveRows.Count - loCompleted;
        var incompleteLoIds = learningObjectiveRows
            .Where(lo => !lo.DoneAt.HasValue)
            .Select(lo => lo.Id)
            .ToHashSet();

        var toDoCount = allTasks.Count(
            t => t.Status == TaskStatusEnum.Backlog || t.Status == TaskStatusEnum.ToDo
        );
        var doingCount = allTasks.Count(t => t.Status == TaskStatusEnum.Doing);
        var rollbackCount = allTasks.Count(t => t.Status == TaskStatusEnum.Rollback);
        var flaggedCount = allTasks.Count(
            t => t.Flagged && t.Status != TaskStatusEnum.Done
        );
        var doneCount = allTasks.Count(t => t.Status == TaskStatusEnum.Done);
        var totalTasks = allTasks.Count;

        var activeTasks = allTasks
            .Where(
                t =>
                    t.Status != TaskStatusEnum.Done
                    && t.Status != TaskStatusEnum.Rollback
            )
            .ToList();
        var totalIncompleteLos = incompleteLoIds.Count;

        var teamsWorkload = activeGroups
            .Select(
                g =>
                {
                    var groupLoCount = activeTasks
                        .Where(
                            t =>
                                t.GroupId == g.Id
                                && incompleteLoIds.Contains(t.LearningObjectiveId)
                        )
                        .Select(t => t.LearningObjectiveId)
                        .Distinct()
                        .Count();
                    var percent = totalIncompleteLos > 0
                        ? Math.Round((double)groupLoCount / totalIncompleteLos * 100, 0)
                        : 0;
                    return new ProjectManagerTeamWorkloadDto
                    {
                        Id = g.Id,
                        Name = g.Name,
                        TaskCount = groupLoCount,
                        WorkloadPercent = percent,
                    };
                }
            )
            .Where(g => g.TaskCount > 0)
            .OrderByDescending(g => g.WorkloadPercent)
            .Take(10)
            .ToList();

        string MapProjectStatus(ProjectStatusEnum status)
        {
            if (status == ProjectStatusEnum.Closed)
                return "completed";
            if (status == ProjectStatusEnum.Hold)
                return "at_risk";
            return "on_track";
        }

        var projectsTable = new List<ProjectManagerProjectRowDto>();
        foreach (var project in activeSubjects.OrderBy(p => p.Name).Take(8))
        {
            var projectLos = learningObjectiveRows
                .Where(lo => lo.SubjectId == project.Id)
                .ToList();
            var projectDone = projectLos.Count(lo => lo.DoneAt.HasValue);
            var projectTotal = projectLos.Count;
            var progress = projectTotal > 0
                ? Math.Round((double)projectDone / projectTotal * 100, 0)
                : 0;

            var yearName = project.SubjectGroup?.Term?.Project?.Year?.Name ?? "";
            var deadline = project.SubjectGroup?.Term?.EndDate;

            projectsTable.Add(
                new ProjectManagerProjectRowDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Year = yearName,
                    Status = MapProjectStatus(project.Status),
                    ProgressPercent = progress,
                    Deadline = deadline.HasValue
                        ? deadline.Value.ToString("MMM dd, yyyy")
                        : "",
                }
            );
        }

        var allLoIds = learningObjectiveRows.Select(lo => lo.Id).ToList();
        var sprintLinks = await _context.SprintLearningObjectives
            .Where(
                slo =>
                    allLoIds.Contains(slo.LearningObjectiveId)
                    && slo.Sprint != null
                    && !slo.Sprint.IsArchived
            )
            .Include(slo => slo.Sprint)
            .Include(slo => slo.LearningObjective)
                .ThenInclude(lo => lo.Lesson)
                    .ThenInclude(l => l.Unit)
            .AsSplitQuery()
            .ToListAsync();

        var sprintIds = sprintLinks.Select(slo => slo.SprintId).Distinct().ToList();
        var today = DateTime.Today;

        var sprintsTable = new List<ProjectManagerSprintRowDto>();
        foreach (
            var sprintGroup in sprintLinks
                .GroupBy(slo => slo.Sprint!)
                .OrderBy(g => g.Key.EndDate)
                .Take(6)
        )
        {
            var sprint = sprintGroup.Key;
            var sprintLoIds = sprintGroup.Select(slo => slo.LearningObjectiveId).ToList();
            var sprintTasks = allTasks
                .Where(t => sprintLoIds.Contains(t.LearningObjectiveId))
                .ToList();
            var sprintDone = sprintTasks.Count(t => t.Status == TaskStatusEnum.Done);
            var sprintTotal = sprintTasks.Count;
            var progress = sprintTotal > 0
                ? Math.Round((double)sprintDone / sprintTotal * 100, 0)
                : 0;

            var firstLo = sprintGroup.First().LearningObjective;
            var subjectId = firstLo?.Lesson?.Unit?.SubjectId;
            var subject = activeSubjects.FirstOrDefault(s => s.Id == subjectId);
            var yearName = subject?.SubjectGroup?.Term?.Project?.Year?.Name ?? "";

            var daysLeft = (sprint.EndDate.Date - today).Days;
            var sprintStatus =
                daysLeft < 0 && progress < 100
                    ? "at_risk"
                    : progress >= 100
                    ? "completed"
                    : "on_track";

            sprintsTable.Add(
                new ProjectManagerSprintRowDto
                {
                    Id = sprint.Id,
                    Name = sprint.Name,
                    ProjectName = subject?.Name ?? sprint.Description,
                    Year = yearName,
                    Status = sprintStatus,
                    ProgressPercent = progress,
                    Deadline = sprint.EndDate.ToString("MMM dd, yyyy"),
                }
            );
        }

        var flaggedRollbackTasks = allTasks
            .Where(
                t =>
                    t.Flagged
                    || t.IsRollback
                    || t.Status == TaskStatusEnum.Rollback
            )
            .OrderByDescending(t => t.CreatedAt)
            .Take(8)
            .Select(
                t =>
                    new ProjectManagerFlaggedRollbackDto
                    {
                        TaskId = t.Id,
                        ProjectId = t.LearningObjective.Lesson.Unit.Subject.Id,
                        UserName = t.User?.Name ?? "Unassigned",
                        TaskName = t.Name,
                        Type =
                            t.Flagged && t.Status != TaskStatusEnum.Rollback
                                ? "flagged"
                                : "rollback",
                        Timestamp = t.CreatedAt.ToString("O"),
                    }
            )
            .ToList();

        var allTaskIds = allTasks.Select(t => t.Id).ToList();
        var activities = await _context.TaskActivities
            .Where(a => allTaskIds.Contains(a.TaskId))
            .Include(a => a.ActorOne)
            .Include(a => a.Task)
            .OrderByDescending(a => a.TimeStamp)
            .Take(8)
            .ToListAsync();

        var activityLog = activities
            .Select(
                a =>
                {
                    var actorName = a.ActorOne?.Name ?? "System";
                    var initials = string.Join(
                        "",
                        actorName
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Take(2)
                            .Select(part => part[0])
                    )
                        .ToUpper();
                    var message = BuildActivityMessage(a.Type, actorName, a.Task.Name);
                    return new ProjectManagerActivityDto
                    {
                        Id = a.Id,
                        UserName = actorName,
                        Initials = initials,
                        Message = message,
                        CreatedAt = a.TimeStamp.ToString("O"),
                    };
                }
            )
            .ToList();

        return new ResponseService<GetProjectManagerDashboardDto>
        {
            Error = false,
            Message = "Project Manager Dashboard",
            Data = new GetProjectManagerDashboardDto
            {
                Projects = activeSubjects.Count,
                Sprints = sprintIds.Count,
                LearningObjectives = learningObjectiveRows.Count,
                Users = orgUsers,
                TeamsWorkload = teamsWorkload,
                LearningObjectivesOverview = new ProjectManagerLearningObjectivesOverviewDto
                {
                    Completed = loCompleted,
                    Uncompleted = loUncompleted,
                    Total = learningObjectiveRows.Count,
                },
                TasksOverview = new ProjectManagerTasksOverviewDto
                {
                    ToDo = toDoCount,
                    Doing = doingCount,
                    Rollback = rollbackCount,
                    Flagged = flaggedCount,
                    Done = doneCount,
                    Total = totalTasks,
                },
                ProjectsTable = projectsTable,
                FlaggedRollbackTasks = flaggedRollbackTasks,
                SprintsTable = sprintsTable,
                ActivityLog = activityLog,
            },
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
            .Include(u => u.Subjects)
                .ThenInclude(s => s.SubjectGroup)
                    .ThenInclude(sg => sg.Term)
                        .ThenInclude(t => t.Project)
                            .ThenInclude(p => p.Year)
            .FirstOrDefaultAsync();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );
        if (user.Role != UserRoleEnum.TeamLeader)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );

        var activeSubjects = user.Subjects
            .Where(
                s =>
                    !s.Archived
                    && s.Status != ProjectStatusEnum.Closed
                    && s.Status != ProjectStatusEnum.Hold
            )
            .ToList();
        var subjectIds = activeSubjects.Select(s => s.Id).ToList();

        var members = await _context.Users
            .Where(
                u =>
                    u.GroupId == user.GroupId
                    && !u.Archived
                    && u.Role == UserRoleEnum.Member
            )
            .ToListAsync();

        var groupTasks = await _context.Tasks
            .Where(
                t =>
                    !t.Archived
                    && t.GroupId == user.GroupId
                    && t.LearningObjective.Lesson.Unit.Subject.Status
                        != ProjectStatusEnum.Closed
                    && t.LearningObjective.Lesson.Unit.Subject.Status
                        != ProjectStatusEnum.Hold
            )
            .Include(t => t.User)
            .Include(t => t.LearningObjective)
                .ThenInclude(lo => lo.Lesson)
                    .ThenInclude(l => l.Unit)
                        .ThenInclude(u => u.Subject)
            .AsSplitQuery()
            .ToListAsync();

        var learningObjectiveRows = await _context.LearningObjectives
            .Where(
                lo =>
                    !lo.Archived
                    && subjectIds.Contains(lo.Lesson.Unit.SubjectId)
            )
            .Select(
                lo =>
                    new
                    {
                        lo.Id,
                        lo.DoneAt,
                        SubjectId = lo.Lesson.Unit.SubjectId,
                    }
            )
            .ToListAsync();

        var loCompleted = learningObjectiveRows.Count(lo => lo.DoneAt.HasValue);
        var loUncompleted = learningObjectiveRows.Count - loCompleted;

        var toDoCount = groupTasks.Count(
            t => t.Status == TaskStatusEnum.Backlog || t.Status == TaskStatusEnum.ToDo
        );
        var doingCount = groupTasks.Count(t => t.Status == TaskStatusEnum.Doing);
        var rollbackCount = groupTasks.Count(t => t.Status == TaskStatusEnum.Rollback);
        var flaggedCount = groupTasks.Count(
            t => t.Flagged && t.Status != TaskStatusEnum.Done
        );
        var doneCount = groupTasks.Count(t => t.Status == TaskStatusEnum.Done);
        var totalTasks = groupTasks.Count;

        var teamDone = doneCount;
        var teamPerformance = totalTasks > 0
            ? Math.Round((double)teamDone / totalTasks * 100, 0)
            : 0;

        var activeMemberTasks = groupTasks
            .Where(
                t =>
                    t.Status != TaskStatusEnum.Done
                    && t.Status != TaskStatusEnum.Rollback
            )
            .ToList();
        var totalActiveMemberWorkload = activeMemberTasks.Count;

        var membersWorkload = members
            .Select(
                m =>
                {
                    var memberActiveCount = activeMemberTasks.Count(t => t.UserId == m.Id);
                    var percent = totalActiveMemberWorkload > 0
                        ? Math.Round(
                            (double)memberActiveCount / totalActiveMemberWorkload * 100,
                            0
                        )
                        : 0;
                    return new TeamLeaderMemberWorkloadDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        TaskCount = memberActiveCount,
                        WorkloadPercent = percent,
                    };
                }
            )
            .OrderByDescending(m => m.WorkloadPercent)
            .ToList();

        string MapProjectStatus(ProjectStatusEnum status)
        {
            if (status == ProjectStatusEnum.Closed)
                return "completed";
            if (status == ProjectStatusEnum.Hold)
                return "at_risk";
            return "on_track";
        }

        var projectsTable = new List<TeamLeaderProjectRowDto>();
        foreach (var project in activeSubjects.OrderBy(p => p.Name))
        {
            var projectLos = learningObjectiveRows
                .Where(lo => lo.SubjectId == project.Id)
                .ToList();
            var projectDone = projectLos.Count(lo => lo.DoneAt.HasValue);
            var projectTotal = projectLos.Count;
            var progress = projectTotal > 0
                ? Math.Round((double)projectDone / projectTotal * 100, 0)
                : 0;

            var yearName = project.SubjectGroup?.Term?.Project?.Year?.Name ?? "";
            var deadline = project.SubjectGroup?.Term?.EndDate;

            projectsTable.Add(
                new TeamLeaderProjectRowDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Year = yearName,
                    Status = MapProjectStatus(project.Status),
                    ProgressPercent = progress,
                    Deadline = deadline.HasValue
                        ? deadline.Value.ToString("MMM dd, yyyy")
                        : "",
                }
            );
        }

        var teamLoIds = learningObjectiveRows.Select(lo => lo.Id).ToList();
        var sprintLinks = await _context.SprintLearningObjectives
            .Where(
                slo =>
                    teamLoIds.Contains(slo.LearningObjectiveId)
                    && slo.Sprint != null
                    && !slo.Sprint.IsArchived
            )
            .Include(slo => slo.Sprint)
            .Include(slo => slo.LearningObjective)
                .ThenInclude(lo => lo.Lesson)
                    .ThenInclude(l => l.Unit)
            .AsSplitQuery()
            .ToListAsync();

        var sprintIds = sprintLinks.Select(slo => slo.SprintId).Distinct().ToList();
        var today = DateTime.Today;

        var sprintsTable = new List<TeamLeaderSprintRowDto>();
        foreach (
            var sprintGroup in sprintLinks
                .GroupBy(slo => slo.Sprint!)
                .OrderBy(g => g.Key.EndDate)
                .Take(6)
        )
        {
            var sprint = sprintGroup.Key;
            var sprintLoIds = sprintGroup.Select(slo => slo.LearningObjectiveId).ToList();
            var sprintTasks = groupTasks
                .Where(t => sprintLoIds.Contains(t.LearningObjectiveId))
                .ToList();
            var sprintDone = sprintTasks.Count(t => t.Status == TaskStatusEnum.Done);
            var sprintTotal = sprintTasks.Count;
            var progress = sprintTotal > 0
                ? Math.Round((double)sprintDone / sprintTotal * 100, 0)
                : 0;

            var firstLo = sprintGroup.First().LearningObjective;
            var subjectId = firstLo?.Lesson?.Unit?.SubjectId;
            var subject = activeSubjects.FirstOrDefault(s => s.Id == subjectId);
            var yearName = subject?.SubjectGroup?.Term?.Project?.Year?.Name ?? "";

            var daysLeft = (sprint.EndDate.Date - today).Days;
            var sprintStatus =
                daysLeft < 0 && progress < 100
                    ? "at_risk"
                    : progress >= 100
                    ? "completed"
                    : "on_track";

            sprintsTable.Add(
                new TeamLeaderSprintRowDto
                {
                    Id = sprint.Id,
                    Name = sprint.Name,
                    ProjectName = subject?.Name ?? sprint.Description,
                    Year = yearName,
                    Status = sprintStatus,
                    ProgressPercent = progress,
                    Deadline = sprint.EndDate.ToString("MMM dd, yyyy"),
                }
            );
        }

        var flaggedRollbackTasks = groupTasks
            .Where(
                t =>
                    t.Flagged
                    || t.IsRollback
                    || t.Status == TaskStatusEnum.Rollback
            )
            .OrderByDescending(t => t.CreatedAt)
            .Take(8)
            .Select(
                t =>
                    new TeamLeaderFlaggedRollbackDto
                    {
                        TaskId = t.Id,
                        ProjectId = t.LearningObjective.Lesson.Unit.Subject.Id,
                        UserName = t.User?.Name ?? "Unassigned",
                        TaskName = t.Name,
                        Type =
                            t.Flagged && t.Status != TaskStatusEnum.Rollback
                                ? "flagged"
                                : "rollback",
                        Timestamp = t.CreatedAt.ToString("O"),
                    }
            )
            .ToList();

        var groupTaskIds = groupTasks.Select(t => t.Id).ToList();
        var activities = await _context.TaskActivities
            .Where(a => groupTaskIds.Contains(a.TaskId))
            .Include(a => a.ActorOne)
            .Include(a => a.Task)
            .OrderByDescending(a => a.TimeStamp)
            .Take(8)
            .ToListAsync();

        var activityLog = activities
            .Select(
                a =>
                {
                    var actorName = a.ActorOne?.Name ?? "System";
                    var initials = string.Join(
                        "",
                        actorName
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Take(2)
                            .Select(part => part[0])
                    )
                        .ToUpper();
                    var message = BuildActivityMessage(a.Type, actorName, a.Task.Name);
                    return new TeamLeaderActivityDto
                    {
                        Id = a.Id,
                        UserName = actorName,
                        Initials = initials,
                        Message = message,
                        CreatedAt = a.TimeStamp.ToString("O"),
                    };
                }
            )
            .ToList();

        return new ResponseService<GetTeamLeaderDashboardDto>
        {
            Message = "Team Leader Dashboard",
            Error = false,
            Data = new GetTeamLeaderDashboardDto
            {
                Projects = activeSubjects.Count,
                Sprints = sprintIds.Count,
                LearningObjectives = learningObjectiveRows.Count,
                Users = members.Count,
                TeamPerformance = teamPerformance,
                MembersWorkload = membersWorkload,
                LearningObjectivesOverview = new TeamLeaderLearningObjectivesOverviewDto
                {
                    Completed = loCompleted,
                    Uncompleted = loUncompleted,
                    Total = learningObjectiveRows.Count,
                },
                TasksOverview = new TeamLeaderTasksOverviewDto
                {
                    ToDo = toDoCount,
                    Doing = doingCount,
                    Rollback = rollbackCount,
                    Flagged = flaggedCount,
                    Done = doneCount,
                    Total = totalTasks,
                },
                ProjectsTable = projectsTable,
                FlaggedRollbackTasks = flaggedRollbackTasks,
                SprintsTable = sprintsTable,
                ActivityLog = activityLog,
            },
        };
    }

    public async Task<
        ActionResult<ResponseService<GetSectionHeadDashboardDto>>
    > GetSectionHeadDashboard()
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
            .Include(u => u.Subjects)
                .ThenInclude(s => s.SubjectGroup)
                    .ThenInclude(sg => sg.Term)
                        .ThenInclude(t => t.Project)
                            .ThenInclude(p => p.Year)
            .FirstOrDefaultAsync();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );
        if (user.Role != UserRoleEnum.SectionHead)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );

        var sectionGroupIds = await (
            from sg in _context.SectionGroups
            join section in _context.Sections on sg.SectionId equals section.Id
            where section.HeadId == uid && !section.Archived
            select sg.GroupId
        )
            .Distinct()
            .ToListAsync();

        if (user.GroupId.HasValue && !sectionGroupIds.Contains(user.GroupId.Value))
            sectionGroupIds.Add(user.GroupId.Value);

        var groups = sectionGroupIds.Any()
            ? await _context.Groups
                .Where(g => sectionGroupIds.Contains(g.Id) && !g.Archived)
                .ToListAsync()
            : new List<Models.Group>();

        var sectionUsers = sectionGroupIds.Any()
            ? await _context.Users
                .Where(
                    u =>
                        !u.Archived
                        && sectionGroupIds.Contains(u.GroupId ?? 0)
                        && u.Role != UserRoleEnum.ProjectManger
                        && u.Role != UserRoleEnum.SectionHead
                )
                .ToListAsync()
            : new List<Models.User>();

        var sectionTasks = sectionGroupIds.Any()
            ? await _context.Tasks
                .Where(
                    t =>
                        !t.Archived
                        && sectionGroupIds.Contains(t.GroupId)
                        && t.LearningObjective.Lesson.Unit.Subject.Status
                            != ProjectStatusEnum.Closed
                        && t.LearningObjective.Lesson.Unit.Subject.Status
                            != ProjectStatusEnum.Hold
                )
                .Include(t => t.User)
                .Include(t => t.LearningObjective)
                    .ThenInclude(lo => lo.Lesson)
                        .ThenInclude(l => l.Unit)
                            .ThenInclude(u => u.Subject)
                .AsSplitQuery()
                .ToListAsync()
            : new List<Models.Task>();

        var activeSubjects = user.Subjects
            .Where(
                s =>
                    !s.Archived
                    && s.Status != ProjectStatusEnum.Closed
                    && s.Status != ProjectStatusEnum.Hold
            )
            .ToList();

        if (!activeSubjects.Any())
        {
            var taskSubjectIds = sectionTasks
                .Select(t => t.LearningObjective.Lesson.Unit.SubjectId)
                .Distinct()
                .ToList();

            activeSubjects = taskSubjectIds.Any()
                ? await _context.Subjects
                    .Where(
                        s =>
                            !s.Archived
                            && taskSubjectIds.Contains(s.Id)
                            && s.Status != ProjectStatusEnum.Closed
                            && s.Status != ProjectStatusEnum.Hold
                    )
                    .Include(s => s.SubjectGroup)
                        .ThenInclude(sg => sg.Term)
                            .ThenInclude(t => t.Project)
                                .ThenInclude(p => p.Year)
                    .ToListAsync()
                : new List<Models.Subject>();
        }

        var subjectIds = activeSubjects.Select(s => s.Id).ToList();

        var learningObjectiveRows = await _context.LearningObjectives
            .Where(
                lo =>
                    !lo.Archived
                    && subjectIds.Contains(lo.Lesson.Unit.SubjectId)
            )
            .Select(
                lo =>
                    new
                    {
                        lo.Id,
                        lo.DoneAt,
                        SubjectId = lo.Lesson.Unit.SubjectId,
                    }
            )
            .ToListAsync();

        var loCompleted = learningObjectiveRows.Count(lo => lo.DoneAt.HasValue);
        var loUncompleted = learningObjectiveRows.Count - loCompleted;

        var toDoCount = sectionTasks.Count(
            t => t.Status == TaskStatusEnum.Backlog || t.Status == TaskStatusEnum.ToDo
        );
        var doingCount = sectionTasks.Count(t => t.Status == TaskStatusEnum.Doing);
        var rollbackCount = sectionTasks.Count(t => t.Status == TaskStatusEnum.Rollback);
        var flaggedCount = sectionTasks.Count(
            t => t.Flagged && t.Status != TaskStatusEnum.Done
        );
        var doneCount = sectionTasks.Count(t => t.Status == TaskStatusEnum.Done);
        var totalTasks = sectionTasks.Count;

        var activeGroupTasks = sectionTasks
            .Where(
                t =>
                    t.Status != TaskStatusEnum.Done
                    && t.Status != TaskStatusEnum.Rollback
            )
            .ToList();
        var totalActiveGroupWorkload = activeGroupTasks.Count;

        var teamsWorkload = groups
            .Select(
                g =>
                {
                    var groupActiveCount = activeGroupTasks.Count(t => t.GroupId == g.Id);
                    var percent = totalActiveGroupWorkload > 0
                        ? Math.Round(
                            (double)groupActiveCount / totalActiveGroupWorkload * 100,
                            0
                        )
                        : 0;
                    return new SectionHeadTeamWorkloadDto
                    {
                        Id = g.Id,
                        Name = g.Name,
                        TaskCount = groupActiveCount,
                        WorkloadPercent = percent,
                    };
                }
            )
            .OrderByDescending(g => g.WorkloadPercent)
            .ToList();

        string MapProjectStatus(ProjectStatusEnum status)
        {
            if (status == ProjectStatusEnum.Closed)
                return "completed";
            if (status == ProjectStatusEnum.Hold)
                return "at_risk";
            return "on_track";
        }

        var projectsTable = new List<SectionHeadProjectRowDto>();
        foreach (var project in activeSubjects.OrderBy(p => p.Name))
        {
            var projectLos = learningObjectiveRows
                .Where(lo => lo.SubjectId == project.Id)
                .ToList();
            var projectDone = projectLos.Count(lo => lo.DoneAt.HasValue);
            var projectTotal = projectLos.Count;
            var progress = projectTotal > 0
                ? Math.Round((double)projectDone / projectTotal * 100, 0)
                : 0;

            var yearName = project.SubjectGroup?.Term?.Project?.Year?.Name ?? "";
            var deadline = project.SubjectGroup?.Term?.EndDate;

            projectsTable.Add(
                new SectionHeadProjectRowDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Year = yearName,
                    Status = MapProjectStatus(project.Status),
                    ProgressPercent = progress,
                    Deadline = deadline.HasValue
                        ? deadline.Value.ToString("MMM dd, yyyy")
                        : "",
                }
            );
        }

        var sectionLoIds = learningObjectiveRows.Select(lo => lo.Id).ToList();
        var sprintLinks = await _context.SprintLearningObjectives
            .Where(
                slo =>
                    sectionLoIds.Contains(slo.LearningObjectiveId)
                    && slo.Sprint != null
                    && !slo.Sprint.IsArchived
            )
            .Include(slo => slo.Sprint)
            .Include(slo => slo.LearningObjective)
                .ThenInclude(lo => lo.Lesson)
                    .ThenInclude(l => l.Unit)
            .AsSplitQuery()
            .ToListAsync();

        var sprintIds = sprintLinks.Select(slo => slo.SprintId).Distinct().ToList();
        var today = DateTime.Today;

        var sprintsTable = new List<SectionHeadSprintRowDto>();
        foreach (
            var sprintGroup in sprintLinks
                .GroupBy(slo => slo.Sprint!)
                .OrderBy(g => g.Key.EndDate)
                .Take(6)
        )
        {
            var sprint = sprintGroup.Key;
            var sprintLoIds = sprintGroup.Select(slo => slo.LearningObjectiveId).ToList();
            var sprintTasks = sectionTasks
                .Where(t => sprintLoIds.Contains(t.LearningObjectiveId))
                .ToList();
            var sprintDone = sprintTasks.Count(t => t.Status == TaskStatusEnum.Done);
            var sprintTotal = sprintTasks.Count;
            var progress = sprintTotal > 0
                ? Math.Round((double)sprintDone / sprintTotal * 100, 0)
                : 0;

            var firstLo = sprintGroup.First().LearningObjective;
            var subjectId = firstLo?.Lesson?.Unit?.SubjectId;
            var subject = activeSubjects.FirstOrDefault(s => s.Id == subjectId);
            var yearName = subject?.SubjectGroup?.Term?.Project?.Year?.Name ?? "";

            var daysLeft = (sprint.EndDate.Date - today).Days;
            var sprintStatus =
                daysLeft < 0 && progress < 100
                    ? "at_risk"
                    : progress >= 100
                    ? "completed"
                    : "on_track";

            sprintsTable.Add(
                new SectionHeadSprintRowDto
                {
                    Id = sprint.Id,
                    Name = sprint.Name,
                    ProjectName = subject?.Name ?? sprint.Description,
                    Year = yearName,
                    Status = sprintStatus,
                    ProgressPercent = progress,
                    Deadline = sprint.EndDate.ToString("MMM dd, yyyy"),
                }
            );
        }

        var flaggedRollbackTasks = sectionTasks
            .Where(
                t =>
                    t.Flagged
                    || t.IsRollback
                    || t.Status == TaskStatusEnum.Rollback
            )
            .OrderByDescending(t => t.CreatedAt)
            .Take(8)
            .Select(
                t =>
                    new SectionHeadFlaggedRollbackDto
                    {
                        TaskId = t.Id,
                        ProjectId = t.LearningObjective.Lesson.Unit.Subject.Id,
                        UserName = t.User?.Name ?? "Unassigned",
                        TaskName = t.Name,
                        Type =
                            t.Flagged && t.Status != TaskStatusEnum.Rollback
                                ? "flagged"
                                : "rollback",
                        Timestamp = t.CreatedAt.ToString("O"),
                    }
            )
            .ToList();

        var sectionTaskIds = sectionTasks.Select(t => t.Id).ToList();
        var activities = await _context.TaskActivities
            .Where(a => sectionTaskIds.Contains(a.TaskId))
            .Include(a => a.ActorOne)
            .Include(a => a.Task)
            .OrderByDescending(a => a.TimeStamp)
            .Take(8)
            .ToListAsync();

        var activityLog = activities
            .Select(
                a =>
                {
                    var actorName = a.ActorOne?.Name ?? "System";
                    var initials = string.Join(
                        "",
                        actorName
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Take(2)
                            .Select(part => part[0])
                    )
                        .ToUpper();
                    var message = BuildActivityMessage(a.Type, actorName, a.Task.Name);
                    return new SectionHeadActivityDto
                    {
                        Id = a.Id,
                        UserName = actorName,
                        Initials = initials,
                        Message = message,
                        CreatedAt = a.TimeStamp.ToString("O"),
                    };
                }
            )
            .ToList();

        return new ResponseService<GetSectionHeadDashboardDto>
        {
            Message = "Section Head Dashboard",
            Error = false,
            Data = new GetSectionHeadDashboardDto
            {
                Projects = activeSubjects.Count,
                Sprints = sprintIds.Count,
                LearningObjectives = learningObjectiveRows.Count,
                Users = sectionUsers.Count,
                Teams = groups.Count,
                TeamsWorkload = teamsWorkload,
                LearningObjectivesOverview = new SectionHeadLearningObjectivesOverviewDto
                {
                    Completed = loCompleted,
                    Uncompleted = loUncompleted,
                    Total = learningObjectiveRows.Count,
                },
                TasksOverview = new SectionHeadTasksOverviewDto
                {
                    ToDo = toDoCount,
                    Doing = doingCount,
                    Rollback = rollbackCount,
                    Flagged = flaggedCount,
                    Done = doneCount,
                    Total = totalTasks,
                },
                ProjectsTable = projectsTable,
                FlaggedRollbackTasks = flaggedRollbackTasks,
                SprintsTable = sprintsTable,
                ActivityLog = activityLog,
            },
        };
    }

    private static string BuildActivityMessage(
        TaskActivityTypeEnum type,
        string actorName,
        string taskName
    )
    {
        switch (type)
        {
            case TaskActivityTypeEnum.Status_Done:
                return $"{actorName} completed task \"{taskName}\"";
            case TaskActivityTypeEnum.Status_Doing:
                return $"{actorName} started working on \"{taskName}\"";
            case TaskActivityTypeEnum.Status_ToDo:
                return $"{actorName} moved \"{taskName}\" to To Do";
            case TaskActivityTypeEnum.Assign:
                return $"{actorName} updated assignment on \"{taskName}\"";
            case TaskActivityTypeEnum.Flag:
                return $"{actorName} flagged \"{taskName}\"";
            case TaskActivityTypeEnum.Rollback:
            case TaskActivityTypeEnum.Status_Rollback:
                return $"{actorName} rolled back \"{taskName}\"";
            case TaskActivityTypeEnum.Comment:
                return $"{actorName} commented on \"{taskName}\"";
            case TaskActivityTypeEnum.PriorityChange:
                return $"{actorName} changed priority on \"{taskName}\"";
            default:
                return $"{actorName} updated \"{taskName}\"";
        }
    }

    public async Task<ActionResult<ResponseService<GetMemberDashboardDto>>> GetMemberDashboard()
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
            .Include(u => u.Subjects)
            .FirstOrDefaultAsync();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );
        if (user.Role != UserRoleEnum.Member)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );

        var subjectIds = user.Subjects
            .Where(s => !s.Archived)
            .Select(s => s.Id)
            .ToList();

        var activeSubjects = user.Subjects
            .Where(
                s =>
                    !s.Archived
                    && s.Status != ProjectStatusEnum.Closed
                    && s.Status != ProjectStatusEnum.Hold
            )
            .ToList();

        var userTasks = await _context.Tasks
            .Where(
                t =>
                    !t.Archived
                    && t.UserId == uid
                    && t.LearningObjective.Lesson.Unit.Subject.Status != ProjectStatusEnum.Closed
                    && t.LearningObjective.Lesson.Unit.Subject.Status != ProjectStatusEnum.Hold
            )
            .Include(t => t.LearningObjective)
                .ThenInclude(lo => lo.Lesson)
                    .ThenInclude(l => l.Unit)
                        .ThenInclude(u => u.Subject)
            .AsSplitQuery()
            .ToListAsync();

        var userTaskLoIds = userTasks.Select(t => t.LearningObjectiveId).Distinct().ToList();

        // Resolve sprint links in SQL — avoids EF missing SprintLearningObjectives on included LOs
        var loSprintEndDates = await _context.SprintLearningObjectives
            .Where(
                slo =>
                    userTaskLoIds.Contains(slo.LearningObjectiveId)
                    && slo.Sprint != null
                    && !slo.Sprint.IsArchived
            )
            .GroupBy(slo => slo.LearningObjectiveId)
            .Select(
                g =>
                    new
                    {
                        LearningObjectiveId = g.Key,
                        EndDate = g.Min(slo => slo.Sprint!.EndDate),
                    }
            )
            .ToDictionaryAsync(x => x.LearningObjectiveId, x => x.EndDate);

        var memberSprintIds = await _context.SprintLearningObjectives
            .Where(
                slo =>
                    userTaskLoIds.Contains(slo.LearningObjectiveId)
                    && slo.Sprint != null
                    && !slo.Sprint.IsArchived
            )
            .Select(slo => slo.SprintId)
            .Distinct()
            .ToListAsync();

        var learningObjectives = await _context.LearningObjectives
            .Where(
                lo =>
                    !lo.Archived
                    && lo.Lesson.Unit.Subject.Users.Any(u => u.Id == uid)
                    && lo.Lesson.Unit.Subject.Status != ProjectStatusEnum.Closed
                    && lo.Lesson.Unit.Subject.Status != ProjectStatusEnum.Hold
            )
            .ToListAsync();

        var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var loCompletedThisMonth = learningObjectives
            .Count(lo => lo.DoneAt.HasValue && lo.DoneAt.Value >= monthStart);

        var loCompleted = learningObjectives.Count(lo => lo.DoneAt.HasValue);
        var loUncompleted = learningObjectives.Count - loCompleted;

        var toDoCount = userTasks.Count(
            t => t.Status == TaskStatusEnum.Backlog || t.Status == TaskStatusEnum.ToDo
        );
        var doingCount = userTasks.Count(t => t.Status == TaskStatusEnum.Doing);
        var rollbackCount = userTasks.Count(t => t.Status == TaskStatusEnum.Rollback);
        var flaggedCount = userTasks.Count(t => t.Flagged && t.Status != TaskStatusEnum.Done);
        var doneCount = userTasks.Count(t => t.Status == TaskStatusEnum.Done);
        var totalTasks = userTasks.Count;

        var groupMemberTasks = await _context.Tasks
            .Where(
                t =>
                    !t.Archived
                    && t.GroupId == user.GroupId
                    && t.LearningObjective.Lesson.Unit.Subject.Status != ProjectStatusEnum.Closed
                    && t.LearningObjective.Lesson.Unit.Subject.Status != ProjectStatusEnum.Hold
            )
            .ToListAsync();
        var teamDone = groupMemberTasks.Count(t => t.Status == TaskStatusEnum.Done);
        var teamPerformance = groupMemberTasks.Count > 0
            ? Math.Round((double)teamDone / groupMemberTasks.Count * 100, 0)
            : 0;

        var sprintIds = memberSprintIds;

        var activeSprints = await _context.Sprints
            .Where(s => !s.IsArchived && sprintIds.Contains(s.Id))
            .OrderBy(s => s.EndDate)
            .ToListAsync();

        var today = DateTime.Today;

        MemberTaskBoardItemDto MapTaskBoardItem(Models.Task t, bool isCompleted)
        {
            var subject = t.LearningObjective.Lesson.Unit.Subject;
            var sprintEnd = loSprintEndDates.TryGetValue(t.LearningObjectiveId, out var endDate)
                ? endDate
                : default(DateTime);

            var dueDate = sprintEnd != default(DateTime) ? sprintEnd : (DateTime?)null;
            var isDueToday = dueDate.HasValue && dueDate.Value.Date == today;

            return new MemberTaskBoardItemDto
            {
                Id = t.Id,
                Name = t.Name,
                ProjectName = subject.Name,
                ProjectId = subject.Id,
                Priority = (int)t.Priority,
                DueDate = dueDate?.ToString("MMM dd"),
                IsDueToday = isDueToday,
                IsCompleted = isCompleted
            };
        }

        var toDoTasks = userTasks
            .Where(t => t.Status == TaskStatusEnum.Backlog || t.Status == TaskStatusEnum.ToDo)
            .OrderByDescending(t => t.CreatedAt)
            .Take(2)
            .Select(t => MapTaskBoardItem(t, false))
            .ToList();

        var inProgressTasks = userTasks
            .Where(t => t.Status == TaskStatusEnum.Doing)
            .OrderByDescending(t => t.CreatedAt)
            .Take(2)
            .Select(t => MapTaskBoardItem(t, false))
            .ToList();

        var doneTasks = userTasks
            .Where(t => t.Status == TaskStatusEnum.Done)
            .OrderByDescending(t => t.CreatedAt)
            .Take(2)
            .Select(t => MapTaskBoardItem(t, true))
            .ToList();

        var sprintDeadlines = new List<MemberSprintDeadlineDto>();
        foreach (var sprint in activeSprints.Take(3))
        {
            var sprintLoIds = await _context.SprintLearningObjectives
                .Where(slo => slo.SprintId == sprint.Id)
                .Select(slo => slo.LearningObjectiveId)
                .ToListAsync();

            var sprintUserTasks = userTasks
                .Where(t => sprintLoIds.Contains(t.LearningObjectiveId))
                .ToList();

            var sprintTaskName = sprintUserTasks.FirstOrDefault()?.Name ?? sprint.Description;
            var sprintDone = sprintUserTasks.Count(t => t.Status == TaskStatusEnum.Done);
            var sprintTotal = sprintUserTasks.Count;
            var progress = sprintTotal > 0
                ? Math.Round((double)sprintDone / sprintTotal * 100, 0)
                : 0;
            var daysLeft = (sprint.EndDate.Date - today).Days;
            var urgency = daysLeft <= 2 ? "critical" : daysLeft <= 7 ? "warning" : "normal";

            sprintDeadlines.Add(
                new MemberSprintDeadlineDto
                {
                    SprintId = sprint.Id,
                    SprintName = sprint.Name,
                    TaskName = sprintTaskName,
                    DaysLeft = Math.Max(0, daysLeft),
                    ProgressPercent = progress,
                    DeadlineDate = $"Deadline {sprint.EndDate:MMMM dd}",
                    Urgency = urgency
                }
            );
        }

        var escalatedActivities = await _context.TaskActivities
            .Where(
                a =>
                    a.Type == TaskActivityTypeEnum.PriorityChange
                    && a.AdditionalInfo == "High"
                    && a.Task.UserId == uid
                    && !a.Task.Archived
            )
            .Include(a => a.Task)
                .ThenInclude(t => t.LearningObjective)
                    .ThenInclude(lo => lo.Lesson)
                        .ThenInclude(l => l.Unit)
                            .ThenInclude(u => u.Subject)
            .Include(a => a.ActorOne)
            .OrderByDescending(a => a.TimeStamp)
            .Take(5)
            .ToListAsync();

        var highPriorityTasks = userTasks
            .Where(
                t =>
                    t.Priority == TaskPriorityEnum.High
                    && t.Status != TaskStatusEnum.Done
                    && !escalatedActivities.Any(a => a.TaskId == t.Id)
            )
            .Take(3)
            .ToList();

        var escalatedTasks = escalatedActivities
            .Select(
                a =>
                    new MemberEscalatedTaskDto
                    {
                        TaskId = a.TaskId,
                        TaskName = a.Task.Name,
                        EscalatedBy = a.ActorOne?.Name ?? "Team Leader",
                        EscalatedAt = a.TimeStamp.ToString("MMM dd, yyyy"),
                        ProjectId = a.Task.LearningObjective.Lesson.Unit.Subject.Id
                    }
            )
            .ToList();

        foreach (var t in highPriorityTasks)
        {
            escalatedTasks.Add(
                new MemberEscalatedTaskDto
                {
                    TaskId = t.Id,
                    TaskName = t.Name,
                    EscalatedBy = "High Priority",
                    EscalatedAt = t.CreatedAt.ToString("MMM dd, yyyy"),
                    ProjectId = t.LearningObjective.Lesson.Unit.Subject.Id
                }
            );
        }

        var workUpdateNotifications = await _context.Notifications
            .Where(
                n =>
                    n.UserId == uid
                    && n.Category == NotificationCategoryEnum.WorkUpdates
            )
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .ToListAsync();

        var workUpdates = workUpdateNotifications
            .Select(
                n =>
                    new MemberWorkUpdateDto
                    {
                        Id = n.Id,
                        AuthorName = n.Title,
                        ProjectName = n.Message.Split(' ').Take(3).Aggregate((a, b) => a + " " + b),
                        Message = n.Message,
                        CreatedAt = n.CreatedAt.ToString("O")
                    }
            )
            .ToList();

        return new ResponseService<GetMemberDashboardDto>
        {
            Error = false,
            Message = "Member Dashboard",
            Data = new GetMemberDashboardDto
            {
                Projects = subjectIds.Count,
                ActiveProjects = activeSubjects.Count,
                Sprints = activeSprints.Count,
                LearningObjectives = learningObjectives.Count,
                LearningObjectivesThisMonth = loCompletedThisMonth,
                TeamPerformance = teamPerformance,
                LearningObjectivesOverview = new MemberLearningObjectivesOverviewDto
                {
                    Completed = loCompleted,
                    Uncompleted = loUncompleted,
                    Total = learningObjectives.Count
                },
                TasksOverview = new MemberTasksOverviewDto
                {
                    ToDo = toDoCount,
                    Doing = doingCount,
                    Rollback = rollbackCount,
                    Flagged = flaggedCount,
                    Done = doneCount,
                    Total = totalTasks
                },
                ToDoTasks = toDoTasks,
                InProgressTasks = inProgressTasks,
                DoneTasks = doneTasks,
                SprintDeadlines = sprintDeadlines,
                EscalatedTasks = escalatedTasks.Take(5).ToList(),
                WorkUpdates = workUpdates
            }
        };
    }
}
