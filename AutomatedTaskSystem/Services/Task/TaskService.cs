using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.Common;
using AutomatedTaskSystem.Dtos.Tasks;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.TaskPriority;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.TaskService;

class NodeWithRevDepth
{
    public int ReversedDepth { get; set; }
    public Node Node { get; set; } = new Node { };
}

public class TaskService : ITaskService
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;

    public TaskService(DataContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<ActionResult<BaseResponseService>> AssignUser(int id, int uid)
    {
        var authRes = _tokenService.GetUserIdFromToken();
        if (authRes.Error)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = authRes.Message }
            );

        var parseStatus = Int32.TryParse(authRes.Data, out int userId);
        if (!parseStatus)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid User Request." }
            );

        var authedUser = await _context.Users
            .Where(u => !u.Archived && u.Id == userId)
            .FirstOrDefaultAsync();
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

        if (uid != 0 && uid != task.UserId)
        {
            var user = await _context.Users
                .Where(u => !u.Archived && u.Id == uid)
                .FirstOrDefaultAsync();

            if (user is null)
                return new NotFoundObjectResult(
                    new BaseResponseService { Error = true, Message = "User is not found" }
                );

            if (user.GroupId != task.GroupId)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Cannot assign user to task" }
                );

            task.User = user;
            task.UserId = user.Id;

            task.Status = TaskStatusEnum.ToDo;

            var newAssignment = new Assignment
            {
                By = authedUser,
                ById = authedUser.Id,
                To = user,
                ToId = user.Id,
                Task = task,
                TaskId = task.Id
            };
            _context.Assignments.Add(newAssignment);
        }
        else if (uid == 0)
        {
            task.User = null;
            task.UserId = null;

            task.Status = TaskStatusEnum.Backlog;

            var newAssignment = new Assignment
            {
                By = authedUser,
                ById = authedUser.Id,
                To = null,
                ToId = null,
                Task = task,
                TaskId = task.Id
            };
            _context.Assignments.Add(newAssignment);
        }

        await _context.SaveChangesAsync();

        return new BaseResponseService { Error = false, Message = "User assigned" };
    }

    public async Task<Models.Task> CreateTask(TaskBank taskBank, LearningObjective lo) =>
        await createTask(taskBank, lo);

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> CreateTask(
        int taskBankId,
        int loId,
        int userId
    )
    {
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

        if (user is not null && user.GroupId != TaskBankItem.GroupId)
            return new BadRequestObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = "User cannot be assigned to this task"
                }
            );

        var task = await createTask(TaskBankItem, lo, user);

        return await getTaskDetails(task.Id);
    }

    public async Task<Models.Task> CreateTask(Step step, LearningObjective lo)
    {
        var newTask = await createTask(step: step, learningObjective: lo, null);

        return newTask;
    }

    public async Task<Models.Task> CreateTask(Step step, LearningObjective lo, Models.Task? from)
    {
        var newTask = await createTask(step: step, learningObjective: lo, from);

        return newTask;
    }

    public async Task<ActionResult<ResponseService<List<GetTaskCardDto>>>> GetProjectTask(int pid)
    {
        var authRes = _tokenService.GetUserIdFromToken();
        if (authRes.Error)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = authRes.Message }
            );

        var statusUid = Int32.TryParse(authRes.Data, out int uid);

        if (!statusUid)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Request" }
            );

        var user = await _context.Users
            .Where(u => u.Id == uid && !u.Archived)
            .Include(u => u.Group)
            .Include(u => u.Projects)
            .FirstOrDefaultAsync();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );

        if (user.Role == UserRoleEnum.ProjectManger)
        {
            var p = await _context.Projects
                .Where(_ => _.Id == pid && !_.Archived)
                .Include(p => p.Units)
                .ThenInclude(u => u.Lessons)
                .ThenInclude(l => l.LearningObjectives)
                .ThenInclude(lo => lo.Tasks)
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

            var tasks = new List<GetTaskCardDto> { };
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
                            if (!task.Archived)
                                tasks.Add(
                                    new GetTaskCardDto
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
                                        TL = task.TL,
                                        User = task.User is null
                                            ? null
                                            : new BasicInfoDto
                                            {
                                                Name = task.User.Name,
                                                Id = task.User.Id
                                            }
                                    }
                                );
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

        var project = user.Projects.Where(p => !p.Archived && p.Id == pid).FirstOrDefault();

        if (project is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Project is not found" }
            );

        if (!project.Users.Any(u => u.Id == user.Id))
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "User is unassigned to project" }
            );

        var groups = new List<Group> { user.Group };

        if (user.Role == UserRoleEnum.SectionHead)
        {
            var section = await _context.Sections
                .Where(s => s.HeadId == user.Id && !s.Archived)
                .Include(s => s.Groups)
                .FirstOrDefaultAsync();

            if (section is not null)
                foreach (var group in section.Groups)
                    groups.Add(group);
        }

        IQueryable<Models.Task> query;

        if (user.Role == UserRoleEnum.TeamLeader || user.Role == UserRoleEnum.SectionHead)
            query = _context.Tasks.Where(
                t =>
                    groups.Contains(t.Group)
                    && t.LearningObjective.Lesson.Unit.ProjectId == project.Id
                    && !t.Archived
            );
        else
            query = _context.Tasks.Where(
                t =>
                    t.GroupId == user.GroupId
                    && !t.Archived
                    && t.LearningObjective.Lesson.Unit.ProjectId == project.Id
                    && (t.UserId == user.Id || t.Status == TaskStatusEnum.Backlog)
                    && !t.TL
            );
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
            Data = _tasks
                .Select(
                    t =>
                        new GetTaskCardDto
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
                            TL = t.TL,
                            User = t.User is null
                                ? null
                                : new BasicInfoDto { Name = t.User.Name, Id = t.User.Id }
                        }
                )
                .ToList(),
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

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> RollbackTask(
        int taskId,
        int stepId
    )
    {
        var authRes = _tokenService.GetUserIdFromToken();
        if (authRes.Error)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = authRes.Message }
            );

        var statusUid = Int32.TryParse(authRes.Data, out int uid);
        if (!statusUid)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Request" }
            );

        var user = await _context.Users
            .Where(u => u.Id == uid && !u.Archived)
            .Include(u => u.Group)
            .FirstOrDefaultAsync();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );

        var task = await _context.Tasks
            .Include(t => t.LearningObjective)
            .Where(t => t.Id == taskId && !t.Archived)
            .FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );

        if (task.UserId is not null && task.UserId != user.Id)
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

        var doneAct = await _context.ActivityTypes.FindAsync(2);
        var completeEA = await _context.EndActivityTypes.FindAsync(3);
        if (completeEA != null)
        {
            var currentEA = await _context.EndActivities
                .Where(
                    _ =>
                        _.UserId == user.Id
                        && _.TaskId == task.Id
                        && _.EndDate == null
                        && _.EndActivityTypeId == null
                )
                .FirstOrDefaultAsync();

            if (currentEA != null)
            {
                currentEA.EndActivityTypeId = completeEA.Id;
                currentEA.EndActivityType = completeEA;
                currentEA.EndDate = DateTime.Now;
            }
        }
        if (doneAct == null)
            return new NotFoundObjectResult(new Responses.BadRequestsDTO("I somehow failed"));

        var newAct = new Activity
        {
            Task = task,
            User = user,
            TaskId = task.Id,
            UserId = user.Id,
            ActivityType = doneAct,
            ActivityTypeId = doneAct.Id
        };

        var rollbackStep = await _context.Steps
            .Where(s => s.Id == stepId && !s.Archived)
            .Include(s => s.Node)
            .Include(s => s.TaskBank)
            .ThenInclude(tb => tb.Group)
            .FirstOrDefaultAsync();
        if (rollbackStep is null)
            return new BadRequestObjectResult(new Responses.BadRequestsDTO("Step is not found."));

        var foundTask = await _context.Tasks
            .Where(
                t =>
                    t.StepId == rollbackStep.Id
                    && t.LearningObjectiveId == task.LearningObjectiveId
                    && !t.Archived
            )
            .OrderBy(t => t.Id)
            .LastOrDefaultAsync();
        if (foundTask is not null)
        {
            foundTask.From = task;
            foundTask.FromId = task.FromId;
            foundTask.RollbackCount++;
            foundTask.IsRollback = true;
            foundTask.Status = TaskStatusEnum.ToDo;
        }
        else
        {
            var newTask = new Models.Task
            {
                Group = rollbackStep.TaskBank.Group,
                GroupId = rollbackStep.TaskBank.GroupId,
                IsReview = rollbackStep.TaskBank.TypeId == 3,
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
            _context.Tasks.Add(newTask);
        }

        _context.Activities.Add(newAct);

        await _context.SaveChangesAsync();

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

        if (task.Flagged)
        {
            task.Flagged = false;
            task.Attention = true;
        }
        else
        {
            task.Flagged = true;

            task.Status = TaskStatusEnum.ToDo;

            var flagEA = await _context.EndActivityTypes.FindAsync(1);

            if (flagEA != null)
            {
                var currentEA = await _context.EndActivities
                    .Where(
                        _ =>
                            _.UserId == task.UserId
                            && _.TaskId == task.Id
                            && _.EndDate == null
                            && _.EndActivityTypeId == null
                    )
                    .FirstOrDefaultAsync();

                if (currentEA is not null)
                {
                    currentEA.EndActivityTypeId = flagEA.Id;
                    currentEA.EndActivityType = flagEA;
                    currentEA.EndDate = DateTime.Now;
                }
            }
        }

        await _context.SaveChangesAsync();

        return await getTaskDetails(task.Id);
    }

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> TogglePause(int id)
    {
        var task = await _context.Tasks
            .Where(t => !t.Archived && t.Id == id)
            .FirstOrDefaultAsync();
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

        if (task.Pause)
        {
            if (task.UserId is not null)
                task.Status = TaskStatusEnum.Doing;
            task.Pause = false;
        }
        else
        {
            task.Status = TaskStatusEnum.ToDo;

            var pauseEA = await _context.EndActivityTypes.FindAsync(2);
            if (pauseEA is not null)
            {
                var currentEA = await _context.EndActivities
                    .Where(
                        _ =>
                            _.UserId == task.UserId
                            && _.TaskId == task.Id
                            && _.EndDate == null
                            && _.EndActivityTypeId == null
                    )
                    .FirstOrDefaultAsync();

                if (currentEA is not null)
                {
                    currentEA.EndActivityTypeId = pauseEA.Id;
                    currentEA.EndActivityType = pauseEA;
                    currentEA.EndDate = DateTime.Now;
                }
            }
            task.Pause = true;
        }

        await _context.SaveChangesAsync();

        return await getTaskDetails(task.Id);
    }

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> UpdateTaskPriority(
        int TaskId,
        TaskPriorityEnum Priority
    )
    {
        var task = await _context.Tasks
            .Where(t => t.Id == TaskId && !t.Archived)
            .FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        task.Priority = Priority;
        await _context.SaveChangesAsync();

        return await getTaskDetails(task.Id);
    }

    private async Task<Models.Task> createTask(
        TaskBank taskBank,
        LearningObjective learningObjective
    ) => await createTask(taskBank, learningObjective, null);

    private async Task<Models.Task> createTask(
        TaskBank taskBank,
        LearningObjective learningObjective,
        User? user
    )
    {
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
            Status = taskBank.TL ?  TaskStatusEnum.ToDo: TaskStatusEnum.Backlog,
            Flagged = false,
            Archived = false,
            IsReview = taskBank.TypeId == 3,
            Attention = false,
            CreatedAt = DateTime.Now,
            RollbackCount = 0,
            IsRollback = false,
            Priority = TaskPriorityEnum.None
        };

        _context.Tasks.Add(newTask);
        await _context.SaveChangesAsync();

        return newTask;
    }

    private async Task<Models.Task> createTask(
        Step step,
        LearningObjective learningObjective,
        Models.Task? from
    )
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
            IsReview = step.TaskBank.TypeId == 3,
            Attention = false,
            CreatedAt = DateTime.Now,
            RollbackCount = 0,
            IsRollback = from is null ? false : true
        };

        _context.Tasks.Add(newTask);
        await _context.SaveChangesAsync();

        return newTask;
    }

    private async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> getTaskDetails(int id)
    {
        var authRes = _tokenService.GetUserIdFromToken();
        if (authRes.Error)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = authRes.Message }
            );

        var statusUid = Int32.TryParse(authRes.Data, out int uid);

        if (!statusUid)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Request" }
            );

        var user = await _context.Users
            .Where(u => u.Id == uid && !u.Archived)
            .Include(u => u.Group)
            .ThenInclude(g => g.Section)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = false, Message = "Invalid auth" }
            );

        var task = await _context.Tasks
            .Where(t => !t.Archived && t.Id == id)
            .Include(t => t.User)
            .Include(t => t.LearningObjective)
            .ThenInclude(t => t.Schema)
            .Include(t => t.LearningObjective)
            .ThenInclude(t => t.Comments)
            .ThenInclude(c => c.User)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        var started = await _context.Activities
            .Where(a => a.TaskId == task.Id && a.ActivityTypeId == 1)
            .OrderBy(a => a.TimeStamp)
            .LastOrDefaultAsync();

        var done = await _context.Activities
            .Where(a => a.TaskId == task.Id && a.ActivityTypeId == 2)
            .OrderBy(a => a.TimeStamp)
            .LastOrDefaultAsync();

        var access = TaskAccess.None;

        if (task.Status != TaskStatusEnum.Done && task.Status != TaskStatusEnum.Rollback)
        {
            if (user.Role == UserRoleEnum.ProjectManger)
            {
                if (task.Status == TaskStatusEnum.Backlog || task.UserId == user.Id || task.UserId == null)
                    access = TaskAccess.WorkOnAndManage;
                else
                    access = TaskAccess.Manage;
            }
            else if (user.Role == UserRoleEnum.SectionHead)
            {
                if (
                    user.GroupId == task.GroupId
                    && (task.UserId == user.Id || task.Status == TaskStatusEnum.Backlog || task.UserId == null)
                )
                    access = TaskAccess.WorkOnAndManage;
                else if (task.UserId != user.Id && task.GroupId == user.GroupId)
                    access = TaskAccess.Manage;
                else
                {
                    var section = await _context.Sections
                        .Where(s => s.HeadId == user.Id)
                        .Include(s => s.Groups)
                        .FirstOrDefaultAsync();

                    if (section is not null && section.Groups.Any(g => g.Id == task.GroupId))
                    {
                        if (task.UserId == user.Id || task.Status == TaskStatusEnum.Backlog || task.UserId == null)
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
                    if (task.UserId == user.Id || task.Status == TaskStatusEnum.Backlog || task.UserId == null)
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

        return new ResponseService<GetTaskDetailsDto>
        {
            Error = false,
            Data = new GetTaskDetailsDto
            {
                CreatedAt = task.CreatedAt,
                Comments = task.LearningObjective.Comments
                    .FindAll(t => !t.Archived)
                    .OrderByDescending(c => c.Timestamp)
                    .Select(
                        c =>
                            new TaskCommentDto
                            {
                                Content = c.Content,
                                Id = c.Id,
                                Timestamp = c.Timestamp,
                                User = new BasicInfoDto { Id = c.User.Id, Name = c.User.Name }
                            }
                    )
                    .ToList(),
                User = task.User is not null
                    ? new BasicInfoDto { Id = task.User.Id, Name = task.User.Name }
                    : null,
                DoneAt = done is null ? null : done.TimeStamp,
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
                StartedAt = started is null ? null : started.TimeStamp,
                Status = task.Status,
                Tag = task.LearningObjective.Tag,
                Template = task.LearningObjective.Template,
                Access = access
            },
            Message = "Task found"
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

        var authRes = _tokenService.GetUserIdFromToken();
        if (authRes.Error)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = authRes.Message }
            );

        var statusUid = Int32.TryParse(authRes.Data, out int uid);
        if (!statusUid)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Request" }
            );

        var user = await _context.Users
            .Where(u => u.Id == uid && !u.Archived)
            .Include(u => u.Group)
            .FirstOrDefaultAsync();
        if (user is null)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid auth" }
            );

        if (task.Status == TaskStatusEnum.Backlog)
        {
            task.User = user;
            task.UserId = user.Id;
            task.Status = TaskStatusEnum.ToDo;

            var newAssignemt = new Assignment
            {
                TaskId = task.Id,
                Task = task,
                By = user,
                To = user,
                ById = user.Id,
                ToId = user.Id
            };

            _context.Assignments.Add(newAssignemt);

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

            var startAct = await _context.ActivityTypes.FindAsync(1);

            if (startAct == null)
                throw new Exception("Activity Type is not found");

            var newEA = new EndActivity
            {
                Task = task,
                TaskId = task.Id,
                User = user,
                UserId = user.Id,
                EndActivityType = null,
                EndActivityTypeId = null,
                StartDate = DateTime.Now,
                EndDate = null
            };

            var newAct = new Activity
            {
                Task = task,
                User = user,
                TaskId = task.Id,
                UserId = user.Id,
                ActivityType = startAct,
                ActivityTypeId = startAct.Id
            };

            _context.Activities.Add(newAct);
            _context.EndActivities.Add(newEA);

            await _context.SaveChangesAsync();
            return await GetTaskDetails(task.Id);
        }
        else if (task.Status == TaskStatusEnum.Doing)
        {
            if (task.User is not null && user.Id != task.User.Id)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Unauthorized" }
                );
            task.Status = TaskStatusEnum.Done;

            var doneAct = await _context.ActivityTypes.FindAsync(2);

            if (doneAct == null)
                throw new Exception("Not Found activity");

            var newAct = new Activity
            {
                Task = task,
                User = user,
                TaskId = task.Id,
                UserId = user.Id,
                ActivityType = doneAct,
                ActivityTypeId = doneAct.Id
            };

            _context.Activities.Add(newAct);

            var currentEA = await _context.EndActivities
                .Where(
                    _ =>
                        _.UserId == user.Id
                        && _.TaskId == task.Id
                        && _.EndDate == null
                        && _.EndActivityTypeId == null
                )
                .FirstOrDefaultAsync();

            if (currentEA is not null)
            {
                currentEA.EndActivityTypeId = 3;
                currentEA.EndDate = DateTime.Now;
            }

            await CreateNext(task);

            task.From = null;

            await _context.SaveChangesAsync();
            return await GetTaskDetails(task.Id);
        }

        return new BadRequestObjectResult(
            new BaseResponseService { Error = true, Message = "Cannot proceed with task" }
        );
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
                foundTasks.ForEach(t =>
                {
                    t.Status = TaskStatusEnum.ToDo;
                });
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
                .ThenInclude(n => n.Requires)
                .ThenInclude(n => n.Steps)
                .FirstOrDefaultAsync();

            if (currentNode is not null)
            {
                if (currentNode.Next.Count == 0)
                    task.LearningObjective.DoneAt = DateTime.Now;

                foreach (var nextNode in currentNode.Next)
                {
                    var requiredIsComplete = true;
                    foreach (var nodeRequired in nextNode.Requires)
                    {
                        var lastStep = nodeRequired.Steps
                            .Where(s => s.Order == nodeRequired.Steps.Count && !s.Archived)
                            .FirstOrDefault();
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
                                if (t.Status != TaskStatusEnum.Done || t.Status == TaskStatusEnum.Rollback)
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
                            foundTasks.ForEach(t =>
                                t.Status = TaskStatusEnum.ToDo
                            );
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
            .ThenInclude(n => n.Requires)
            .ThenInclude(n => n.Steps)
            .FirstOrDefaultAsync();

        if (currentNode is not null)
        {
            if (currentNode.Next.Count == 0)
                lo.DoneAt = DateTime.Now;

            foreach (var nextNode in currentNode.Next)
            {
                var requiredIsComplete = true;
                foreach (var nodeRequired in nextNode.Requires)
                {
                    var lastStep = nodeRequired.Steps
                        .Where(s => s.Order == nodeRequired.Steps.Count && !s.Archived)
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
                            if (t.Status != TaskStatusEnum.Done || t.Status == TaskStatusEnum.Rollback)
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
                        foundTasks.ForEach(t =>
                        {
                            t.Status = TaskStatusEnum.Backlog;
                        });
                    else
                    {
                        await createTask(step: firstStep, learningObjective: lo, null);
                    }
                }
            }
        }
        return true;
    }

    public async Task<ActionResult<ResponseService<GetCreatableTasksDto>>> CreatableTasks(
        int projectId
    )
    {
        var authRes = _tokenService.GetUserIdFromToken();
        if (authRes.Error)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = authRes.Message }
            );

        var statusUid = Int32.TryParse(authRes.Data, out int uid);
        if (!statusUid)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Request" }
            );

        var user = await _context.Users
            .Where(u => u.Id == uid && !u.Archived)
            .Include(u => u.Group)
            .FirstOrDefaultAsync();
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

        var los = new List<BasicInfoDto> { };

        foreach (var u in project.Units)
            foreach (var l in u.Lessons)
                foreach (var lo in l.LearningObjectives)
                    if (!lo.Archived)
                        los.Add(new BasicInfoDto { Id = lo.Id, Name = lo.Name });

        los.Sort((a, b) => String.Compare(a.Name.ToLower(), b.Name.ToLower()));

        if (user.Role == UserRoleEnum.TeamLeader)
        {
            var _taskBankItems = await _context.TaskBank
                .Include(tb => tb.Group)
                .Where(tb => tb.Active && tb.GroupId == user.GroupId)
                .ToListAsync();

            _taskBankItems.Sort((a, b) => String.Compare(a.Name.ToLower(), b.Name.ToLower()));
            return new ResponseService<GetCreatableTasksDto>
            {
                Error = false,
                Message = "Creatable tasks List",
                Data = new GetCreatableTasksDto
                {
                    LearningObjectives = los,
                    Assignees = project.Users
                        .Where(u => !u.Archived && u.GroupId == user.GroupId)
                        .Select(
                            i =>
                                new UserDto
                                {
                                    Name = i.Name,
                                    Id = i.Id,
                                    Group = new BasicInfoDto
                                    {
                                        Name = i.Group.Name,
                                        Id = i.Group.Id
                                    }
                                }
                        )
                        .ToList(),
                    Options = _taskBankItems
                        .Select(
                            tb =>
                                new TaskOption
                                {
                                    Name = tb.Name,
                                    Id = tb.Id,
                                    TeamLead = tb.TL,
                                    Group = new BasicInfoDto
                                    {
                                        Id = tb.Group.Id,
                                        Name = tb.Group.Name
                                    }
                                }
                        )
                        .ToList()
                }
            };
        }

        if (user.Role == UserRoleEnum.SectionHead)
        {
            var section = await _context.Sections
                .Where(s => s.HeadId == user.Id)
                .Include(s => s.Groups)
                .FirstOrDefaultAsync();

            var groups = new List<Group> { user.Group };

            if (section is not null)
                foreach (var group in section.Groups)
                    groups.Add(group);

            var _taskBankItems = await _context.TaskBank
                .Include(tb => tb.Group)
                .Where(tb => tb.Active && groups.Contains(tb.Group))
                .ToListAsync();

            _taskBankItems.Sort((a, b) => String.Compare(a.Name.ToLower(), b.Name.ToLower()));
            return new ResponseService<GetCreatableTasksDto>
            {
                Error = false,
                Message = "Creatable tasks List",
                Data = new GetCreatableTasksDto
                {
                    LearningObjectives = los,
                    Assignees = project.Users
                        .Where(u => !u.Archived && u.GroupId == user.GroupId)
                        .Select(
                            i =>
                                new UserDto
                                {
                                    Name = i.Name,
                                    Id = i.Id,
                                    Group = new BasicInfoDto
                                    {
                                        Name = i.Group.Name,
                                        Id = i.Group.Id
                                    }
                                }
                        )
                        .ToList(),
                    Options = _taskBankItems
                        .Select(
                            tb =>
                                new TaskOption
                                {
                                    Name = tb.Name,
                                    Id = tb.Id,
                                    TeamLead = tb.TL,
                                    Group = new BasicInfoDto
                                    {
                                        Id = tb.Group.Id,
                                        Name = tb.Group.Name
                                    }
                                }
                        )
                        .ToList()
                }
            };
        }

        var taskBankItems = await _context.TaskBank
            .Include(tb => tb.Group)
            .Where(tb => tb.Active)
            .ToListAsync();

        taskBankItems.Sort((a, b) => String.Compare(a.Name.ToLower(), b.Name.ToLower()));

        return new ResponseService<GetCreatableTasksDto>
        {
            Error = false,
            Message = "Creatable tasks List",
            Data = new GetCreatableTasksDto
            {
                LearningObjectives = los,
                Assignees = project.Users
                    .Where(u => !u.Archived)
                    .Select(
                        i =>
                            new UserDto
                            {
                                Name = i.Name,
                                Id = i.Id,
                                Group = new BasicInfoDto { Name = i.Group.Name, Id = i.Group.Id }
                            }
                    )
                    .ToList(),
                Options = taskBankItems
                    .Select(
                        tb =>
                            new TaskOption
                            {
                                Name = tb.Name,
                                Id = tb.Id,
                                TeamLead = tb.TL,
                                Group = new BasicInfoDto { Id = tb.Group.Id, Name = tb.Group.Name }
                            }
                    )
                    .ToList()
            }
        };
    }

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> SkipTask(int id)
    {
        var authRes = _tokenService.GetUserIdFromToken();
        if (authRes.Error)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = authRes.Message }
            );

        var statusUid = Int32.TryParse(authRes.Data, out int resId);
        if (!statusUid)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Request" }
            );

        var user = await _context.Users
            .Where(u => u.Id == resId && !u.Archived)
            .Include(u => u.Group)
            .FirstOrDefaultAsync();
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

        task.Status = TaskStatusEnum.Done;

        var doneAct = await _context.ActivityTypes.FindAsync(2);

        if (doneAct == null)
            throw new Exception("Not Found activity");

        var newAct = new Activity
        {
            Task = task,
            User = user,
            TaskId = task.Id,
            UserId = user.Id,
            ActivityType = doneAct,
            ActivityTypeId = doneAct.Id
        };

        _context.Activities.Add(newAct);

        var currentEA = await _context.EndActivities
            .Where(
                _ =>
                    _.UserId == user.Id
                    && _.TaskId == task.Id
                    && _.EndDate == null
                    && _.EndActivityTypeId == null
            )
            .FirstOrDefaultAsync();

        if (currentEA is not null)
        {
            currentEA.EndActivityTypeId = 3;
            currentEA.EndDate = DateTime.Now;
        }

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
                                IsComplete = nodeTasks.Any(t => t.StepId == s.Id && t.Status == TaskStatusEnum.Done)
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

        throw new NotImplementedException();
    }

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> JumpTask(
        int id,
        List<PutJumpedTaskDto> options
    )
    {
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
                    t.Status = TaskStatusEnum.Backlog;
                else
                    t.Status = TaskStatusEnum.Done;
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
                    Status = check ? (s.TaskBank.TL ? TaskStatusEnum.ToDo : TaskStatusEnum.Backlog) : TaskStatusEnum.Done,
                    Flagged = false,
                    Archived = false,
                    IsReview = s.TaskBank.TypeId == 3,
                    Attention = false,
                    CreatedAt = DateTime.Now,
                    RollbackCount = 0,
                    IsRollback = false
                };

                _context.Tasks.Add(newTask);
            }
        }

        await _context.SaveChangesAsync();

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
}
