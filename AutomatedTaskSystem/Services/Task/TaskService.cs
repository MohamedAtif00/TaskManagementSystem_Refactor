using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.Common;
using AutomatedTaskSystem.Dtos.LearningObjective;
using AutomatedTaskSystem.Dtos.Lessons;
using AutomatedTaskSystem.Dtos.NotificationDtos;
using AutomatedTaskSystem.Dtos.Projects;
using AutomatedTaskSystem.Dtos.Tasks;
using AutomatedTaskSystem.Dtos.Unit;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.TaskActivityType;
using AutomatedTaskSystem.Models.Enums.TaskBankType;
using AutomatedTaskSystem.Models.Enums.TaskDurationEndReason;
using AutomatedTaskSystem.Models.Enums.TaskPriority;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Services.AuthService;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.RollbackService;
using AutomatedTaskSystem.Services.Notification;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace AutomatedTaskSystem.Services.TaskService;

class NodeWithRevDepth
{
    public int ReversedDepth { get; set; }
    public Node Node { get; set; } = new Node { };
}

public class TaskService : ITaskService
{
    private readonly DataContext _context;
    private readonly IAuthService _authService;
    private readonly IRollbackService _rollbackService;
	private readonly INotificationService _notificationService;

	private sealed class SprintTaskRow
	{
	    public int Id { get; set; }
	    public string Name { get; set; } = string.Empty;
	    public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Backlog;
	    public bool Pause { get; set; }
	    public bool Attention { get; set; }
	    public bool Flagged { get; set; }
	    public bool IsReview { get; set; }
	    public bool IsRollback { get; set; }
	    public int RollbackCount { get; set; }
	    public int Duration { get; set; }
	    public DateTime CreatedAt { get; set; }
	    public int LearningObjectiveId { get; set; }
	    public string LearningObjectiveName { get; set; } = string.Empty;
	    public int? UserId { get; set; }
	    public string? UserName { get; set; }
	    public TaskPriorityEnum Priority { get; set; } = TaskPriorityEnum.None;
	    public string? FromName { get; set; }
	}

	private sealed class TaskActivityRangeRow
	{
	    public int TaskId { get; set; }
	    public DateTime? Earliest { get; set; }
	    public DateTime? Latest { get; set; }
	}

	private sealed class TaskDurationRow
	{
	    public int TaskId { get; set; }
	    public double Duration { get; set; }
	}

	public TaskService(DataContext context, IAuthService authService, IRollbackService rollbackService, INotificationService notificationService)
	{
	    _context = context;
	    _authService = authService;
	    _rollbackService = rollbackService;
	    _notificationService = notificationService;
	}

	public async Task<ActionResult<BaseResponseService>> AssignUser(int id, int uid)
	{
        var authedUser = await _authService.GetAuthedUser();
        if (authedUser is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Auth Request" }
            );

        if (authedUser.Role == UserRoleEnum.Member)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Unauthorized" }
            );

	        var task = await _context.Tasks.Where(t => !t.Archived && t.Id == id).FirstOrDefaultAsync();
        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        if (task.Status == TaskStatusEnum.Done || task.Status == TaskStatusEnum.Rollback)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Task is inoperable" }
            );

	        int? newlyAssignedUserId = null;

	    if (uid != 0 && uid != task.UserId)
        {
            var user = await _context.Users
                .Where(u => !u.Archived && u.Id == uid)
                .FirstOrDefaultAsync();

            if (user is null)
                return new NotFoundObjectResult(
                    new BaseResponseService { Error = true, Message = "User is not found" }
                );

            // Owner and Project Manager can assign users to tasks regardless of group
            if (authedUser.Role != UserRoleEnum.Owner && authedUser.Role != UserRoleEnum.ProjectManger && user.GroupId != task.GroupId)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Cannot assign user to task" }
                );

            if (task.Status == TaskStatusEnum.Doing)
            {
                var taskDuration = await _context.TaskWorkTimes
                    .Where(d => d.TaskId == task.Id && d.EndDate == null)
                    .FirstOrDefaultAsync();

                if (taskDuration is not null)
                {
                    var now = DateTime.Now;
                    taskDuration.EndDate = now;
                    taskDuration.Duration = now.Subtract(taskDuration.StartDate).TotalMilliseconds;
                    taskDuration.EndReason = TaskDurationEndReasonEnum.Reassign;
                }
            }

            task.User = user;
            task.UserId = user.Id;

            task.Status = TaskStatusEnum.ToDo;

	        var newActivity = new TaskActivity
            {
                Task = task,
                TaskId = task.Id,
                Type = TaskActivityTypeEnum.Assign,
                ActorOne = authedUser,
                ActorOneId = authedUser.Id,
                ActorTwo = user,
                ActorTwoId = user.Id,
                TaskSecondary = null,
                TaskSecondaryId = null,
                TimeStamp = DateTime.Now,
                AdditionalInfo = null
            };

	        _context.TaskActivities.Add(newActivity);
	        newlyAssignedUserId = user.Id;
        }
        else if (uid == 0)
        {
            task.User = null;
            task.UserId = null;

            task.Status = task.TL ? TaskStatusEnum.ToDo : TaskStatusEnum.Backlog;

            if (task.Pause)
                task.Pause = false;

            var newActivity = new TaskActivity
            {
                Task = task,
                TaskId = task.Id,
                Type = TaskActivityTypeEnum.Assign,
                ActorOne = authedUser,
                ActorOneId = authedUser.Id,
                ActorTwo = null,
                ActorTwoId = null,
                TaskSecondary = null,
                TaskSecondaryId = null,
                TimeStamp = DateTime.Now,
                AdditionalInfo = null
            };

            _context.TaskActivities.Add(newActivity);
        }

	    await _context.SaveChangesAsync();

	    if (newlyAssignedUserId.HasValue)
	    {
	        await _notificationService.NotifyUserOfTaskAssignment(newlyAssignedUserId.Value, task.Id, authedUser.Id);
	    }

	    // Notify team leader when task is unassigned and moved to backlog (non-TL tasks only)
	    if (uid == 0 && task.Status == TaskStatusEnum.Backlog)
	    {
	        await _notificationService.NotifyTeamLeaderOfBacklogTask(task.Id, task.GroupId, "Task unassigned");
	    }

	    return new BaseResponseService { Error = false, Message = "User assigned" };
    }

    public async Task<Models.Task> CreateTask(TaskBank taskBank, LearningObjective lo) =>
        await createTask(taskBank, lo);

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> CreateTask(int taskBankId, int loId, int userId)
    {
        var authedUser = await _authService.GetAuthedUser();
        if (authedUser is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );

        var lo = await _context.LearningObjectives
            .Where(lo => !lo.Archived && lo.Id == loId)
            .Include(lo => lo.Schema)
            .Include(lo => lo.Lesson)
            .ThenInclude(l => l.Unit)
            .ThenInclude(u => u.Project)
            .FirstOrDefaultAsync();
        if (lo is null)
            return new BadRequestObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = "Learning objective is not found"
                }
            );

        var TaskBankItem = await _context.TaskBank
            .Where(g => g.Id == taskBankId && g.Active)
            .Include(tb => tb.Group)
            .FirstOrDefaultAsync();
        if (TaskBankItem is null)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Task Bank Item is not found" }
            );

        var user =
            userId == 0
                ? null
                : await _context.Users
                    .Where(u => !u.Archived && u.Id == userId)
                    .FirstOrDefaultAsync();

        if (user is null && userId != 0)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "User is not found" }
            );

        // Owner and Project Manager can assign users from any group, others are restricted to same group
        if (user is not null && authedUser.Role != UserRoleEnum.Owner && authedUser.Role != UserRoleEnum.ProjectManger && user.GroupId != TaskBankItem.GroupId)
            return new BadRequestObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = "User cannot be assigned to this task"
                }
            );

        var sprintId = _context.Sprints
                .FirstOrDefault(x => DateTime.Now.Date <= x.EndDate.Date && DateTime.Now.Date >= x.StartDate.Date)?.Id;

        var task = await createTask(TaskBankItem, lo, user, sprintId);


        return await getTaskDetails(task.Id);
    }

    public async Task<Models.Task> CreateTask(Step step, LearningObjective lo) =>
        await createTask(step: step, learningObjective: lo, null);

    public async Task<Models.Task> CreateTask(Step step, LearningObjective lo, Models.Task? from) =>
        await createTask(step: step, learningObjective: lo, from);

    /// <summary>
    /// Retrieves a list of task cards associated with a specific Learning Objective.
    /// </summary>
    /// <param name="learningObjectiveId">The ID of the Learning Objective.</param>
    /// <returns>A ResponseService containing a list of GetTaskCardDto if successful, otherwise an error response.</returns>
    public async Task<ActionResult<ResponseService<List<GetTaskCardDto>>>> GetTasksByLearningObjectiveId(int learningObjectiveId, int sprintId)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
        {
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid authentication request." }
            );
        }

        var learningObjective = await _context.LearningObjectives
            .Where(lo => lo.Id == learningObjectiveId && !lo.Archived)
            .Include(lo => lo.Lesson)
                .ThenInclude(l => l.Unit)
                    .ThenInclude(u => u.Project)
                        .ThenInclude(p => p.Users) // Include project users for auth check
            .Include(lo => lo.SprintLearningObjectives)
                .ThenInclude(slo => slo.Sprint)
            .FirstOrDefaultAsync();

        if (learningObjective is null)
        {
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Learning Objective not found or archived." }
            );
        }

        // --- AUTH CHECK --- (No changes from previous refined version, assuming it's correct now)
        if (user.Role == UserRoleEnum.Member || user.Role == UserRoleEnum.TeamLeader || user.Role == UserRoleEnum.SectionHead)
        {
            var hasAccess = learningObjective.Lesson.Unit.Project.Users.Any(u => u.Id == user.Id);

            if (!hasAccess && (user.Role == UserRoleEnum.TeamLeader || user.Role == UserRoleEnum.SectionHead))
            {
                var relevantTaskGroupIds = await _context.Tasks
                                                .Where(t => t.LearningObjectiveId == learningObjectiveId)
                                                .Select(t => (int?)t.GroupId)
                                                .Distinct()
                                                .ToListAsync();

                if (relevantTaskGroupIds.Any())
                {
                    if (user.Role == UserRoleEnum.TeamLeader && relevantTaskGroupIds.Contains(user.GroupId))
                    {
                        hasAccess = true;
                    }
                    else if (user.Role == UserRoleEnum.SectionHead &&
                             await _context.SectionGroups.AnyAsync(sg => sg.Section.HeadId == user.Id && relevantTaskGroupIds.Contains(sg.GroupId)))
                    {
                        hasAccess = true;
                    }
                }
            }

            if (!hasAccess)
            {
                return new UnauthorizedObjectResult(
                    new BaseResponseService { Error = true, Message = "You do not have permission to view tasks for this Learning Objective." }
                );
            }
        }
        // --- END AUTH CHECK ---

        DateTime? sprintStartDate = null;
        DateTime? sprintEndDate = null; // Will store the start of the day *after* the sprint's actual end date

        var currentSprint = learningObjective.SprintLearningObjectives
                                .Select(slo => slo.Sprint)
                                .FirstOrDefault(s => s.Id == sprintId);

        if (currentSprint != null)
        {
            sprintStartDate = currentSprint.StartDate.Date;
            sprintEndDate = currentSprint.EndDate.Date.AddDays(1);
        }
        else
        {
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Learning Objective is not associated with the specified sprint." }
            );
        }

        // --- Core Logic: Get tasks where their LATEST activity falls within the sprint period ---

        var tasks = await _context.Tasks
            .Where(t => !t.Archived && t.LearningObjectiveId == learningObjectiveId)
            // Join with TaskActivities to find the last activity for each task
            .Select(t => new
            {
                Task = t,
                // Get the latest activity timestamp for this task
                LatestActivityTimeStamp = _context.TaskActivities
                                                    .Where(ta => ta.TaskId == t.Id)
                                                    .OrderByDescending(ta => ta.TimeStamp)
                                                    .Select(ta => ta.TimeStamp)
                                                    .FirstOrDefault() ,// This will give DateTime.MinValue if no activities
                t.CreatedAt,
                t.Status
            })
            // Filter based on the latest activity timestamp falling within the sprint dates
            .Where(x => (x.LatestActivityTimeStamp.Date >= sprintStartDate.Value.Date &&
                        x.LatestActivityTimeStamp.Date <= sprintEndDate.Value.Date) || (x.CreatedAt < sprintStartDate && x.Status == TaskStatusEnum.Backlog))
            .Select(x => x.Task) // Select the original Task entity back
            .Include(t => t.User)
            .Include(t => t.Group)
            .Include(t => t.From)
            .Select(t => new GetTaskCardDto
            {
                Paused = t.Pause,
                Attention = t.Attention,
                Flagged = t.Flagged,
                From = t.From == null ? "" : t.From.Name,
                Id = t.Id,
                IsReview = t.IsReview,
                IsRollback = t.IsRollback,
                LearningObjective = new BasicInfoDto
                {
                    Id = t.LearningObjective.Id,
                    Name = t.LearningObjective.Name
                },
                Name = t.Name,
                Priority = t.Priority,
                RollbackCount = t.RollbackCount,
                Status = t.Status, // Displays the *current* status from the Task entity
                User = t.User == null
                    ? null
                    : new BasicInfoDto
                    {
                        Name = t.User.Name,
                        Id = t.User.Id
                    },
                baseDuration = t.Duration,
                duration = (decimal?)_context.TaskWorkTimes
                    .Where(q => q.TaskId == t.Id)
                    .Sum(q => q.Duration) / 60000 // Convert milliseconds to minutes
            })
            .ToListAsync();

        return new ResponseService<List<GetTaskCardDto>>
        {
            Data = tasks,
            Error = false,
            Message = $"Successfully retrieved tasks for Learning Objective ID: {learningObjectiveId} within sprint ID: {sprintId}."
        };
    }

    /// <summary>
    /// Retrieves a list of all task cards where the task's Learning Objective is associated with the specified Sprint.
    /// Tasks whose Learning Objective is not associated with the sprint will not be included.
    /// Additionally, tasks must not be archived.
    /// The displayed start and end dates of tasks will be adjusted to fit within the sprint's duration.
    /// </summary>
    /// <param name="sprintId">The ID of the Sprint.</param>
	    /// <returns>A ResponseService containing a list of GetTaskCardDto if successful, otherwise an error response.</returns>
	public async Task<ActionResult<ResponseService<List<GetTaskCardDto>>>> GetTasksBySprintId(int sprintId)
	    {
	        var user = await _authService.GetAuthedUser();
	        if (user is null)
	        {
	            return new UnauthorizedObjectResult(
	                new BaseResponseService { Error = false, Message = "Invalid auth" }
	            );
	        }
	
	        var sprint = await _context.Sprints
	            .Where(s => s.Id == sprintId)
	            .FirstOrDefaultAsync();
	
	        if (sprint is null)
	        {
	            return new NotFoundObjectResult(
	                new BaseResponseService { Error = true, Message = "Sprint not found" }
	            );
	        }
	
	        // Define sprint start and end dates for comparison
	        var sprintStartDate = sprint.StartDate.Date;
	        var sprintEndDate = sprint.EndDate.Date;
	
	        // Base query: tasks in this sprint (via LO association) and not archived
	        IQueryable<Models.Task> tasksQuery = _context.Tasks
	            .Where(t => !t.Archived && t.LearningObjective.SprintLearningObjectives.Any(slo => slo.SprintId == sprintId))
	            .Include(t => t.LearningObjective)
	            .Include(t => t.User);
	
	        // NOTE: Role-based filtering for sprint tasks can be added here similar to
	        // GetTasksByLearningObjectiveId if/when requirements are clarified.
	
	        var tasks = await tasksQuery.ToListAsync();
	        var taskIds = tasks.Select(t => t.Id).ToList();
	
	        // Preload activity ranges for all tasks in a single query
	        var activityLookup = await _context.TaskActivities
	            .Where(ta => taskIds.Contains(ta.TaskId))
	            .GroupBy(ta => ta.TaskId)
	            .Select(g => new
	            {
	                TaskId = g.Key,
	                Earliest = g.Min(ta => ta.TimeStamp),
	                Latest = g.Max(ta => ta.TimeStamp)
	            })
	            .ToDictionaryAsync(x => x.TaskId);
	
	        // Preload work time durations for all tasks
	        var durationLookup = await _context.TaskWorkTimes
	            .Where(twt => taskIds.Contains(twt.TaskId))
	            .GroupBy(twt => twt.TaskId)
	            .Select(g => new
	            {
	                TaskId = g.Key,
	                Duration = g.Sum(twt => twt.Duration)
	            })
	            .ToDictionaryAsync(x => x.TaskId);
	
	        var result = tasks
	            .Select(task =>
	            {
	                activityLookup.TryGetValue(task.Id, out var activityInfo);
	                var latestTimestamp = activityInfo?.Latest ?? DateTime.MinValue;
	
	                var taskOriginalStart = task.CreatedAt.Date;
	                var taskOriginalEnd = latestTimestamp != DateTime.MinValue
	                    ? latestTimestamp.Date
	                    : task.CreatedAt.Date;
	
	                // Clip to sprint window
	                var effectiveStartDate = taskOriginalStart < sprintStartDate ? sprintStartDate : taskOriginalStart;
	                var effectiveEndDate = taskOriginalEnd > sprintEndDate ? sprintEndDate : taskOriginalEnd;
	
	                durationLookup.TryGetValue(task.Id, out var durationInfo);
	                var duration = durationInfo?.Duration ?? 0;
	
	                return new GetTaskCardDto
	                {
	                    Paused = task.Pause,
	                    Attention = task.Attention,
	                    Flagged = task.Flagged,
	                    From = task.From == null ? "" : task.From.Name,
	                    Id = task.Id,
	                    IsReview = task.IsReview,
	                    IsRollback = task.IsRollback,
	                    LearningObjective = new BasicInfoDto
	                    {
	                        Id = task.LearningObjective.Id,
	                        Name = task.LearningObjective.Name
	                    },
	                    Name = task.Name,
	                    Priority = task.Priority,
	                    RollbackCount = task.RollbackCount,
	                    Status = task.Status,
	                    User = task.User == null
	                        ? null
	                        : new BasicInfoDto
	                        {
	                            Name = task.User.Name,
	                            Id = task.User.Id
	                        },
	                    baseDuration = task.Duration,
	                    duration = (decimal?)duration / 60000,
	                    AdjustedStartDate = effectiveStartDate,
	                    AdjustedEndDate = effectiveEndDate
	                };
	            })
	            .ToList();
	
	        return new ResponseService<List<GetTaskCardDto>>
	        {
	            Data = result,
	            Error = false,
	            Message = $"Successfully retrieved tasks for sprint ID: {sprintId}."
	        };
		    }

	/// <summary>
	    /// Streaming version of GetTasksBySprintId that yields task cards in batches
	    /// using Dapper instead of Entity Framework for data access, while preserving
	    /// the same business logic, batching, and dictionary-based caching.
	    /// </summary>
	    /// <param name="sprintId">The ID of the Sprint.</param>
	    /// <returns>An async stream of GetTaskCardDto instances.</returns>
	public async IAsyncEnumerable<GetTaskCardDto> GetTasksBySprintIdStream(int sprintId)
	    {
		    var user = await _authService.GetAuthedUser();
		    if (user is null)
		    {
			    throw new UnauthorizedAccessException("Invalid auth");
		    }

		    var connection = _context.Database.GetDbConnection();
		    if (connection.State != ConnectionState.Open)
		    {
			    await connection.OpenAsync();
		    }

		    var isProjectManager = user.Role == UserRoleEnum.ProjectManger || user.Role == UserRoleEnum.Owner;
		    var isTeamLeader = user.Role == UserRoleEnum.TeamLeader;
		    var isSectionHead = user.Role == UserRoleEnum.SectionHead;
		    var isMember = user.Role == UserRoleEnum.Member;

		    List<Group>? allowedGroups = null;
		    List<int>? allowedGroupIds = null;

		    if (isTeamLeader || isSectionHead)
		    {
			    var userGroup = await _context.Groups
			        .Where(g => g.Id == user.GroupId)
			        .FirstOrDefaultAsync();

			    if (userGroup is null)
			    {
			        throw new Exception("User has a not found group");
			    }

			    allowedGroups = new List<Group> { userGroup };

			    if (isSectionHead)
			    {
			        var section = await _context.Sections
			            .Where(s => s.HeadId == user.Id && !s.Archived)
			            .FirstOrDefaultAsync();

			        if (section is not null)
			        {
			            var groupIds = await _context.SectionGroups
			                .Where(sg => sg.SectionId == section.Id)
			                .Select(sg => sg.GroupId)
			                .ToListAsync();

			            if (groupIds.Count > 0)
			            {
			                var sectionGroups = await _context.Groups
			                    .Where(g => groupIds.Contains(g.Id))
			                    .ToListAsync();

			                allowedGroups.AddRange(sectionGroups);
			            }
			        }
			    }

			    allowedGroupIds = allowedGroups
			        .Select(g => g.Id)
			        .Distinct()
			        .ToList();

			    if (allowedGroupIds.Count == 0)
			    {
			        yield break;
			    }
		    }
		    else if (isMember)
		    {
			    if (!user.GroupId.HasValue)
			    {
			        yield break;
			    }
		    }

		    const string sprintSql = @"SELECT TOP (1) Id, Name, Description, StartDate, EndDate, IsArchived
			                            FROM Sprints
			                            WHERE Id = @SprintId";

		    var sprint = await connection.QueryFirstOrDefaultAsync<AutomatedTaskSystem.Models.Sprint>(
			    sprintSql,
			    new { SprintId = sprintId }
		    );
		    if (sprint is null)
		    {
			    throw new InvalidOperationException("Sprint not found");
		    }

		    // Define sprint start and end dates for comparison
		    var sprintStartDate = sprint.StartDate.Date;
		    var sprintEndDate = sprint.EndDate.Date;

		    // Get all task IDs for this sprint (via LearningObjective-SprintLearningObjective association)
		    var taskIdsSql = @"SELECT DISTINCT t.Id
			                            FROM Tasks t
			                            INNER JOIN LearningObjectives lo ON t.LearningObjectiveId = lo.Id
			                            INNER JOIN SprintLearningObjectives slo ON slo.LearningObjectiveId = lo.Id
			                            WHERE t.Archived = 0 AND slo.SprintId = @SprintId";

		    if (isMember)
		    {
			    taskIdsSql += @" AND t.GroupId = @UserGroupId
			                    AND (t.UserId = @UserId OR t.Status = @BacklogStatus)
			                    AND (t.TL = 0 OR t.UserId = @UserId)";
		    }
		    else if (isTeamLeader || isSectionHead)
		    {
			    taskIdsSql += @" AND t.GroupId IN @GroupIds";
		    }

		    List<int> taskIds;
		    if (isMember)
		    {
			    taskIds = (await connection.QueryAsync<int>(
			            taskIdsSql,
			            new
			            {
			                SprintId = sprintId,
			                UserId = user.Id,
			                UserGroupId = user.GroupId!.Value,
			                BacklogStatus = (int)TaskStatusEnum.Backlog
			            }
			        ))
			        .ToList();
		    }
		    else if (isTeamLeader || isSectionHead)
		    {
			    taskIds = (await connection.QueryAsync<int>(
			            taskIdsSql,
			            new
			            {
			                SprintId = sprintId,
			                GroupIds = allowedGroupIds
			            }
			        ))
			        .ToList();
		    }
		    else
		    {
			    taskIds = (await connection.QueryAsync<int>(
			            taskIdsSql,
			            new { SprintId = sprintId }
			        ))
			        .ToList();
		    }

		    if (taskIds.Count == 0)
		    {
			    yield break;
		    }

		    // Preload activity ranges for all tasks in this sprint in a single query
		    const string activitySql = @"SELECT
			                                    ta.TaskId,
			                                    MIN(ta.TimeStamp) AS Earliest,
			                                    MAX(ta.TimeStamp) AS Latest
			                            FROM TaskActivities ta
			                            WHERE ta.TaskId IN @TaskIds
			                            GROUP BY ta.TaskId";

		    var activityLookup = (await connection.QueryAsync<TaskActivityRangeRow>(
			        activitySql,
			        new { TaskIds = taskIds }
			    ))
			    .ToDictionary(x => x.TaskId);

		    // Preload work time durations for all sprint tasks in a single query.
		    const string durationSql = @"SELECT
			                                    twt.TaskId,
			                                    SUM(twt.Duration) AS Duration
			                            FROM TaskWorkTimes twt
			                            WHERE twt.TaskId IN @TaskIds
			                            GROUP BY twt.TaskId";

		    var durationLookup = (await connection.QueryAsync<TaskDurationRow>(
			        durationSql,
			        new { TaskIds = taskIds }
			    ))
			    .ToDictionary(x => x.TaskId);

		    // Stream tasks in batches to reduce memory usage while minimizing round trips.
		    const int batchSize = 200; // tuned for 50031000+ records per sprint
		    var skip = 0;

		    var tasksBatchSql = @"SELECT
			                                        t.Id,
			                                        t.Name,
			                                        t.Status,
			                                        t.Pause,
			                                        t.Attention,
			                                        t.Flagged,
			                                        t.IsReview,
			                                        t.IsRollback,
			                                        t.RollbackCount,
			                                        t.Duration,
			                                        t.CreatedAt,
			                                        t.LearningObjectiveId,
			                                        lo.Name AS LearningObjectiveName,
			                                        t.UserId,
			                                        u.Name AS UserName,
			                                        t.Priority,
			                                        f.Name AS FromName
			                                FROM Tasks t
			                                INNER JOIN LearningObjectives lo ON t.LearningObjectiveId = lo.Id
			                                INNER JOIN SprintLearningObjectives slo ON slo.LearningObjectiveId = lo.Id
			                                LEFT JOIN Users u ON t.UserId = u.Id
			                                LEFT JOIN Tasks f ON t.FromId = f.Id
			                                WHERE t.Archived = 0 AND slo.SprintId = @SprintId";

		    if (isMember)
		    {
			    tasksBatchSql += @" AND t.GroupId = @UserGroupId
			                            AND (t.UserId = @UserId OR t.Status = @BacklogStatus)
			                            AND (t.TL = 0 OR t.UserId = @UserId)";
		    }
		    else if (isTeamLeader || isSectionHead)
		    {
			    tasksBatchSql += @" AND t.GroupId IN @GroupIds";
		    }

		    tasksBatchSql += @"
			                                ORDER BY t.Id
			                                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

		    while (true)
		    {
			    List<SprintTaskRow> tasksBatch;
			    if (isMember)
			    {
			        tasksBatch = (await connection.QueryAsync<SprintTaskRow>(
			                tasksBatchSql,
			                new
			                {
			                    SprintId = sprintId,
			                    Skip = skip,
			                    Take = batchSize,
			                    UserId = user.Id,
			                    UserGroupId = user.GroupId!.Value,
			                    BacklogStatus = (int)TaskStatusEnum.Backlog
			                }
			            ))
			            .ToList();
			    }
			    else if (isTeamLeader || isSectionHead)
			    {
			        tasksBatch = (await connection.QueryAsync<SprintTaskRow>(
			                tasksBatchSql,
			                new
			                {
			                    SprintId = sprintId,
			                    Skip = skip,
			                    Take = batchSize,
			                    GroupIds = allowedGroupIds
			                }
			            ))
			            .ToList();
			    }
			    else
			    {
			        tasksBatch = (await connection.QueryAsync<SprintTaskRow>(
			                tasksBatchSql,
			                new { SprintId = sprintId, Skip = skip, Take = batchSize }
			            ))
			            .ToList();
			    }

			    if (tasksBatch.Count == 0)
			    {
			        yield break;
			    }

			    foreach (var task in tasksBatch)
			    {
			        activityLookup.TryGetValue(task.Id, out var activityInfo);
			        var latestTimestamp = activityInfo?.Latest ?? DateTime.MinValue;

			        var taskOriginalStart = task.CreatedAt.Date;
			        var taskOriginalEnd = latestTimestamp != DateTime.MinValue
			            ? latestTimestamp.Date
			            : task.CreatedAt.Date;

			        // Clip to sprint window
			        var effectiveStartDate = taskOriginalStart < sprintStartDate ? sprintStartDate : taskOriginalStart;
			        var effectiveEndDate = taskOriginalEnd > sprintEndDate ? sprintEndDate : taskOriginalEnd;

			        durationLookup.TryGetValue(task.Id, out var durationInfo);
			        var duration = durationInfo?.Duration ?? 0d;

			        yield return new GetTaskCardDto
			        {
			            Paused = task.Pause,
			            Attention = task.Attention,
			            Flagged = task.Flagged,
			            From = task.FromName ?? string.Empty,
			            Id = task.Id,
			            IsReview = task.IsReview,
			            IsRollback = task.IsRollback,
			            LearningObjective = new BasicInfoDto
			            {
			                Id = task.LearningObjectiveId,
			                Name = task.LearningObjectiveName
			            },
			            Name = task.Name,
			            Priority = task.Priority,
			            RollbackCount = task.RollbackCount,
			            Status = task.Status,
			            User = task.UserId == null
			                ? null
			                : new BasicInfoDto
			                {
			                    Name = task.UserName ?? string.Empty,
			                    Id = task.UserId.Value
			                },
			            baseDuration = task.Duration,
			            duration = (decimal?)duration / 60000m,
			            AdjustedStartDate = effectiveStartDate,
			            AdjustedEndDate = effectiveEndDate
			        };
			    }

			    skip += tasksBatch.Count;
		    }
	    }
		
	//public async Task<ActionResult<ResponseService<List<GetTaskCardDto>>>> GetProjectTasksBySprint(int sprintId)
    //{
    //    //var user = await _authService.GetAuthedUser();
    //    //if (user is null)
    //    //    return new UnauthorizedObjectResult(
    //    //        new BaseResponseService { Error = false, Message = "Invalid auth" }
    //    //    );

    //    var sprint = await _context.Sprints
    //    .Include(s => s.Tasks)
    //        .ThenInclude(t => t.LearningObjective)
    //            .ThenInclude(lo => lo.Lesson)
    //                .ThenInclude(l => l.Unit)
    //    .Include(s => s.Tasks)
    //        .ThenInclude(t => t.User)
    //    .Include(s => s.Tasks)
    //        .ThenInclude(t => t.Group)
    //    .Include(s => s.Tasks)
    //        .ThenInclude(t => t.From)
    //    .FirstOrDefaultAsync(s => s.Id == sprintId);

    //    if (sprint is null)
    //        return new NotFoundObjectResult(
    //            new BaseResponseService { Error = true, Message = "Sprint not found" }
    //        );

    //    var tasks = sprint.Tasks
    //        .Where(t => !t.Archived)
    //        .Select(t => new GetTaskCardDto
    //        {
    //            Paused = t.Pause,
    //            Attention = t.Attention,
    //            Flagged = t.Flagged,
    //            From = t.From is null ? "" : t.From.Name,
    //            Id = t.Id,
    //            IsReview = t.IsReview,
    //            IsRollback = t.IsRollback,
    //            LearningObjective = new BasicInfoDto
    //            {
    //                Id = t.LearningObjective.Id,
    //                Name = t.LearningObjective.Name
    //            },
    //            Name = t.Name,
    //            Priority = t.Priority,
    //            RollbackCount = t.RollbackCount,
    //            Status = t.Status,
    //            User = t.User is null
    //                ? null
    //                : new BasicInfoDto
    //                {
    //                    Name = t.User.Name,
    //                    Id = t.User.Id
    //                },
    //            baseDuration = t.Duration,
    //            duration = (decimal?)_context.TaskWorkTimes
    //                .Where(q => q.TaskId == t.Id)
    //                .Sum(q => q.Duration) / 60000
    //        })
    //        .ToList();

    //    return new ResponseService<List<GetTaskCardDto>>
    //    {
    //        Data = tasks,
    //        Error = false,
    //        Message = "Public list of tasks in sprint"
    //    };
    //}
	public async Task<ActionResult<ResponseService<List<GetTaskCardDto>>>> GetProjectTask(int pid)
	    {
	        var user = await _authService.GetAuthedUser();
	        if (user is null)
	            return new UnauthorizedObjectResult(
	                new BaseResponseService { Error = false, Message = "Invalid auth" }
	            );

	        // Project managers and owners can see all tasks in the project, regardless of group
	        if (user.Role == UserRoleEnum.ProjectManger || user.Role == UserRoleEnum.Owner)
	        {
            var p = await _context.Projects
                .Where(_ => _.Id == pid && !_.Archived)
                .Include(p => p.Units)
                .ThenInclude(u => u.Lessons)
                .ThenInclude(l => l.LearningObjectives)
                .ThenInclude(lo => lo.Tasks)
                .ThenInclude(t => t.User)
                .Include(p => p.Units)
                .ThenInclude(u => u.Lessons)
                .ThenInclude(l => l.LearningObjectives)
                .ThenInclude(lo => lo.Tasks)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync();

            if (p is null)
                return new NotFoundObjectResult(
                    new BaseResponseService { Error = true, Message = "Project is not found" }
                );

            var tasks = new List<GetTaskCardDto>();
            foreach (var unit in p.Units)
            {
                if (unit.Archived)
                    continue;

                foreach (var lesson in unit.Lessons)
                {
                    if (lesson.Archived)
                        continue;

                    foreach (var lo in lesson.LearningObjectives)
                    {
                        if (lo.Archived)
                            continue;

                        foreach (var task in lo.Tasks)
                        {
                            if (!task.Archived)
                            {
                                tasks.Add(new GetTaskCardDto
                                {
                                    Paused = task.Pause,
                                    Attention = task.Attention,
                                    Flagged = task.Flagged,
                                    From = task.From is null ? "" : task.From.Name,
                                    Id = task.Id,
                                    IsReview = task.IsReview,
                                    IsRollback = task.IsRollback,
                                    LearningObjective = new BasicInfoDto
                                    {
                                        Id = lo.Id,
                                        Name = lo.Name
                                    },
                                    Name = task.Name,
                                    Priority = task.Priority,
                                    RollbackCount = task.RollbackCount,
                                    Status = task.Status,
                                    User = task.User is null
                                        ? null
                                        : new BasicInfoDto
                                        {
                                            Name = task.User.Name,
                                            Id = task.User.Id
                                        },
                                    baseDuration = task.Duration,
                                    duration = (decimal?)_context.TaskWorkTimes
                                                    .Where(q => q.TaskId == task.Id)
                                                    .Sum(q => q.Duration) / 60000
                                });
                            }
                        }
                    }
                }
            }

            return new ResponseService<List<GetTaskCardDto>>
            {
                Data = tasks,
                Error = false,
                Message = "List of all tasks in project"
            };
        }

        var project = await _context.Projects
            .Where(p => !p.Archived && p.Id == pid)
            .Include(p => p.Users)
            .FirstOrDefaultAsync();

        if (project is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Project is not found" }
            );

        if (!project.Users.Any(u => u.Id == user.Id))
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "User is unassigned to project" }
            );

	        var userGroup = await _context.Groups
	            .Where(g => g.Id == user.GroupId)
	            .FirstOrDefaultAsync();
	
	        // If for some reason the user has no associated group, fail gracefully
	        if (userGroup is null)
	            return new BadRequestObjectResult(
	                new BaseResponseService { Error = true, Message = "User has a not found group" }
	            );

        var groups = new List<Group> { userGroup };

        if (user.Role == UserRoleEnum.SectionHead)
        {
            var section = await _context.Sections
                .Where(s => s.HeadId == user.Id && !s.Archived)
                .FirstOrDefaultAsync();

            if (section is not null)
            {
                var sectionGroupIds = await _context.SectionGroups
                    .Where(sg => sg.SectionId == section.Id)
                    .Select(sg => sg.GroupId)
                    .ToListAsync();

                var sectionGroups = await _context.Groups
                    .Where(g => sectionGroupIds.Contains(g.Id))
                    .ToListAsync();

                groups.AddRange(sectionGroups);
            }
        }

        IQueryable<Models.Task> query;

        if (user.Role == UserRoleEnum.TeamLeader || user.Role == UserRoleEnum.SectionHead)
        {
            query = _context.Tasks.Where(
                t =>
                    groups.Select(g => g.Id).Contains(t.GroupId)
                    && t.LearningObjective.Lesson.Unit.ProjectId == project.Id
                    && !t.Archived
            );
        }
        else
        {
            query = _context.Tasks.Where(
                t =>
                    t.GroupId == user.GroupId
                    && !t.Archived
                    && t.LearningObjective.Lesson.Unit.ProjectId == project.Id
                    && (t.UserId == user.Id || t.Status == TaskStatusEnum.Backlog)
                    && (!t.TL || t.UserId == user.Id)
            );
        }

        var _tasks = await query
            .Include(t => t.LearningObjective)
            .ThenInclude(lo => lo.Lesson)
            .ThenInclude(l => l.Unit)
            .Include(t => t.User)
            .Include(t => t.Group)
            .Include(t => t.From)
            .ToListAsync();

        return new ResponseService<List<GetTaskCardDto>>
        {
            Data = _tasks.Select(t => new GetTaskCardDto
            {
                Paused = t.Pause,
                Attention = t.Attention,
                Flagged = t.Flagged,
                From = t.From is null ? "" : t.From.Name,
                Id = t.Id,
                IsReview = t.IsReview,
                IsRollback = t.IsRollback,
                LearningObjective = new BasicInfoDto
                {
                    Id = t.LearningObjective.Id,
                    Name = t.LearningObjective.Name
                },
                Name = t.Name,
                Priority = t.Priority,
                RollbackCount = t.RollbackCount,
                Status = t.Status,
                User = t.User is null
                    ? null
                    : new BasicInfoDto
                    {
                        Name = t.User.Name,
                        Id = t.User.Id
                    },
                baseDuration = t.Duration,
                duration = (decimal?)_context.TaskWorkTimes
                    .Where(q => q.TaskId == t.Id)
                    .Sum(q => q.Duration) / 60000
            }).ToList(),
            Error = false,
            Message = "List of available tasks"
        };
    }

    public async Task<ActionResult<ResponseService<GetTaskAssignmentDto>>> GetTaskAssignment(int id)
    {
        var task = await _context.Tasks
            .Where(t => t.Id == id && !t.Archived)
            .Include(t => t.LearningObjective)
            .ThenInclude(lo => lo.Lesson)
            .ThenInclude(l => l.Unit)
            .ThenInclude(u => u.Project)
            .ThenInclude(p => p.Users)
            .Include(t => t.User)
            .FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        var user = task.LearningObjective.Lesson.Unit.Project.Users
            .Where(u => u.Id != task.UserId && u.GroupId == task.GroupId && !u.Archived)
            .ToList();

        return new ResponseService<GetTaskAssignmentDto>
        {
            Data = new GetTaskAssignmentDto
            {
                AssignedUser = task.User is null
                    ? null
                    : new BasicInfoDto { Id = task.User.Id, Name = task.User.Name },
                AssignableUsers = user.Select(u => new BasicInfoDto { Id = u.Id, Name = u.Name })
                    .ToList()
            },
            Error = false,
            Message = "List of all assignable users"
        };
    }

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> RollbackTask(int taskId, int stepId, List<RollbackLogDto> logs, string? clarification)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );

        var task = await _context.Tasks
            .Include(t => t.LearningObjective)
                .ThenInclude(lo => lo.Lesson)
                    .ThenInclude(l => l.Unit)
                        .ThenInclude(u => u.Project)
            .Include(t => t.LearningObjective)
                .ThenInclude(lo => lo.SprintLearningObjectives)
                    .ThenInclude(slo => slo.Sprint)
            .Include(t => t.User)
            .Where(t => t.Id == taskId && !t.Archived)
            .FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );

        // Owner and Project Manager can rollback any task, others can only rollback their own tasks
        if (task.UserId is not null && task.UserId != user.Id && user.Role != UserRoleEnum.Owner && user.Role != UserRoleEnum.ProjectManger)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not assigned for you" }
            );
        if (task.UserId is null)
            task.User = user;

        if (!task.IsReview)
            return new BadRequestObjectResult(
                new Responses.BadRequestsDTO("Task is not in a Reviewable")
            );
        if (task.Status != TaskStatusEnum.Doing)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = false, Message = "Task status should be started" }
            );

        task.Status = TaskStatusEnum.Rollback;

        var taskDuration = await _context.TaskWorkTimes
            .Where(t => t.TaskId == task.Id && t.EndDate == null)
            .FirstOrDefaultAsync();

        if (taskDuration is not null)
        {
            var now = DateTime.Now;
            taskDuration.EndDate = now;
            taskDuration.EndReason = TaskDurationEndReasonEnum.Complete;
            taskDuration.Duration = now.Subtract(taskDuration.StartDate).TotalMilliseconds;
        }

        var rollbackStep = await _context.Steps
            .Where(s => s.Id == stepId && !s.Archived)
            .Include(s => s.Node)
            .Include(s => s.TaskBank)
            .ThenInclude(tb => tb.Group)
            .FirstOrDefaultAsync();
        if (rollbackStep is null)
            return new BadRequestObjectResult(new Responses.BadRequestsDTO("Step is not found."));

        var foundTask = await _context.Tasks
            .Include(t => t.User)
            .Where(
                t =>
                    t.StepId == rollbackStep.Id
                    && t.LearningObjectiveId == task.LearningObjectiveId
                    && !t.Archived
            )
            .OrderBy(t => t.Id)
            .LastOrDefaultAsync();

        // Track target task info for notification
        Models.Task targetTask;

        if (foundTask is not null)
        {
            targetTask = foundTask;
            foundTask.From = task;
            foundTask.FromId = task.FromId;
            foundTask.RollbackCount++;
            foundTask.IsRollback = true;
            foundTask.Status = TaskStatusEnum.ToDo;

            var newActivity = new TaskActivity
            {
                ActorOne = user,
                ActorOneId = user.Id,
                Type = TaskActivityTypeEnum.Status_Rollback,
                Task = task,
                TaskId = task.Id,
                TaskSecondary = foundTask,
                TaskSecondaryId = foundTask.Id,
                AdditionalInfo = null,
                TimeStamp = DateTime.Now,
                ActorTwo = null,
                ActorTwoId = null
            };

            var newActivity2 = new TaskActivity
            {
                ActorOne = user,
                ActorOneId = user.Id,
                Type = TaskActivityTypeEnum.Rollback,
                TaskSecondary = task,
                TaskSecondaryId = task.Id,
                Task = foundTask,
                TaskId = foundTask.Id,
                AdditionalInfo = null,
                TimeStamp = DateTime.Now,
                ActorTwo = null,
                ActorTwoId = null
            };

            var RollbackLog = await _rollbackService.CreateRollback(
                FromTaskId: task.Id,
                ToTaskId: foundTask.Id,
                user: user,
                Clarification: clarification,
                logs: logs
            );

            if (RollbackLog.Error)
                return new BadRequestObjectResult(RollbackLog);

            _context.TaskActivities.Add(newActivity);
            _context.TaskActivities.Add(newActivity2);
        }
        else
        {
            var newTask = new Models.Task
            {
                Group = rollbackStep.TaskBank.Group,
                GroupId = rollbackStep.TaskBank.GroupId,
                IsReview = rollbackStep.TaskBank.Type == TaskBankTypeEnum.Review,
                LearningObjective = task.LearningObjective,
                LearningObjectiveId = task.LearningObjectiveId,
                Name = rollbackStep.TaskBank.Name,
                Step = rollbackStep,
                StepId = rollbackStep.Id,
                TL = rollbackStep.TaskBank.TL,
                Archived = false,
                Attention = false,
                CreatedAt = DateTime.Now,
                Flagged = false,
                From = task,
                FromId = task.Id,
                IsRollback = true,
                Pause = false,
                Priority = rollbackStep.Priority,
                RollbackCount = 1,
                Status = rollbackStep.TaskBank.TL ? TaskStatusEnum.ToDo : TaskStatusEnum.Backlog,
            };
            targetTask = newTask;
            _context.Tasks.Add(newTask);

            var newActivity = new TaskActivity
            {
                ActorOne = user,
                ActorOneId = user.Id,
                Type = TaskActivityTypeEnum.Status_Rollback,
                Task = task,
                TaskId = task.Id,
                TaskSecondary = newTask,
                TaskSecondaryId = newTask.Id,
                AdditionalInfo = null,
                TimeStamp = DateTime.Now,
                ActorTwo = null,
                ActorTwoId = null
            };

            var newActivity2 = new TaskActivity
            {
                ActorOne = user,
                ActorOneId = user.Id,
                Type = TaskActivityTypeEnum.Rollback,
                TaskSecondary = task,
                TaskSecondaryId = task.Id,
                Task = newTask,
                TaskId = newTask.Id,
                AdditionalInfo = null,
                TimeStamp = DateTime.Now,
                ActorTwo = null,
                ActorTwoId = null
            };

            var RollbackLog = await _rollbackService.CreateRollback(
                FromTaskId: task.Id,
                ToTaskId: newTask.Id,
                user: user,
                Clarification: null,
                logs: logs
            );

            if (RollbackLog.Error)
                return new BadRequestObjectResult(RollbackLog);

            _context.TaskActivities.Add(newActivity);
            _context.TaskActivities.Add(newActivity2);
        }

        await _context.SaveChangesAsync();

        // Send rollback notification
        var project = task.LearningObjective.Lesson.Unit.Project;
        var rollbackNotification = new RollBackNotificationDto
        {
            FromUserId = user.Id,
            FromUserName = user.Name,
            ToUserId = targetTask.UserId ?? 0,
            ToUserName = targetTask.User?.Name ?? "Unassigned",
            TeamLeaderId = foundTask.User?.TeamleaderId ?? 0,
            FromTaskId = task.Id,
            FromTaskName = task.Name,
            ToTaskId = targetTask.Id,
            ToTaskName = targetTask.Name,
            LoName = task.LearningObjective.Name,
            ProjectId = project.Id,
            ProjectName = project.Name,
            SprintName = task.LearningObjective.SprintLearningObjectives?.FirstOrDefault()?.Sprint?.Name ?? "",
            RollbackCount = foundTask.RollbackCount
        };

        // Fire and forget - don't block the response on notification
        _ = await _notificationService.NotifyMemberOfRollBack(rollbackNotification, critical: foundTask.RollbackCount > 1);

        return await GetTaskDetails(task.Id);
    }

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> GetTaskDetails(int id) =>
        await getTaskDetails(id);

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> ToggleFlag(int id)
    {
        var task = await _context.Tasks
            .Where(t => !t.Archived && t.Id == id)
            .Include(t => t.Group)
            .Include(t => t.LearningObjective)
            .Include(t => t.User)
            .FirstOrDefaultAsync();
        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        var user = await _authService.GetAuthedUser();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );

        if (task.Flagged)
        {
            task.Flagged = false;
            task.Attention = true;

            var newActivity = new TaskActivity
            {
                Type = TaskActivityTypeEnum.Unflag,
                Task = task,
                TaskId = task.Id,
                ActorOne = user,
                ActorOneId = user.Id,
                ActorTwo = null,
                ActorTwoId = null,
                TimeStamp = DateTime.Now,
                AdditionalInfo = null,
                TaskSecondary = null,
                TaskSecondaryId = null
            };

            _context.TaskActivities.Add(newActivity);
        }
        else
        {
            task.Flagged = true;

            task.Status = TaskStatusEnum.ToDo;

            var taskDuration = await _context.TaskWorkTimes
                .Where(d => d.TaskId == task.Id && d.EndDate == null)
                .FirstOrDefaultAsync();

            if (taskDuration is not null)
            {
                var now = DateTime.Now;
                taskDuration.EndDate = now;
                taskDuration.EndReason = TaskDurationEndReasonEnum.Flag;
                taskDuration.Duration = now.Subtract(taskDuration.StartDate).TotalMilliseconds;
            }

            var newActivity = new TaskActivity
            {
                Type = TaskActivityTypeEnum.Flag,
                Task = task,
                TaskId = task.Id,
                ActorOne = user,
                ActorOneId = user.Id,
                ActorTwo = null,
                ActorTwoId = null,
                TimeStamp = DateTime.Now,
                AdditionalInfo = null,
                TaskSecondary = null,
                TaskSecondaryId = null
            };

            _context.TaskActivities.Add(newActivity);
        }

        await _context.SaveChangesAsync();

        return await getTaskDetails(task.Id);
    }

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> TogglePause(int id)
    {
        var task = await _context.Tasks.Where(t => !t.Archived && t.Id == id).FirstOrDefaultAsync();
        if (task == null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = false, Message = "Task is not found" }
            );

        if (!task.Pause && task.Status != TaskStatusEnum.Doing)
            return new BadRequestObjectResult(
                new BaseResponseService
                {
                    Error = false,
                    Message = "Task with a status other than 'Doing' cannot be pause"
                }
            );
        else if (task.Pause && task.Status != TaskStatusEnum.ToDo)
            return new BadRequestObjectResult(
                new BaseResponseService
                {
                    Error = false,
                    Message = "Task cannot be resumed if status is not 'To Do'"
                }
            );

        var user = await _authService.GetAuthedUser();
        if (user is null)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid user Auth." }
            );

        if (task.Pause)
        {
            if (task.UserId is not null && user.Id == task.UserId)
            {
                task.Status = TaskStatusEnum.Doing;

                var taskDuration = new TaskWorkTime
                {
                    EndDate = null,
                    StartDate = DateTime.Now,
                    TaskId = task.Id,
                    Duration = 0,
                    EndReason = null,
                    UserId = user.Id,
                    User = user,
                    Task = task
                };

                _context.TaskWorkTimes.Add(taskDuration);
            }
            task.Pause = false;

            var newActivity = new TaskActivity
            {
                Type = TaskActivityTypeEnum.Resume,
                Task = task,
                TaskId = task.Id,
                ActorOne = user,
                ActorOneId = user.Id,
                ActorTwo = null,
                ActorTwoId = null,
                TimeStamp = DateTime.Now,
                AdditionalInfo = null,
                TaskSecondary = null,
                TaskSecondaryId = null
            };

            _context.TaskActivities.Add(newActivity);
        }
        else
        {
            task.Status = TaskStatusEnum.ToDo;

            var taskDuration = await _context.TaskWorkTimes
                .Where(d => d.TaskId == task.Id && d.EndDate == null)
                .FirstOrDefaultAsync();

            if (taskDuration is not null)
            {
                var now = DateTime.Now;
                taskDuration.EndDate = now;
                taskDuration.EndReason = TaskDurationEndReasonEnum.Pause;
                taskDuration.Duration = now.Subtract(taskDuration.StartDate).TotalMilliseconds;
            }

            var newActivity = new TaskActivity
            {
                Type = TaskActivityTypeEnum.Pause,
                Task = task,
                TaskId = task.Id,
                ActorOne = user,
                ActorOneId = user.Id,
                ActorTwo = null,
                ActorTwoId = null,
                TimeStamp = DateTime.Now,
                AdditionalInfo = null,
                TaskSecondary = null,
                TaskSecondaryId = null
            };

            _context.TaskActivities.Add(newActivity);
            task.Pause = true;
        }

        await _context.SaveChangesAsync();

        return await getTaskDetails(task.Id);
    }

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> UpdateTaskPriority(int TaskId, TaskPriorityEnum Priority)
    {
        var user = await _authService.GetAuthedUser();

        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth." }
            );

        var task = await _context.Tasks
            .Where(t => t.Id == TaskId && !t.Archived)
            .FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found." }
            );

        task.Priority = Priority;

        var newActivity = new TaskActivity
        {
            Type = TaskActivityTypeEnum.PriorityChange,
            Task = task,
            TaskId = task.Id,
            ActorOne = user,
            ActorOneId = user.Id,
            ActorTwo = null,
            ActorTwoId = null,
            TimeStamp = DateTime.Now,
            AdditionalInfo =
                Priority == TaskPriorityEnum.High
                    ? "High"
                    : (
                        Priority == TaskPriorityEnum.Medium
                            ? "Medium"
                            : (Priority == TaskPriorityEnum.Low ? "Low" : "None")
                    ),
            TaskSecondary = null,
            TaskSecondaryId = null
        };

        _context.TaskActivities.Add(newActivity);

        await _context.SaveChangesAsync();

        return await getTaskDetails(task.Id);
    }

    private async Task<Models.Task> createTask(TaskBank taskBank, LearningObjective learningObjective) => await createTask(taskBank, learningObjective, null);

	private async Task<Models.Task> createTask(TaskBank taskBank, LearningObjective learningObjective, User? user, int? sprintId = null)
	    {
	        var authedUser = await _authService.GetAuthedUser();

	        var newTask = new Models.Task
        {
            Step = null,
            StepId = null,
            LearningObjective = learningObjective,
            LearningObjectiveId = learningObjective.Id,
            TL = taskBank.TL,
            From = null,
            FromId = null,
            Name = taskBank.Name,
            User = user is null ? null : user,
            UserId = user is null ? null : user.Id,
            Group = taskBank.Group,
            GroupId = taskBank.GroupId,
            Pause = false,
            Status = taskBank.TL ? TaskStatusEnum.ToDo : TaskStatusEnum.Backlog,
            Flagged = false,
            Archived = false,
            IsReview = taskBank.Type == TaskBankTypeEnum.Review,
            Attention = false,
            CreatedAt = DateTime.Now,
            RollbackCount = 0,
            IsRollback = false,
            Duration = taskBank.Duration,
            Priority = TaskPriorityEnum.None,
        };

        var createdAct = new TaskActivity
        {
            Task = newTask,
            TaskId = newTask.Id,
            AdditionalInfo = null,
            TaskSecondaryId = null,
            TaskSecondary = null,
            ActorTwoId = null,
            ActorTwo = null,
            ActorOneId = authedUser is null ? null : authedUser.Id,
            ActorOne = authedUser,
            TimeStamp = DateTime.Now,
            Type = TaskActivityTypeEnum.Created,
        };

	        _context.Tasks.Add(newTask);
	        _context.TaskActivities.Add(createdAct);
	        await _context.SaveChangesAsync();

	        if (user is not null)
	        {
	            await _notificationService.NotifyUserOfTaskAssignment(user.Id, newTask.Id, authedUser?.Id);
	        }
	        else if (newTask.Status == TaskStatusEnum.Backlog)
	        {
	            // Notify team leader when task is created in backlog status without an assigned user
	            await _notificationService.NotifyTeamLeaderOfBacklogTask(newTask.Id, newTask.GroupId, "New task created");
	        }

	        return newTask;
    }

    private async Task<Models.Task> createTask(Step step, LearningObjective learningObjective, Models.Task? from)
    {
        var newTask = new Models.Task
        {
            Priority = step.Priority,
            Step = step,
            StepId = step.Id,
            LearningObjective = learningObjective,
            LearningObjectiveId = learningObjective.Id,
            TL = step.TaskBank.TL,
            From = from,
            FromId = from is null ? null : from.Id,
            Name = step.TaskBank.Name,
            User = null,
            UserId = null,
            Group = step.TaskBank.Group,
            GroupId = step.TaskBank.GroupId,
            Pause = false,
            Status = step.TaskBank.TL ? TaskStatusEnum.ToDo : TaskStatusEnum.Backlog,
            Flagged = false,
            Archived = false,
            IsReview = step.TaskBank.Type == TaskBankTypeEnum.Review,
            Attention = false,
            CreatedAt = DateTime.Now,
            RollbackCount = 0,
            IsRollback = from is null ? false : true
        };

        var createdAct = new TaskActivity
        {
            Task = newTask,
            TaskId = newTask.Id,
            AdditionalInfo = null,
            TaskSecondaryId = null,
            TaskSecondary = null,
            ActorTwoId = null,
            ActorTwo = null,
            ActorOneId = null,
            ActorOne = null,
            TimeStamp = DateTime.Now,
            Type = TaskActivityTypeEnum.Created,
        };

        _context.Tasks.Add(newTask);
        _context.TaskActivities.Add(createdAct);
        await _context.SaveChangesAsync();

        // Notify team leader when task is created in backlog status (unassigned, non-TL tasks)
        if (newTask.Status == TaskStatusEnum.Backlog)
        {
            await _notificationService.NotifyTeamLeaderOfBacklogTask(newTask.Id, newTask.GroupId, "New workflow task created");
        }

        return newTask;
    }

    // private async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> getTaskDetails(int id)
    // No change to signature, but modify the Includes:
    private async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> getTaskDetails(int id)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );

        var task = await _context.Tasks
            .Where(t => !t.Archived && t.Id == id)
            .Include(t => t.User)
            .Include(t => t.LearningObjective)
                .ThenInclude(lo => lo.Schema) // Existing include
            .Include(t => t.LearningObjective)
                .ThenInclude(lo => lo.Comments) // Existing include
                    .ThenInclude(c => c.User) // Existing include
            .Include(t => t.LearningObjective)
                .ThenInclude(lo => lo.Comments) // Existing include
                    .ThenInclude(c => c.Child) // Existing include
            .Include(t => t.LearningObjective) // **NEW: Include SprintLearningObjectives and Sprint**
                .ThenInclude(lo => lo.SprintLearningObjectives)
                    .ThenInclude(slo => slo.Sprint) // Eagerly load Sprint data
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        var activities = await _context.TaskActivities
            .Where(a => a.TaskId == task.Id)
            .Include(a => a.ActorOne)
            .Include(a => a.ActorTwo)
            .Include(a => a.TaskSecondary)
            .ToListAsync();

        var durations = await _context.TaskWorkTimes.Where(d => d.TaskId == task.Id).ToListAsync();

        var started = await _context.TaskActivities
            .Where(a => a.TaskId == task.Id && a.Type == TaskActivityTypeEnum.Status_Doing)
            .OrderBy(a => a.TimeStamp)
            .LastOrDefaultAsync();

        var done = await _context.TaskActivities
            .Where(
                a =>
                    a.TaskId == task.Id
                    && (
                        a.Type == TaskActivityTypeEnum.Status_Done
                        || a.Type == TaskActivityTypeEnum.Status_Rollback
                    )
            )
            .OrderBy(a => a.TimeStamp)
            .LastOrDefaultAsync();

        var access = TaskAccess.None;

        if (task.Status != TaskStatusEnum.Done && task.Status != TaskStatusEnum.Rollback)
        {
            if (user.Role == UserRoleEnum.ProjectManger || user.Role == UserRoleEnum.Owner)
            {
                if (
                    task.Status == TaskStatusEnum.Backlog
                    || task.UserId == user.Id
                    || task.UserId == null
                )
                    access = TaskAccess.WorkOnAndManage;
                else
                    access = TaskAccess.Manage;
            }
            else if (user.Role == UserRoleEnum.SectionHead)
            {
                if (
                    user.GroupId == task.GroupId
                    && (
                        task.UserId == user.Id
                        || task.Status == TaskStatusEnum.Backlog
                        || task.UserId == null
                    )
                )
                    access = TaskAccess.WorkOnAndManage;
                else if (task.UserId != user.Id && task.GroupId == user.GroupId)
                    access = TaskAccess.Manage;
                else
                {
                    var section = await _context.Sections
                        .Where(s => s.HeadId == user.Id)
                        .Include(s => s.SectionGroups)
                        .ThenInclude(s => s.Group)
                        .FirstOrDefaultAsync();

                    if (section is not null && section.SectionGroups.Any(g => g.GroupId == task.GroupId))
                    {
                        if (
                            task.UserId == user.Id
                            || task.Status == TaskStatusEnum.Backlog
                            || task.UserId == null
                        )
                            access = TaskAccess.WorkOnAndManage;
                        else
                            access = TaskAccess.Manage;
                    }
                }
            }
            else if (user.Role == UserRoleEnum.TeamLeader)
            {
                if (task.GroupId == user.GroupId)
                {
                    if (
                        task.UserId == user.Id
                        || task.Status == TaskStatusEnum.Backlog
                        || task.UserId == null
                    )
                        access = TaskAccess.WorkOnAndManage;
                    else
                        access = TaskAccess.Manage;
                }
            }
            else if (user.Role == UserRoleEnum.Member)
                if (task.GroupId == user.GroupId)
                    if (task.UserId == user.Id || task.Status == TaskStatusEnum.Backlog)
                        access = TaskAccess.WorkOn;
        }

        double duration = 0;

        foreach (var d in durations)
            duration += d.Duration;

        var issuesRecieved = task.IsReview
            ? 0
            : (await _context.Rollbacks.Where(rb => rb.ToTaskId == task.Id).ToListAsync()).Count;
        var issuesCreated = task.IsReview
            ? (await _context.Rollbacks.Where(rb => rb.TaskId == task.Id).ToListAsync()).Count
            : 0;
        var notes = task.IsReview
            ? 0
            : (
                await _context.RollbackIssues
                    .Where(
                        rb =>
                            rb.StepId == task.StepId
                            && task.LearningObjectiveId == rb.Rollback.Task.LearningObjectiveId
                    )
                    .Include(rb => rb.Rollback)
                    .ThenInclude(r => r.Task)
                    .ToListAsync()
            ).Count;

        return new ResponseService<GetTaskDetailsDto>
        {
            Error = false,
            Data = new GetTaskDetailsDto
            {
                CreatedAt = task.CreatedAt,
                Comments = task.LearningObjective.Comments
                    .Where(c => c.Child == null)
                    .OrderByDescending(c => c.Timestamp)
                    .Select(c =>
                        c.Archived
                            ? new TaskCommentDto
                            {
                                Id = c.Id,
                                Timestamp = c.Timestamp,
                                User = new BasicInfoDto { Id = c.User.Id, Name = c.User.Name },
                                IsDeleted = true
                            }
                            : new TaskCommentDto
                            {
                                Content = c.Content,
                                Id = c.Id,
                                Timestamp = c.Timestamp,
                                User = new BasicInfoDto { Id = c.User.Id, Name = c.User.Name },
                                IsEdited = task.LearningObjective.Comments.Any(_ => _.ChildId == c.Id)
                            }
                    ).ToList(),
                User = task.User != null
                    ? new BasicInfoDto { Id = task.User.Id, Name = task.User.Name }
                    : null,
                DoneAt = done != null &&
                         (task.Status != TaskStatusEnum.Rollback || task.Status == TaskStatusEnum.Done)
                    ? done.TimeStamp
                    : null,
                Environment = task.LearningObjective.Environment,
                Flagged = task.Flagged,
                Id = task.Id,
                IsReview = task.IsReview,
                LearningObjective = new BasicInfoDto
                {
                    Id = task.LearningObjective.Id,
                    Name = task.LearningObjective.Name
                },
                Name = task.Name,
                Pause = task.Pause,
                Priority = task.Priority,
                Schema = new BasicInfoDto
                {
                    Id = task.LearningObjective.Schema.Id,
                    Name = task.LearningObjective.Schema.Name
                },
                StartedAt = started != null &&
                            (task.Status == TaskStatusEnum.Doing ||
                             task.Status != TaskStatusEnum.Rollback ||
                             task.Status == TaskStatusEnum.Done)
                    ? started.TimeStamp
                    : null,
                Status = task.Status,
                Tag = task.LearningObjective.Tag,
                Template = task.LearningObjective.Template,
                Access = access,
                Activities = activities
                    .Select(a => new GetTaskActivity
                    {
                        Id = a.Id,
                        AdditionalInfo = a.AdditionalInfo,
                        ActorOne = a.ActorOne != null
                            ? new BasicInfoDto { Id = a.ActorOne.Id, Name = a.ActorOne.Name }
                            : null,
                        ActorTwo = a.ActorTwo != null
                            ? new BasicInfoDto { Id = a.ActorTwo.Id, Name = a.ActorTwo.Name }
                            : null,
                        TimeStamp = a.TimeStamp,
                        Type = a.Type,
                        SecondaryTask = a.TaskSecondary != null
                            ? new BasicInfoDto { Id = a.TaskSecondary.Id, Name = a.TaskSecondary.Name }
                            : null
                    })
                    .OrderByDescending(a => a.TimeStamp)
                    .ToList(),
                Duration = duration,
                IssuesCreated = issuesCreated,
                IssuesRecieved = issuesRecieved,
                Notes = notes,
                BaseDuration = task.Duration
            },
            Message = "Task found"
        };
    }


    /// <summary>
    /// Retrieves detailed information for a specific task, with its start and end dates
    /// adjusted to fit within the boundaries of a *specified* sprint associated with its Learning Objective.
    /// </summary>
    /// <param name="taskId">The ID of the task to retrieve.</param>
    /// <param name="sprintId">The ID of the specific sprint to adjust the dates against.</param>
    /// <returns>A ResponseService containing a GetTaskDetailsDto with adjusted dates if successful, otherwise an error response.</returns>
    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> GetTaskDetailsAdjustedForSprint(int taskId, int sprintId)
    {
        var response = await getTaskDetails(taskId);

        // Check if the base task retrieval was successful
        if (response.Result is ObjectResult objectResult && objectResult.Value is BaseResponseService baseResponse && baseResponse.Error)
        {
            // Propagate error from getTaskDetails if task not found or unauthorized
            return response;
        }

        // Safely cast to the expected DTO type
        var taskDetailsDto = response.Value?.Data;
        if (taskDetailsDto == null)
        {
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Failed to retrieve task details or DTO conversion failed." }
            );
        }

        // Now, explicitly load the specified sprint and verify its association with the Learning Objective.
        var relevantSprint = await _context.Sprints
            .Where(s => s.Id == sprintId) // Find the specific sprint
            .Include(s => s.SprintLearningObjectives.Where(slo => slo.LearningObjectiveId == taskDetailsDto.LearningObjective.Id)) // Filter SLOs for *this* LO
            .FirstOrDefaultAsync();

        // If the specified sprint is not found, or the LO is not associated with it, we cannot adjust dates.
        if (relevantSprint == null || !relevantSprint.SprintLearningObjectives.Any(slo => slo.LearningObjectiveId == taskDetailsDto.LearningObjective.Id))
        {
            // Return original DTO as no valid sprint context for adjustment was found.
            return new ResponseService<GetTaskDetailsDto>
            {
                Data = taskDetailsDto,
                Error = false,
                Message = $"Successfully retrieved task details for Task ID: {taskId}. Specified Sprint ID: {sprintId} not found, archived, or not associated with the task's Learning Objective."
            };
        }

        // Define sprint's start and end dates for clipping (date only)
        DateTime sprintEffectiveStart = relevantSprint.StartDate.Date;
        DateTime sprintEffectiveEnd = relevantSprint.EndDate.Date;

        // Get the task's original start and end dates (date only)
        DateTime? taskOriginalStart = taskDetailsDto.StartedAt?.Date;
        DateTime? taskOriginalDone = taskDetailsDto.DoneAt?.Date; // Use a nullable for DoneAt initially

        DateTime effectiveTaskStart;
        DateTime effectiveTaskEnd;

        // Determine the effective start date for the task within the sprint context
        if (taskOriginalStart.HasValue)
        {
            effectiveTaskStart = taskOriginalStart.Value;
            // Clip task's start date to be no earlier than sprint start
            if (effectiveTaskStart < sprintEffectiveStart)
            {
                effectiveTaskStart = sprintEffectiveStart;
            }
            // If task's original start is after sprint end, it effectively starts AT sprint end for this report
            if (effectiveTaskStart > sprintEffectiveEnd)
            {
                effectiveTaskStart = sprintEffectiveEnd;
            }
        }
        else
        {
            // If task never started, for reporting purposes within a sprint:
            // If it's not backlog, assume it effectively starts at sprint start.
            if (taskDetailsDto.Status != TaskStatusEnum.Backlog)
            {
                effectiveTaskStart = sprintEffectiveStart;
            }
            else
            {
                // For backlog tasks that haven't started, they don't have an effective start within a sprint.
                effectiveTaskStart = DateTime.MinValue; // Placeholder for unstarted/backlog
            }
        }

        // Determine the effective end date for the task within the sprint context
        if (taskOriginalDone.HasValue)
        {
            effectiveTaskEnd = taskOriginalDone.Value;
        }
        else if (taskDetailsDto.Status == TaskStatusEnum.Done || taskDetailsDto.Status == TaskStatusEnum.Rollback)
        {
            // If status is Done/Rollback but DoneAt is null, use sprint end as a fallback
            effectiveTaskEnd = sprintEffectiveEnd;
        }
        else
        {
            // For active tasks, the current date is the natural end, then clip.
            effectiveTaskEnd = DateTime.Now.Date;
        }

        // Now, clip the determined effectiveTaskEnd to be no later than sprint end
        if (effectiveTaskEnd > sprintEffectiveEnd)
        {
            effectiveTaskEnd = sprintEffectiveEnd;
        }
        // And no earlier than sprint start (in case task finished before sprint began)
        if (effectiveTaskEnd < sprintEffectiveStart)
        {
            effectiveTaskEnd = sprintEffectiveStart;
        }


        // Finally, ensure the effective start is not after the effective end.
        // This is the logical consistency check, applied after all clipping.
        if (effectiveTaskStart != DateTime.MinValue && effectiveTaskStart > effectiveTaskEnd)
        {
            effectiveTaskEnd = effectiveTaskStart;
        }


        // Apply the adjusted dates to the DTO

        // For StartedAt:
        if (taskDetailsDto.Status != TaskStatusEnum.Backlog)
        {
            taskDetailsDto.StartedAt = effectiveTaskStart;
        }
        else
        {
            taskDetailsDto.StartedAt = null; // Explicitly null for backlog if no StartedAt
        }

        // New logic for DoneAt: Only assign if status is TaskStatusEnum.Done, otherwise null.
        if (taskDetailsDto.Status == TaskStatusEnum.Done)
        {
            taskDetailsDto.DoneAt = effectiveTaskEnd;
        }
        else
        {
            taskDetailsDto.DoneAt = null; // For any other status (InProgress, Todo, Rollback, Backlog, etc.), DoneAt is null.
        }

        return new ResponseService<GetTaskDetailsDto>
        {
            Data = taskDetailsDto,
            Error = false,
            Message = $"Successfully retrieved task details for Task ID: {taskId}, adjusted for Sprint ID: {sprintId}."
        };
    }


    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> ProceedTask(int taskId)
    {
        var task = await _context.Tasks
            .Where(t => !t.Archived && t.Id == taskId)
            .Include(t => t.User)
            .Include(t => t.LearningObjective)
            .Include(t => t.Step)
            .Include(t => t.From)
            .FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        var user = await _authService.GetAuthedUser();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );

        if (task.Status == TaskStatusEnum.Backlog)
        {
            task.User = user;
            task.UserId = user.Id;
            task.Status = TaskStatusEnum.ToDo;

            var newActivity = new TaskActivity
            {
                TaskId = task.Id,
                Task = task,
                ActorOne = user,
                ActorOneId = user.Id,
                Type = TaskActivityTypeEnum.Status_ToDo,
                AdditionalInfo = null,
                TaskSecondary = null,
                TaskSecondaryId = null,
                ActorTwo = null,
                ActorTwoId = null,
                TimeStamp = DateTime.Now
            };

            _context.TaskActivities.Add(newActivity);

            await _context.SaveChangesAsync();

            return await GetTaskDetails(task.Id);
        }
        else if (task.Status == TaskStatusEnum.ToDo)
        {
            if (task.Attention)
                task.Attention = false;

            if (task.User is not null && user.Id != task.User.Id)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Unauthorized" }
                );

            if (task.User is null)
                task.User = user;

            task.Status = TaskStatusEnum.Doing;
            if (task.LearningObjective.StartedAt is null)
                task.LearningObjective.StartedAt = DateTime.Now;

            var newDuration = new TaskWorkTime
            {
                Task = task,
                TaskId = task.Id,
                User = user,
                UserId = user.Id,
                EndDate = null,
                StartDate = DateTime.Now,
                EndReason = null,
                Duration = 0
            };

            var newActivity = new TaskActivity
            {
                Task = task,
                TaskId = task.Id,
                Type = TaskActivityTypeEnum.Status_Doing,
                TimeStamp = DateTime.Now,
                ActorOne = user,
                ActorOneId = user.Id,
                ActorTwo = null,
                ActorTwoId = null,
                TaskSecondary = null,
                TaskSecondaryId = null,
                AdditionalInfo = null,
            };

            _context.TaskActivities.Add(newActivity);
            _context.TaskWorkTimes.Add(newDuration);

            await _context.SaveChangesAsync();
            return await GetTaskDetails(task.Id);
        }


        return new BadRequestObjectResult(
            new BaseResponseService { Error = true, Message = "Cannot proceed with task" }
        );
    }

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> CompleteTask(int taskId,bool forceComplete = false)
    {
        var task = await _context.Tasks
            .Where(t => !t.Archived && t.Id == taskId)
            .Include(t => t.User)
            .Include(t => t.LearningObjective)
            .Include(t => t.Step)
            .Include(t => t.From)
            .FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        var user = await _authService.GetAuthedUser();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );
        if (task.Status != TaskStatusEnum.Doing && !forceComplete)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Task may not be completed yet" }
            );
        // Owner and Project Manager can complete any task, others can only complete their own tasks
        if (task.User is not null && user.Id != task.User.Id && user.Role != UserRoleEnum.Owner && user.Role != UserRoleEnum.ProjectManger)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Unauthorized" }
            );
        task.Status = TaskStatusEnum.Done;

        var newActivity = new TaskActivity
        {
            Task = task,
            TaskId = task.Id,
            ActorOne = user,
            ActorOneId = user.Id,
            TaskSecondary = null,
            TaskSecondaryId = null,
            ActorTwo = null,
            ActorTwoId = null,
            AdditionalInfo = null,
            TimeStamp = DateTime.Now,
            Type = TaskActivityTypeEnum.Status_Done
        };

        var taskDuration = await _context.TaskWorkTimes
            .Where(d => d.TaskId == task.Id && d.EndDate == null)
            .FirstOrDefaultAsync();

        if (taskDuration is not null)
        {
            var now = DateTime.Now;
            taskDuration.EndDate = now;
            taskDuration.EndReason = TaskDurationEndReasonEnum.Complete;
            taskDuration.Duration = now.Subtract(taskDuration.StartDate).TotalMilliseconds;
        }

        _context.TaskActivities.Add(newActivity);

        await CreateNext(task);

        task.From = null;
	
	        await _context.SaveChangesAsync();
	
	        // After the task is completed and changes are saved, check if this was the last
	        // remaining task in the associated project. If so, automatically mark the project
	        // as closed and notify the owner that the project has been completed.
	        await TryAutoCompleteProject(task.Id);
	
	        return await GetTaskDetails(task.Id);
    }

	private async System.Threading.Tasks.Task TryAutoCompleteProject(int completedTaskId)
	    {
	        // Find the project this task belongs to via navigation properties
	        var projectId = await _context.Tasks
	            .Where(t => !t.Archived && t.Id == completedTaskId)
	            .Select(t => (int?)t.LearningObjective.Lesson.Unit.ProjectId)
	            .FirstOrDefaultAsync();
	
	        if (!projectId.HasValue)
	            return;
	
	        // Count remaining non-archived tasks in the project that are not Done
	        var remainingTasks = await _context.Tasks
	            .Where(t => !t.Archived
	                        && t.LearningObjective.Lesson.Unit.ProjectId == projectId.Value
	                        && t.Status != TaskStatusEnum.Done)
	            .CountAsync();
	
	        if (remainingTasks > 0)
	            return;
	
	        // All tasks are done for this project — mark the project as Closed if not already
	        var project = await _context.Projects
	            .Where(p => !p.Archived && p.Id == projectId.Value)
	            .FirstOrDefaultAsync();
	
	        if (project is null || project.Status == ProjectStatusEnum.Closed)
	            return;
	
	        project.Status = ProjectStatusEnum.Closed;
	        await _context.SaveChangesAsync();
	
	        await _notificationService.NotifyOwnerOfProjectCompleted(projectId.Value);
	    }

    public async Task<bool> CreateNext(int taskId)
    {
        var task = await _context.Tasks
            .Where(t => t.Id == taskId)
            .Include(t => t.User)
            .Include(t => t.LearningObjective)
            .Include(t => t.Step)
            .Include(t => t.From)
            .FirstOrDefaultAsync();

        if (task is null)
            return false;

        return await CreateNext(task);
    }

    public async Task<bool> CreateNext(Models.Task task)
    {
        if (task.Step is null)
            return false;
        var nextStep = await _context.Steps
            .Where(
                s => s.NodeId == task.Step.NodeId && !s.Archived && s.Order == task.Step.Order + 1
            )
            .Include(s => s.TaskBank)
            .ThenInclude(tb => tb.Group)
            .FirstOrDefaultAsync();

        if (nextStep is not null)
        {
            var foundTasks = await _context.Tasks
                .Where(
                    t =>
                        t.StepId == nextStep.Id
                        && t.LearningObjectiveId == task.LearningObjectiveId
                        && !t.Archived
                )
                .ToListAsync();
            if (foundTasks.Count > 0)
            {
                foundTasks.ForEach(t =>
                {
                    t.Status = TaskStatusEnum.ToDo;

                    var newActivity = new TaskActivity
                    {
                        Task = t,
                        TaskId = t.Id,
                        ActorOne = null,
                        ActorOneId = null,
                        TaskSecondary = null,
                        TaskSecondaryId = null,
                        ActorTwo = null,
                        ActorTwoId = null,
                        AdditionalInfo = null,
                        TimeStamp = DateTime.Now,
                        Type = TaskActivityTypeEnum.Reactivated
                    };
                });
            }
            else
                await createTask(
                    step: nextStep,
                    learningObjective: task.LearningObjective,
                    task.From
                );
        }
        else
        {
            var currentNode = await _context.Nodes
                .Where(n => n.Id == task.Step.NodeId && !n.Archived)
                .Include(n => n.Next)
                .ThenInclude(n => n.Steps)
                .Include(n => n.Next)
                .ThenInclude(n => n.Previous)
                .ThenInclude(n => n.Steps)
                .FirstOrDefaultAsync();

            if (currentNode is null)
                return true;

            if (currentNode.Next.Count == 0)
                task.LearningObjective.DoneAt = DateTime.Now;

            foreach (var nextNode in currentNode.Next)
            {
                var requiredIsComplete = true;
                foreach (var previousNode in nextNode.Previous)
                {
                    var nodeSteps = previousNode.Steps
                        .Where(s => !s.Archived)
                        .ToList()
                        .OrderByDescending(s => s.Order);
                    var lastStep = nodeSteps.FirstOrDefault();
                    if (lastStep is not null)
                    {
                        var lastTask = await _context.Tasks
                            .Where(
                                t =>
                                    t.StepId == lastStep.Id
                                    && t.LearningObjectiveId == task.LearningObjectiveId
                                    && !t.Archived
                            )
                            .ToListAsync();

                        if (lastTask.Count == 0)
                            requiredIsComplete = false;

                        lastTask.ForEach(t =>
                        {
                            if (
                                t.Status != TaskStatusEnum.Done
                                || t.Status == TaskStatusEnum.Rollback
                            )
                                requiredIsComplete = false;
                        });
                    }
                }

                if (requiredIsComplete)
                {
                    var firstStep = await _context.Steps
                        .Where(s => s.NodeId == nextNode.Id && s.Order == 1 && !s.Archived)
                        .Include(s => s.TaskBank)
                        .ThenInclude(tb => tb.Group)
                        .FirstOrDefaultAsync();

                    if (firstStep is null)
                        return false;

                    var foundTasks = await _context.Tasks
                        .Where(
                            t =>
                                t.StepId == firstStep.Id
                                && t.LearningObjectiveId == task.LearningObjectiveId
                                && !t.Archived
                        )
                        .ToListAsync();

                    if (foundTasks.Count > 0)
                        foundTasks.ForEach(t => t.Status = TaskStatusEnum.ToDo);
                    else
                    {
                        await createTask(
                            step: firstStep,
                            learningObjective: task.LearningObjective,
                            task.From
                        );
                    }
                }
            }
        }
        return true;
    }

    public async Task<bool> CreateNextNode(int nodeId, int loId)
    {
        var lo = await _context.LearningObjectives.Where(lo => lo.Id == loId).FirstOrDefaultAsync();

        if (lo is null)
            return false;

        var currentNode = await _context.Nodes
            .Where(n => n.Id == nodeId && !n.Archived)
            .Include(n => n.Next)
            .ThenInclude(n => n.Steps)
            .Include(n => n.Next)
            .ThenInclude(n => n.Previous)
            .ThenInclude(n => n.Steps)
            .FirstOrDefaultAsync();

        if (currentNode is not null)
        {
            if (currentNode.Next.Count == 0)
                lo.DoneAt = DateTime.Now;

            foreach (var nextNode in currentNode.Next)
            {
                var requiredIsComplete = true;
                foreach (var previousNode in nextNode.Previous)
                {
                    var lastStep = previousNode.Steps
                        .Where(s => !s.Archived)
                        .OrderByDescending(s => s.Order)
                        .FirstOrDefault();
                    if (lastStep is not null)
                    {
                        var lastTask = await _context.Tasks
                            .Where(
                                t =>
                                    t.StepId == lastStep.Id
                                    && t.LearningObjectiveId == lo.Id
                                    && !t.Archived
                            )
                            .ToListAsync();

                        if (lastTask.Count == 0)
                            requiredIsComplete = false;

                        lastTask.ForEach(t =>
                        {
                            if (
                                t.Status != TaskStatusEnum.Done
                                || t.Status == TaskStatusEnum.Rollback
                            )
                                requiredIsComplete = false;
                        });
                    }
                }

                if (requiredIsComplete)
                {
                    var firstStep = await _context.Steps
                        .Where(s => s.NodeId == nextNode.Id && s.Order == 1 && !s.Archived)
                        .Include(s => s.TaskBank)
                        .ThenInclude(tb => tb.Group)
                        .FirstOrDefaultAsync();

                    if (firstStep is null)
                        return false;

                    var foundTasks = await _context.Tasks
                        .Where(
                            t =>
                                t.StepId == firstStep.Id
                                && t.LearningObjectiveId == lo.Id
                                && !t.Archived
                        )
                        .ToListAsync();

                    if (foundTasks.Count > 0)
                    {
                        foreach (var t in foundTasks)
                        {
                            t.Status = TaskStatusEnum.Backlog;

                            // Notify based on whether task has an assigned user
                            if (t.UserId.HasValue)
                            {
                                await _notificationService.NotifyUserOfBacklogTask(t.UserId.Value, t.Id, "Workflow progression - task reactivated");
                            }
                            else
                            {
                                await _notificationService.NotifyTeamLeaderOfBacklogTask(t.Id, t.GroupId, "Workflow progression - task reactivated");
                            }
                        }
                    }
                    else
                    {
                        await createTask(step: firstStep, learningObjective: lo, null);
                    }
                }
            }
        }
        return true;
    }

    public async Task<ActionResult<ResponseService<GetCreatableTasksDto>>> CreatableTasks(int projectId)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );

        var project = await _context.Projects
            .Where(p => p.Id == projectId)
            .Include(p => p.Users)
            .ThenInclude(u => u.Group)
            .Include(p => p.Units)
            .ThenInclude(p => p.Lessons)
            .ThenInclude(p => p.LearningObjectives)
            .FirstOrDefaultAsync();

        if (project is null)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Project is not found" }
            );

        var los = project.Units
            .SelectMany(u => u.Lessons)
            .SelectMany(l => l.LearningObjectives)
            .Where(lo => !lo.Archived)
            .OrderBy(lo => lo.Name)
            .Select(lo => new BasicInfoDto { Id = lo.Id, Name = lo.Name })
            .ToList();

        if (user.Role == UserRoleEnum.TeamLeader)
        {
            var _taskBankItems = await _context.TaskBank
                .Include(tb => tb.Group)
                .Where(tb => tb.Active && tb.GroupId == user.GroupId)
                .ToListAsync();

            _taskBankItems.Sort((a, b) => string.Compare(a.Name.ToLower(), b.Name.ToLower()));

            return new ResponseService<GetCreatableTasksDto>
            {
                Error = false,
                Message = "Creatable tasks List",
                Data = new GetCreatableTasksDto
                {
                    LearningObjectives = los,
                    Assignees = project.Users
                        .Where(u => !u.Archived && u.GroupId == user.GroupId)
                        .Select(i => new UserDto
                        {
                            Name = i.Name,
                            Id = i.Id,
                            Group = new BasicInfoDto
                            {
                                Name = i.Group.Name,
                                Id = i.Group.Id
                            }
                        })
                        .ToList(),
                    Options = _taskBankItems
                        .Select(tb => new TaskOption
                        {
                            Name = tb.Name,
                            Id = tb.Id,
                            TeamLead = tb.TL,
                            Group = new BasicInfoDto
                            {
                                Id = tb.Group.Id,
                                Name = tb.Group.Name
                            }
                        })
                        .ToList()
                }
            };
        }

        if (user.Role == UserRoleEnum.SectionHead)
        {
            var sectionGroups = await _context.SectionGroups
                .Where(sg => sg.Section.HeadId == user.Id)
                .Select(sg => sg.Group)
                .ToListAsync();

            var _taskBankItems = await _context.TaskBank
                .Include(tb => tb.Group)
                .Where(tb => tb.Active && sectionGroups.Contains(tb.Group))
                .ToListAsync();

            _taskBankItems.Sort((a, b) => string.Compare(a.Name.ToLower(), b.Name.ToLower()));

            return new ResponseService<GetCreatableTasksDto>
            {
                Error = false,
                Message = "Creatable tasks List",
                Data = new GetCreatableTasksDto
                {
                    LearningObjectives = los,
                    Assignees = project.Users
                        .Where(u => !u.Archived && u.GroupId == user.GroupId)
                        .Select(i => new UserDto
                        {
                            Name = i.Name,
                            Id = i.Id,
                            Group = new BasicInfoDto
                            {
                                Name = i.Group.Name,
                                Id = i.Group.Id
                            }
                        })
                        .ToList(),
                    Options = _taskBankItems
                        .Select(tb => new TaskOption
                        {
                            Name = tb.Name,
                            Id = tb.Id,
                            TeamLead = tb.TL,
                            Group = new BasicInfoDto
                            {
                                Id = tb.Group.Id,
                                Name = tb.Group.Name
                            }
                        })
                        .ToList()
                }
            };
        }

        var taskBankItems = await _context.TaskBank
            .Include(tb => tb.Group)
            .Where(tb => tb.Active)
            .ToListAsync();

        taskBankItems.Sort((a, b) => string.Compare(a.Name.ToLower(), b.Name.ToLower()));

        return new ResponseService<GetCreatableTasksDto>
        {
            Error = false,
            Message = "Creatable tasks List",
            Data = new GetCreatableTasksDto
            {
                LearningObjectives = los,
                Assignees = project.Users
                    .Where(u => !u.Archived)
                    .Select(i => new UserDto
                    {
                        Name = i.Name,
                        Id = i.Id,
                        Group = new BasicInfoDto
                        {
                            Name = i.Group.Name,
                            Id = i.Group.Id
                        }
                    })
                    .ToList(),
                Options = taskBankItems
                    .Select(tb => new TaskOption
                    {
                        Name = tb.Name,
                        Id = tb.Id,
                        TeamLead = tb.TL,
                        Group = new BasicInfoDto
                        {
                            Id = tb.Group.Id,
                            Name = tb.Group.Name
                        }
                    })
                    .ToList()
            }
        };
    }

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> SkipTask(int id)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );

        var task = await _context.Tasks
            .Where(t => t.Id == id)
            .Include(t => t.LearningObjective)
            .ThenInclude(t => t.Schema)
            .Include(t => t.LearningObjective)
            .ThenInclude(t => t.Comments)
            .ThenInclude(t => t.User)
            .FirstOrDefaultAsync();

        if (task is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        if (task.Status == TaskStatusEnum.Doing)
        {
            var taskDuration = await _context.TaskWorkTimes
                .Where(d => d.TaskId == task.Id && d.EndDate == null)
                .FirstOrDefaultAsync();

            if (taskDuration is not null)
            {
                var now = DateTime.Now;
                taskDuration.EndDate = now;
                taskDuration.EndReason = TaskDurationEndReasonEnum.Skip;
                taskDuration.Duration = now.Subtract(taskDuration.StartDate).TotalMilliseconds;
            }
        }

        task.Status = TaskStatusEnum.Done;
        if (task.Pause)
            task.Pause = false;

        var newActivity = new TaskActivity
        {
            Task = task,
            TaskId = task.Id,
            Type = TaskActivityTypeEnum.Skip,
            TimeStamp = DateTime.Now,
            ActorOne = user,
            ActorOneId = user.Id,
            ActorTwo = null,
            ActorTwoId = null,
            TaskSecondary = null,
            TaskSecondaryId = null,
            AdditionalInfo = null,
        };
        _context.TaskActivities.Add(newActivity);

        await CreateNext(task.Id);

        await _context.SaveChangesAsync();

        return await GetTaskDetails(id);
    }

    public async Task<ActionResult<ResponseService<List<GetNodeAheadDto>>>> GetSchemaSteps(int id)
    {
        var task = await _context.Tasks
            .Where(t => !t.Archived && t.Id == id)
            .Include(t => t.LearningObjective)
            .FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        var schema = await _context.Schemas
            .Where(s => s.Id == task.LearningObjective.SchemaId && !s.Archived)
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Previous)
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Next)
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Steps)
            .ThenInclude(s => s.TaskBank)
            .ThenInclude(s => s.Group)
            .FirstOrDefaultAsync();

        if (schema is null)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Schema is not active" }
            );

        var nodesRes = new List<GetNodeAheadDto> { };

        var nodesAhead = (
            from node in schema.Nodes
            where !node.Archived
            orderby node.Order
            select node
        ).ToList();

        var tasks = await _context.Tasks
            .Where(t => t.LearningObjectiveId == task.LearningObjectiveId && !t.Archived)
            .Include(t => t.Step)
            .ToListAsync();

        foreach (var node in nodesAhead)
        {
            var nodeTasks = tasks
                .Where(t => t.Step is not null && t.Step.NodeId == node.Id)
                .ToList();

            nodesRes.Add(
                new GetNodeAheadDto
                {
                    PreviousNodes = node.Previous
                        .Where(n => !n.Archived)
                        .Select(s => new BasicInfoDto { Id = s.Id, Name = s.Name })
                        .ToList(),
                    NextNodes = node.Next
                        .Where(n => !n.Archived)
                        .Select(s => new BasicInfoDto { Id = s.Id, Name = s.Name })
                        .ToList(),
                    Id = node.Id,
                    Name = node.Name,
                    Order = node.Order,
                    IsComplete =
                        nodeTasks.All(t => t.Status == TaskStatusEnum.Done)
                        && nodeTasks.Count == node.Steps.Where(s => !s.Archived).Count(),
                    Steps = node.Steps
                        .Where(s => !s.Archived)
                        .OrderBy(s => s.Order)
                        .Select(s =>
                        {
                            return new GetStepAheadDto
                            {
                                Id = s.Id,
                                Name = s.TaskBank.Name,
                                Group = new BasicInfoDto
                                {
                                    Name = s.TaskBank.Group.Name,
                                    Id = s.TaskBank.Group.Id
                                },
                                IsComplete = nodeTasks.Any(
                                    t => t.StepId == s.Id && t.Status == TaskStatusEnum.Done
                                )
                            };
                        })
                        .ToList(),
                }
            );
        }

        return new ResponseService<List<GetNodeAheadDto>>
        {
            Data = nodesRes,
            Message = "List of All Nodes"
        };
    }

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> JumpTask(int id, List<PutJumpedTaskDto> options)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null || (user.Role != UserRoleEnum.ProjectManger && user.Role != UserRoleEnum.Owner))
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );

        var task = await _context.Tasks
            .Where(t => t.Id == id && !t.Archived)
            .Include(t => t.LearningObjective)
            .ThenInclude(lo => lo.Tasks)
            .FirstOrDefaultAsync();
        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        if (options.Count == 0)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Please provide jump points" }
            );

        var schema = await _context.Schemas
            .Where(s => !s.Archived && s.Id == task.LearningObjective.SchemaId)
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Next)
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Previous)
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Steps)
            .ThenInclude(s => s.TaskBank)
            .ThenInclude(tb => tb.Group)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (schema is null)
            throw new Exception("Schema is not found for the found node");

        var nodes = schema.Nodes
            .Where(n => !n.Archived && options.Select(o => o.NodeId).ToArray().Contains(n.Id))
            .ToList();

        if (nodes.Count() != options.Count || !nodes.All(n => nodes.First().SchemaId == n.SchemaId))
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Nodes Selected" }
            );

        var steps = new List<Step> { };

        foreach (var option in options)
        {
            var node = nodes.Where(n => n.Id == option.NodeId).FirstOrDefault();
            if (node is not null)
            {
                var step = node.Steps
                    .Where(s => !s.Archived && option.StepId == s.Id)
                    .FirstOrDefault();

                if (step is not null)
                    steps.Add(step);
            }
        }

        if (steps.Count() != options.Count())
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Steps Selected" }
            );

        List<NodeWithRevDepth> depths = new List<NodeWithRevDepth> { };
        foreach (var node in nodes)
            depths.Add(
                new NodeWithRevDepth
                {
                    ReversedDepth = GetReversedNodeDepth(node, schema),
                    Node = node
                }
            );

        var stepsToCreate = new Stack<Step> { };
        var nodesToHandle = new Stack<Node> { };

        depths.Sort((a, b) => b.ReversedDepth - a.ReversedDepth);

        depths.ForEach(d => nodesToHandle.Push(d.Node));

        while (nodesToHandle.Count > 0)
        {
            var node = nodesToHandle.Pop();
            if (options.Any(opt => opt.NodeId == node.Id))
            {
                var opt = options.Where(o => o.NodeId == node.Id).First();
                var mainStep = node.Steps.Where(s => s.Id == opt.StepId).First();
                foreach (var step in node.Steps)
                {
                    if (
                        step.Archived
                        || step.Order > mainStep.Order
                        || stepsToCreate.Any(s => s.Id == step.Id)
                    )
                        continue;
                    stepsToCreate.Push(step);
                }
                foreach (var n in node.Previous)
                {
                    if (nodesToHandle.Any(_ => _.Id == n.Id))
                        continue;
                    var foundNode = schema.Nodes.Where(nd => nd.Id == n.Id && !nd.Archived).First();
                    nodesToHandle.Push(foundNode);
                }
                continue;
            }
            foreach (var step in node.Steps)
            {
                if (step.Archived || stepsToCreate.Any(s => s.Id == step.Id))
                    continue;
                stepsToCreate.Push(step);
            }
            foreach (var n in node.Previous)
            {
                if (nodesToHandle.Any(_ => _.Id == n.Id))
                    continue;
                var foundNode = schema.Nodes.Where(nd => nd.Id == n.Id && !nd.Archived).First();
                nodesToHandle.Push(foundNode);
            }
        }

        // Collect tasks created with Backlog status for notifications
        var backlogTasksToNotify = new List<Models.Task>();

        while (stepsToCreate.Count > 0)
        {
            var step = stepsToCreate.Pop();
            var t = await _context.Tasks
                .Where(
                    loTask =>
                        loTask.StepId == step.Id
                        && !loTask.Archived
                        && loTask.LearningObjectiveId == task.LearningObjectiveId
                )
                .FirstOrDefaultAsync();

            if (t is not null)
            {
                if (options.Any(o => o.StepId == step.Id))
                {
                    if (t.UserId is not null)
                        t.Status = TaskStatusEnum.ToDo;
                    else
                    {
                        t.Status = TaskStatusEnum.Backlog;

                        // Notify team leader when task is reactivated to backlog (unassigned)
                        await _notificationService.NotifyTeamLeaderOfBacklogTask(t.Id, t.GroupId, "Task reactivated via jump");
                    }

                    var newTaskAct2 = new TaskActivity
                    {
                        Task = t,
                        TaskId = t.Id,
                        Type = TaskActivityTypeEnum.ReactivateJump,
                        TimeStamp = DateTime.Now,
                        ActorOne = user,
                        ActorOneId = user.Id,
                        ActorTwo = null,
                        ActorTwoId = null,
                        TaskSecondary = null,
                        TaskSecondaryId = null,
                        AdditionalInfo = null
                    };

                    _context.TaskActivities.Add(newTaskAct2);
                }
                else
                {
                    t.Status = TaskStatusEnum.Done;

                    var newTaskAct2 = new TaskActivity
                    {
                        Task = t,
                        TaskId = t.Id,
                        Type = TaskActivityTypeEnum.Jump,
                        TimeStamp = DateTime.Now,
                        ActorOne = user,
                        ActorOneId = user.Id,
                        ActorTwo = null,
                        ActorTwoId = null,
                        TaskSecondary = null,
                        TaskSecondaryId = null,
                        AdditionalInfo = null
                    };

                    _context.TaskActivities.Add(newTaskAct2);
                }
            }
            else
            {
                var s = await _context.Steps
                    .Include(s => s.TaskBank)
                    .ThenInclude(tb => tb.Group)
                    .Where(ss => ss.Id == step.Id)
                    .FirstAsync();

                var check = options.Any(o => o.StepId == s.Id);

                var newTask = new Models.Task
                {
                    Priority = step.Priority,
                    Step = s,
                    StepId = s.Id,
                    LearningObjective = task.LearningObjective,
                    LearningObjectiveId = task.LearningObjective.Id,
                    TL = s.TaskBank.TL,
                    From = null,
                    FromId = null,
                    Name = s.TaskBank.Name,
                    User = null,
                    UserId = null,
                    Group = s.TaskBank.Group,
                    GroupId = s.TaskBank.GroupId,
                    Pause = false,
                    Status = check
                        ? (s.TaskBank.TL ? TaskStatusEnum.ToDo : TaskStatusEnum.Backlog)
                        : TaskStatusEnum.Done,
                    Flagged = false,
                    Archived = false,
                    IsReview = s.TaskBank.Type == TaskBankTypeEnum.Review,
                    Attention = false,
                    CreatedAt = DateTime.Now,
                    RollbackCount = 0,
                    IsRollback = false
                };

                var newTaskAct = new TaskActivity
                {
                    Task = newTask,
                    TaskId = newTask.Id,
                    Type = TaskActivityTypeEnum.Created,
                    TimeStamp = DateTime.Now,
                    ActorOne = user,
                    ActorOneId = user.Id,
                    ActorTwo = null,
                    ActorTwoId = null,
                    TaskSecondary = null,
                    TaskSecondaryId = null,
                    AdditionalInfo = null
                };

                if (!check)
                {
                    var newTaskAct2 = new TaskActivity
                    {
                        Task = newTask,
                        TaskId = newTask.Id,
                        Type = TaskActivityTypeEnum.Jump,
                        TimeStamp = DateTime.Now,
                        ActorOne = user,
                        ActorOneId = user.Id,
                        ActorTwo = null,
                        ActorTwoId = null,
                        TaskSecondary = null,
                        TaskSecondaryId = null,
                        AdditionalInfo = null
                    };
                    _context.TaskActivities.Add(newTaskAct2);
                }

                _context.TaskActivities.Add(newTaskAct);

                _context.Tasks.Add(newTask);

                // Collect backlog tasks for notification after save
                if (newTask.Status == TaskStatusEnum.Backlog)
                {
                    backlogTasksToNotify.Add(newTask);
                }
            }
        }

        await _context.SaveChangesAsync();

        // Send notifications for tasks created with Backlog status
        foreach (var backlogTask in backlogTasksToNotify)
        {
            // Since these are newly created tasks without assigned users, notify team leader
            await _notificationService.NotifyTeamLeaderOfBacklogTask(backlogTask.Id, backlogTask.GroupId, "New workflow step task created");
        }

        return await getTaskDetails(task.Id);
    }

    private int GetReversedNodeDepth(Node currentNode, Schema schema, int currentDepth = 0)
    {
        if (currentNode.SchemaId != schema.Id)
            throw new Exception("Node is not related to Schema");

        if (currentNode.Archived)
            throw new Exception("Cannot measure depth of archived Nodes");

        if (currentNode.isEnd)
            return currentDepth;

        if (currentNode.Next.Count < 0)
            return int.MinValue;

        int depth = currentDepth;

        foreach (var n in currentNode.Next)
        {
            var node = schema.Nodes.Where(_ => n.Id == _.Id).First();
            int tempDepth = GetReversedNodeDepth(node, schema, currentDepth + 1);

            if (tempDepth > depth && tempDepth > 0)
                depth = tempDepth;
        }

        return depth;
    }

    public async Task<ActionResult<ResponseService<TaskCommentDto>>> AddComment(int id, string comment)
    {
        var user = await _authService.GetAuthedUser();

        var task = await _context.Tasks
            .Where(t => t.Id == id && !t.Archived)
            .Include(t => t.LearningObjective)
            .FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );

        if (comment == "")
            return new BadRequestObjectResult(
                new BaseResponseService { Error = false, Message = "Comment cannot be empty" }
            );

        var newComment = new Comment
        {
            Archived = false,
            LearningObjective = task.LearningObjective,
            LearningObjectiveId = task.LearningObjectiveId,
            Content = comment,
            Timestamp = DateTime.Now,
            User = user,
            UserId = user.Id,
            Task = task,
            TaskId = task.Id
        };

        var newTaskAct = new TaskActivity
        {
            Task = task,
            TaskId = task.Id,
            Type = TaskActivityTypeEnum.Comment,
            TimeStamp = DateTime.Now,
            ActorOne = user,
            ActorOneId = user.Id,
            ActorTwo = null,
            ActorTwoId = null,
            TaskSecondary = null,
            TaskSecondaryId = null,
            AdditionalInfo = null
        };

        _context.TaskActivities.Add(newTaskAct);
        _context.Comments.Add(newComment);
        await _context.SaveChangesAsync();

        return new ResponseService<TaskCommentDto>
        {
            Error = false,
            Message = "New Comment Added",
            Data = new TaskCommentDto
            {
                Id = newComment.Id,
                User = new BasicInfoDto { Id = newComment.UserId, Name = newComment.User.Name },
                Timestamp = newComment.Timestamp,
                Content = newComment.Content
            }
        };
    }

    public async Task<ActionResult<ResponseService<TaskCommentDto>>> EditComment(int taskId, int commentId, string content)
    {
        var user = await _authService.GetAuthedUser();

        var task = await _context.Tasks
            .Where(t => t.Id == taskId && !t.Archived)
            .Include(t => t.LearningObjective)
            .ThenInclude(t => t.Comments)
            .ThenInclude(c => c.Child)
            .FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );

        var comment = task.LearningObjective.Comments.Find(c => c.Id == commentId);

        if (comment is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Comment is not found" }
            );

        if (comment.UserId != user.Id)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Cannot edit others comments" }
            );

        if (content == "")
            return new BadRequestObjectResult(
                new BaseResponseService { Error = false, Message = "Comment cannot be empty" }
            );

        if (comment.Child is not null)
            return await EditComment(taskId, comment.Child.Id, content);

        var newComment = new Comment
        {
            Archived = false,
            LearningObjective = task.LearningObjective,
            LearningObjectiveId = task.LearningObjectiveId,
            Content = content,
            Timestamp = comment.Timestamp,
            CreatedAt = DateTime.Now,
            User = user,
            UserId = user.Id,
            Task = task,
            TaskId = task.Id,
        };

        comment.Child = newComment;
        comment.ChildId = newComment.Id;

        var newTaskAct = new TaskActivity
        {
            Task = task,
            TaskId = task.Id,
            Type = TaskActivityTypeEnum.EditComment,
            TimeStamp = DateTime.Now,
            ActorOne = user,
            ActorOneId = user.Id,
            ActorTwo = null,
            ActorTwoId = null,
            TaskSecondary = null,
            TaskSecondaryId = null,
            AdditionalInfo = null
        };

        _context.TaskActivities.Add(newTaskAct);
        _context.Comments.Add(newComment);
        await _context.SaveChangesAsync();

        return new ResponseService<TaskCommentDto>
        {
            Error = false,
            Message = "New Comment Added",
            Data = new TaskCommentDto
            {
                Id = newComment.Id,
                User = new BasicInfoDto { Id = newComment.UserId, Name = newComment.User.Name },
                Timestamp = newComment.Timestamp,
                Content = newComment.Content,
                IsEdited = true
            }
        };
    }

    public async Task<ActionResult<ResponseService<TaskCommentDto>>> DeleteComment(int taskId, int commentId)
    {
        var user = await _authService.GetAuthedUser();

        var task = await _context.Tasks
            .Where(t => t.Id == taskId && !t.Archived)
            .Include(t => t.LearningObjective)
            .ThenInclude(t => t.Comments)
            .ThenInclude(c => c.Child)
            .Include(t => t.LearningObjective)
            .ThenInclude(t => t.Comments)
            .ThenInclude(t => t.User)
            .FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );

        var comment = task.LearningObjective.Comments.Find(c => c.Id == commentId);

        if (comment is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Comment is not found" }
            );

        if (comment.UserId != user.Id && user.Role != UserRoleEnum.ProjectManger && user.Role != UserRoleEnum.Owner)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Cannot delete others comments" }
            );

        if (comment.Child is not null)
            return await DeleteComment(taskId, comment.Child.Id);

        comment.Archived = true;

        var newTaskAct = new TaskActivity
        {
            Task = task,
            TaskId = task.Id,
            Type = TaskActivityTypeEnum.DeleteComment,
            TimeStamp = DateTime.Now,
            ActorOne = user,
            ActorOneId = user.Id,
            ActorTwo = null,
            ActorTwoId = null,
            TaskSecondary = null,
            TaskSecondaryId = null,
            AdditionalInfo = null
        };

        _context.TaskActivities.Add(newTaskAct);
        await _context.SaveChangesAsync();

        return new ResponseService<TaskCommentDto>
        {
            Error = false,
            Message = "New Comment Added",
            Data = new TaskCommentDto
            {
                Id = comment.Id,
                User = new BasicInfoDto { Id = comment.UserId, Name = comment.User.Name },
                Timestamp = comment.Timestamp,
                IsDeleted = true
            }
        };
    }

    public async Task<BaseResponseService>  CreateProcess(List<int> options, int schemaId, int loId)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null || (user.Role != UserRoleEnum.ProjectManger && user.Role != UserRoleEnum.Owner))
            return new BaseResponseService { Error = true, Message = "Invalid auth" };
        

        if (options.Count == 0)
            return new BaseResponseService { Error = true, Message = "Please provide Steps" };

        var lo = await _context.LearningObjectives
            .Where(lo => lo.Id == loId && !lo.Archived)
            .FirstOrDefaultAsync();

        if (lo is null)
            return new BaseResponseService { Error = true, Message = "Lo is not found" };

        var schema = await _context.Schemas
            .Where(s => s.Id == schemaId && !s.Archived)
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Steps)
            .ThenInclude(s => s.TaskBank)
            .ThenInclude(tb => tb.Group)
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Previous)
            .FirstOrDefaultAsync();

        if (schema is null)
            return new BaseResponseService { Error = true, Message = "Please provide Steps" };

        var selectedSteps = new List<Step> { };

        foreach (var node in schema.Nodes)
            if (!node.Archived)
                foreach (var step in node.Steps)
                {
                    if (options.Contains(step.Id))
                    {
                        if (step.Archived)
                            return new BaseResponseService
                            {
                                Error = true,
                                Message = "Steps not found"
                            };
                        selectedSteps.Add(step);
                    }
                }

        if (selectedSteps.Count != options.Count)
            return new BaseResponseService { Error = true, Message = "Steps not found" };

        var nodes = schema.Nodes
            .Where(n => !n.Archived && selectedSteps.Any(s => s.NodeId == n.Id))
            .ToList();

        if (nodes.Count() != options.Count || !nodes.All(n => nodes.First().SchemaId == n.SchemaId))
            return new BaseResponseService { Error = true, Message = "Invalid Nodes Selected" };

        List<NodeWithRevDepth> depths = new List<NodeWithRevDepth> { };
        foreach (var node in nodes)
            depths.Add(
                new NodeWithRevDepth
                {
                    ReversedDepth = GetReversedNodeDepth(node, schema),
                    Node = node
                }
            );

        var stepsToCreate = new Stack<Step> { };
        var nodesToHandle = new Stack<Node> { };

        depths.Sort((a, b) => b.ReversedDepth - a.ReversedDepth);

        depths.ForEach(d => nodesToHandle.Push(d.Node));

        while (nodesToHandle.Count > 0)
        {
            var node = nodesToHandle.Pop();
            if (selectedSteps.Any(s => s.NodeId == node.Id))
            {
                var mainStep = selectedSteps.Where(o => o.NodeId == node.Id).First();
                foreach (var step in node.Steps)
                {
                    if (
                        step.Archived
                        || step.Order > mainStep.Order
                        || stepsToCreate.Any(s => s.Id == step.Id)
                    )
                        continue;
                    stepsToCreate.Push(step);
                }
                foreach (var n in node.Previous)
                {
                    if (nodesToHandle.Any(_ => _.Id == n.Id))
                        continue;
                    var foundNode = schema.Nodes.Where(nd => nd.Id == n.Id && !nd.Archived).First();
                    nodesToHandle.Push(foundNode);
                }
                continue;
            }
            foreach (var step in node.Steps)
            {
                if (step.Archived || stepsToCreate.Any(s => s.Id == step.Id))
                    continue;
                stepsToCreate.Push(step);
            }
            foreach (var n in node.Previous)
            {
                if (nodesToHandle.Any(_ => _.Id == n.Id))
                    continue;
                var foundNode = schema.Nodes.Where(nd => nd.Id == n.Id && !nd.Archived).First();
                nodesToHandle.Push(foundNode);
            }
        }

        // Collect tasks created with Backlog status for notifications
        var backlogTasksToNotify = new List<Models.Task>();

        while (stepsToCreate.Count > 0)
        {
            var step = stepsToCreate.Pop();

            var check = selectedSteps.Any(o => o.Id == step.Id);

            var newTask = new Models.Task
            {
                Priority = step.Priority,
                Step = step,
                StepId = step.Id,
                LearningObjective = lo,
                LearningObjectiveId = lo.Id,
                TL = step.TaskBank.TL,
                From = null,
                FromId = null,
                Name = step.TaskBank.Name,
                User = null,
                UserId = null,
                Group = step.TaskBank.Group,
                GroupId = step.TaskBank.GroupId,
                Pause = false,
                Status = check
                    ? (step.TaskBank.TL ? TaskStatusEnum.ToDo : TaskStatusEnum.Backlog)
                    : TaskStatusEnum.Done,
                Flagged = false,
                Archived = false,
                IsReview = step.TaskBank.Type == TaskBankTypeEnum.Review,
                Attention = false,
                CreatedAt = DateTime.Now,
                RollbackCount = 0,
                IsRollback = false
            };

            var newTaskAct = new TaskActivity
            {
                Task = newTask,
                TaskId = newTask.Id,
                Type = TaskActivityTypeEnum.Created,
                TimeStamp = DateTime.Now,
                ActorOne = user,
                ActorOneId = user.Id,
                ActorTwo = null,
                ActorTwoId = null,
                TaskSecondary = null,
                TaskSecondaryId = null,
                AdditionalInfo = null
            };

            if (!check)
            {
                var newTaskAct2 = new TaskActivity
                {
                    Task = newTask,
                    TaskId = newTask.Id,
                    Type = TaskActivityTypeEnum.Jump,
                    TimeStamp = DateTime.Now,
                    ActorOne = user,
                    ActorOneId = user.Id,
                    ActorTwo = null,
                    ActorTwoId = null,
                    TaskSecondary = null,
                    TaskSecondaryId = null,
                    AdditionalInfo = null
                };
                _context.TaskActivities.Add(newTaskAct2);
            }

            _context.TaskActivities.Add(newTaskAct);

            _context.Tasks.Add(newTask);

            // Collect backlog tasks for notification after save
            if (newTask.Status == TaskStatusEnum.Backlog)
            {
                backlogTasksToNotify.Add(newTask);
            }
        }

        await _context.SaveChangesAsync();

        // Send notifications for tasks created with Backlog status
        foreach (var backlogTask in backlogTasksToNotify)
        {
            // Since these are newly created tasks without assigned users, notify team leader
            await _notificationService.NotifyTeamLeaderOfBacklogTask(backlogTask.Id, backlogTask.GroupId, "New workflow process task created");
        }

        return new BaseResponseService { Message = "Process Created", Error = false };
    }

    public async Task<ActionResult<ResponseService<GetProjectSheetDto>>> GetProjectTaskChips(int pid)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );

        var p = await _context.Projects
            .Where(_ => _.Id == pid && !_.Archived)
            .FirstOrDefaultAsync();

        if (p is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Project is not found" }
            );

        await _context
            .Entry(p)
            .Collection(p => p.Units)
            .Query()
            .Where(u => !u.Archived)
            .LoadAsync();

        var res = new GetProjectSheetDto
        {
            Id = p.Id,
            Name = p.Name
        };

        // SectionHead role processing using SectionGroup junction table
        if (user.Role == UserRoleEnum.SectionHead)
        {
            var sectionGroups = await _context.SectionGroups
                .Where(sg => sg.Section.HeadId == user.Id && !sg.Section.Archived)
                .Include(sg => sg.Group)
                .ToListAsync();

            foreach (var sectionGroup in sectionGroups)
            {
                if (!sectionGroup.Group.Archived)
                    res.WorkableTasks.Add(new BasicInfoDto
                    {
                        Id = sectionGroup.Group.Id,
                        Name = sectionGroup.Group.Name
                    });
            }
        }

        // Handle roles that must be tied to a specific group
        // Project managers and owners are global roles and are not required to have a group
        if (user.Role != UserRoleEnum.ProjectManger && user.Role != UserRoleEnum.Owner)
        {
            var group = await _context.Groups
                .Where(s => s.Id == user.GroupId)
                .FirstOrDefaultAsync();

            if (group is null)
                throw new Exception("User group is not found");

            res.WorkableTasks.Add(new BasicInfoDto
            {
                Id = group.Id,
                Name = group.Name
            });
        }

        // Process Units and Lessons as before
        foreach (var unit in p.Units)
        {
            var unitChip = new GetUnitChipDto
            {
                Name = unit.Name,
                Id = unit.Id,
            };

            await _context
                .Entry(unit)
                .Collection(unit => unit.Lessons)
                .Query()
                .Where(l => !l.Archived)
                .LoadAsync();

            foreach (var lesson in unit.Lessons)
            {
                var lessonChip = new GetLessonChipDto
                {
                    Id = lesson.Id,
                    Name = lesson.Name
                };

                await _context
                    .Entry(lesson)
                    .Collection(lesson => lesson.LearningObjectives)
                    .Query()
                    .Where(lo => !lo.Archived)
                    .Include(lo => lo.Schema)
                    .LoadAsync();

                foreach (var lo in lesson.LearningObjectives)
                {
                    var loChip = new GetLearningObjectiveChipDto
                    {
                        Id = lo.Id,
                        Name = lo.Name,
                        Tag = lo.Tag,
                        Template = lo.Template,
                        Schema = new BasicInfoDto
                        {
                            Id = lo.Schema.Id,
                            Name = lo.Schema.Name
                        },
                    };

                    await _context
                        .Entry(lo)
                        .Collection(lo => lo.Tasks)
                        .Query()
                        .Where(t => !t.Archived)
                        .Include(t => t.Group)
                        .Include(t => t.User)
                        .LoadAsync();

                    foreach (var task in lo.Tasks)
                    {
                        loChip.Tasks.Add(new GetTaskChipDto
                        {
                            Id = task.Id,
                            Name = task.Name,
                            User = task.User is null ? null : new BasicInfoDto
                            {
                                Id = task.User.Id,
                                Name = task.User.Name
                            },
                            Group = new BasicInfoDto
                            {
                                Id = task.Group.Id,
                                Name = task.Group.Name
                            },
                            Paused = task.Pause,
                            Status = task.Status,
                            IsRollback = task.IsRollback,
                            RollbackCounts = task.RollbackCount
                        });
                    }

                    lessonChip.Los.Add(loChip);
                }
                unitChip.Lessons.Add(lessonChip);
            }
            res.Units.Add(unitChip);
        }

        return new ResponseService<GetProjectSheetDto>
        {
            Data = res,
            Error = false,
            Message = "Project Sheet"
        };
    }

    public void PauseAllTasksForUser(int userId)
    {
        // Get all tasks for the user that are active and not already paused
        var tasks = _context.Tasks
            .Where(t => !t.Archived && t.UserId == userId && 
                    !t.Pause && t.Status != TaskStatusEnum.Done &&
                    t.Status != TaskStatusEnum.Backlog && 
                    t.Status == TaskStatusEnum.Doing)
            .ToList();

        if (!tasks.Any())
            return;

        var activities = new List<TaskActivity>(); // For batch insert of task activities
        var now = DateTime.Now;

        foreach (var task in tasks)
        {
            if (task.Status != TaskStatusEnum.Doing)
            {
                // Log attempt to pause non-active task
                activities.Add(new TaskActivity
                {
                    Type = TaskActivityTypeEnum.Pause,
                    Task = task,
                    TaskId = task.Id,
                    ActorOneId = userId,
                    ActorTwoId = null,
                    TimeStamp = now,
                    AdditionalInfo = "Task cannot be paused because it's not in 'Doing' status",
                    TaskSecondary = null,
                    TaskSecondaryId = null
                });

                continue;
            }

            // Get all ongoing work time sessions for the task
            var ongoingSessions = _context.TaskWorkTimes
                .Where(twt => twt.TaskId == task.Id && twt.EndDate == null)
                .ToList();

            foreach (var session in ongoingSessions)
            {
                session.EndDate = now;
                session.Duration = (now - session.StartDate).TotalMilliseconds;
                session.EndReason = TaskDurationEndReasonEnum.Session;
                session.UserId = userId; // Ensure user is set if needed
            }

            // Pause the task
            task.Status = TaskStatusEnum.ToDo;
            task.Pause = true;

            // Add activity log
            activities.Add(new TaskActivity
            {
                Type = TaskActivityTypeEnum.Pause,
                Task = task,
                TaskId = task.Id,
                ActorOneId = userId,
                ActorTwoId = null,
                TimeStamp = now,
                AdditionalInfo = null,
                TaskSecondary = null,
                TaskSecondaryId = null
            });
        }

        // Add all new activities
        if (activities.Any())
            _context.TaskActivities.AddRange(activities);

        // Save everything in one transaction
        _context.SaveChanges();
    }





}