using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.Common;
using AutomatedTaskSystem.Dtos.Tasks;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using AutomatedTaskSystem.Static;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.TaskService;

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

        if (authedUser.RoleId == 4)
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Unauthorized" }
            );

        var task = await _context.Tasks.Where(t => !t.Archived && t.Id == id).FirstOrDefaultAsync();
        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        if (task.StatusId == 4 || task.StatusId == 5)
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

            var status = await _context.Statuses.FindAsync(2);
            if (status is null)
                throw new Exception("Status is not found");

            task.User = user;
            task.UserId = user.Id;

            task.Status = status;
            task.StatusId = status.Id;
        }
        else if (uid != task.Id)
        {
            var status = await _context.Statuses.FindAsync(1);
            if (status is null)
                throw new Exception("Status is not found");

            task.User = null;
            task.UserId = null;

            task.Status = status;
            task.StatusId = status.Id;
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
        var backLogStatus = await _context.Statuses.Where(s => s.Id == 1).FirstOrDefaultAsync();

        var todoStatus = await _context.Statuses.Where(s => s.Id == 2).FirstOrDefaultAsync();

        var newTask = await createTask(step: step, learningObjective: lo, null);

        return newTask;
    }

    public async Task<Models.Task> CreateTask(Step step, LearningObjective lo, Models.Task? from)
    {
        var backLogStatus = await _context.Statuses.Where(s => s.Id == 1).FirstOrDefaultAsync();

        var todoStatus = await _context.Statuses.Where(s => s.Id == 2).FirstOrDefaultAsync();

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

        if (user.RoleId == 1)
        {
            var p = await _context.Projects
                .Where(_ => _.Id == pid && !_.Archived)
                .Include(p => p.Units)
                .ThenInclude(u => u.Lessons)
                .ThenInclude(l => l.LearningObjectives)
                .ThenInclude(lo => lo.Tasks)
                .ThenInclude(t => t.Status)
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
                                        Status = task.Status.Name,
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

        if (user.RoleId == 2)
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

        if (user.RoleId == 3 || user.RoleId == 2)
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
                    && t.LearningObjective.Lesson.Unit.ProjectId == project.Id
                    && (t.UserId == user.Id || t.StatusId == 1)
                    && !t.Archived
            );
        var _tasks = await query
            .Include(t => t.LearningObjective)
            .ThenInclude(lo => lo.Lesson)
            .ThenInclude(l => l.Unit)
            .Include(t => t.User)
            .Include(t => t.Group)
            .Include(t => t.Status)
            .Include(t => t.From)
            .ToListAsync();

        return new ResponseService<List<GetTaskCardDto>>
        {
            Data = _tasks
                .Select(
                    t =>
                        new GetTaskCardDto
                        {
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
                            Status = t.Status.Name,
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
        if (task.StatusId != Statuses.Doing)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = false, Message = "Task status should be started" }
            );

        var RollbackStatus = await _context.Statuses.FindAsync(Statuses.Rollback);
        if (RollbackStatus is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = false, Message = "Error" }
            );
        task.Status = RollbackStatus;
        task.StatusId = Statuses.Rollback;

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
            .Include(t => t.Status)
            .LastOrDefaultAsync();
        if (foundTask is not null)
        {
            var status = await _context.Statuses.FindAsync(Statuses.ToDo);
            if (status is null)
                return new BadRequestObjectResult(new Responses.BadRequestsDTO("Error"));

            foundTask.From = task;
            foundTask.FromId = task.FromId;
            foundTask.RollbackCount++;
            foundTask.IsRollback = true;
            foundTask.Status = status;
        }
        else
        {
            var status = await _context.Statuses.FindAsync(
                rollbackStep.TaskBank.TL ? Statuses.ToDo : Statuses.Backlog
            );
            if (status is null)
                return new BadRequestObjectResult(new Responses.BadRequestsDTO("Error"));

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
                Status = status,
                StatusId = status.Id,
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
            .Include(t => t.Status)
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

            var status = await _context.Statuses.FindAsync(2);
            if (status is null)
                throw new Exception("Failed to find status");

            task.Status = status;

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
            .Include(t => t.Status)
            .FirstOrDefaultAsync();
        if (task == null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = false, Message = "Task is not found" }
            );

        if (!task.Pause && task.StatusId != 3)
            return new BadRequestObjectResult(
                new BaseResponseService
                {
                    Error = false,
                    Message = "Task with a status other than 'Doing' cannot be pause"
                }
            );

        if (task.Pause && task.StatusId != 2)
            return new BadRequestObjectResult(
                new BaseResponseService
                {
                    Error = false,
                    Message = "Task cannot be resumed if status is not 'To Do'"
                }
            );

        if (!task.Pause)
        {
            var status = await _context.Statuses.FindAsync(2);
            if (status is null)
                throw new Exception("Failed to find status");
            task.Status = status;
        }

        task.Pause = !task.Pause;

        if (task.Pause)
        {
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
        }

        await _context.SaveChangesAsync();

        return await getTaskDetails(task.Id);
    }

    public async Task<ActionResult<ResponseService<Responses.ITaskDTO>>> UpdateTaskPriority(
        int TaskId,
        int? Priority
    )
    {
        var task = await _context.Tasks
            .Where(t => t.Id == TaskId && !t.Archived)
            .Include(t => t.From)
            .Include(t => t.Group)
            .Include(t => t.User)
            .Include(t => t.Status)
            .Include(t => t.LearningObjective)
            .ThenInclude(lo => lo.Schema)
            .Include(t => t.LearningObjective)
            .ThenInclude(lo => lo.Lesson)
            .ThenInclude(l => l.Unit)
            .ThenInclude(u => u.Project)
            .FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        if (Priority is null || Priority == 1 || Priority == 2 || Priority == 3)
            task.Priority = Priority;
        else
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Priority" }
            );

        await _context.SaveChangesAsync();

        var started = await _context.Activities
            .Where(a => a.TaskId == task.Id && a.ActivityTypeId == 1)
            .OrderBy(a => a.TimeStamp)
            .LastOrDefaultAsync();

        var done = await _context.Activities
            .Where(a => a.TaskId == task.Id && a.ActivityTypeId == 2)
            .OrderBy(a => a.TimeStamp)
            .LastOrDefaultAsync();

        return new ResponseService<Responses.ITaskDTO>
        {
            Error = false,
            Message = "Task Priority edited",
            Data = new Responses.ITaskDTO
            {
                Environment = task.LearningObjective.Environment,
                Tag = task.LearningObjective.Tag,
                Template = task.LearningObjective.Template,
                Flagged = task.Flagged,
                Id = task.Id,
                IsReview = task.IsReview,
                LearningObjective = new Responses.IDName
                {
                    Id = task.LearningObjective.Id,
                    Name = task.LearningObjective.Name
                },
                Name = task.Name,
                Status = task.Status.Name,
                Pause = task.Pause,
                Schema = new Responses.IDName
                {
                    Name = task.LearningObjective.Schema.Name,
                    Id = task.LearningObjective.SchemaId
                },
                StartedAt = started is null ? null : started.TimeStamp,
                DoneAt = done is null ? null : done.TimeStamp,
                Priority = task.Priority
            }
        };
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
        var status = await _context.Statuses
            .Where(s => s.Id == (taskBank.TL || user != null ? 2 : 1))
            .FirstOrDefaultAsync();

        if (status is null)
            throw new Exception($"Unable to find status of id {(taskBank.TL ? 2 : 1)}");

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
            Status = status,
            StatusId = status.Id,
            Flagged = false,
            Archived = false,
            IsReview = taskBank.TypeId == 3,
            Attention = false,
            CreatedAt = DateTime.Now,
            RollbackCount = 0,
            IsRollback = false,
            Priority = null
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
        var status = await _context.Statuses
            .Where(s => s.Id == (step.TaskBank.TL ? 2 : 1))
            .FirstOrDefaultAsync();

        if (status is null)
            throw new Exception($"Unable to find status of id {(step.TaskBank.TL ? 2 : 1)}");

        var prevTasks =
            from == null
                ? 0
                : (
                    await _context.Tasks
                        .Where(
                            t =>
                                t.StepId == step.Id
                                && t.LearningObjectiveId == learningObjective.Id
                                && !t.Archived
                        )
                        .ToListAsync()
                ).Count;

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
            Status = status,
            StatusId = status.Id,
            Flagged = false,
            Archived = false,
            IsReview = step.TaskBank.TypeId == 3,
            Attention = false,
            CreatedAt = DateTime.Now,
            RollbackCount = prevTasks,
            IsRollback = from is null ? false : true
        };

        _context.Tasks.Add(newTask);
        await _context.SaveChangesAsync();

        return newTask;
    }

    private async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> getTaskDetails(int id)
    {
        var task = await _context.Tasks
            .Where(t => !t.Archived && t.Id == id)
            .Include(t => t.Status)
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

        return new ResponseService<GetTaskDetailsDto>
        {
            Error = false,
            Data = new GetTaskDetailsDto
            {
                Comments = task.LearningObjective.Comments
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
                Status = task.Status.Name,
                Tag = task.LearningObjective.Tag,
                Template = task.LearningObjective.Template
            },
            Message = "Task found"
        };
    }

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> ProceedTask(int taskId)
    {
        var task = await _context.Tasks
            .Where(t => !t.Archived && t.Id == taskId)
            .Include(t => t.Status)
            .Include(t => t.User)
            .Include(t => t.LearningObjective)
            .Include(t => t.Step)
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

        if (task.StatusId == 1)
        {
            task.User = user;
            task.UserId = user.Id;
            task.StatusId = 2;

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
        else if (task.StatusId == 2)
        {
            if (task.Attention)
                task.Attention = false;

            if (task.User is not null && user.Id != task.User.Id)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Unauthorized" }
                );

            if (task.User is null)
                task.User = user;

            task.StatusId = Statuses.Doing;
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
        else if (task.StatusId == 3)
        {
            if (task.User is not null && user.Id != task.User.Id)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Unauthorized" }
                );
            task.StatusId = Statuses.Done;

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
            .Include(t => t.Status)
            .Include(t => t.User)
            .Include(t => t.LearningObjective)
            .Include(t => t.Step)
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
                .Include(t => t.Status)
                .ToListAsync();
            if (foundTasks.Count > 0)
                foundTasks.ForEach(t => t.StatusId = Statuses.ToDo);
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
                                .Include(t => t.Status)
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
                                if (t.StatusId != Statuses.Done || t.StatusId == Statuses.Rollback)
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
                            .Include(t => t.Status)
                            .Where(
                                t =>
                                    t.StepId == firstStep.Id
                                    && t.LearningObjectiveId == task.LearningObjectiveId
                                    && !t.Archived
                            )
                            .ToListAsync();

                        if (foundTasks.Count > 0)
                            foundTasks.ForEach(t =>
                            {
                                t.StatusId = Statuses.ToDo;
                            });
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
                            .Include(t => t.Status)
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
                            if (t.StatusId != Statuses.Done || t.StatusId == Statuses.Rollback)
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
                        .Include(t => t.Status)
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
                            t.StatusId = Statuses.Backlog;
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

    public async Task<ActionResult<ResponseService<GetCreatableTasks>>> CreatableTasks(
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

        if (user.RoleId == 3)
        {
            var _taskBankItems = await _context.TaskBank
                .Include(tb => tb.Group)
                .Where(tb => tb.Active && tb.GroupId == user.GroupId)
                .ToListAsync();

            _taskBankItems.Sort((a, b) => String.Compare(a.Name.ToLower(), b.Name.ToLower()));
            return new ResponseService<GetCreatableTasks>
            {
                Error = false,
                Message = "Creatable tasks List",
                Data = new GetCreatableTasks
                {
                    LearningObjectives = los,
                    Assignees = project.Users
                        .Where(u => !u.Archived && u.GroupId == user.GroupId)
                        .Select(i => new BasicInfoDto { Name = i.Name, Id = i.Id })
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

        return new ResponseService<GetCreatableTasks>
        {
            Error = false,
            Message = "Creatable tasks List",
            Data = new GetCreatableTasks
            {
                LearningObjectives = los,
                Assignees = project.Users
                    .Where(u => !u.Archived)
                    .Select(i => new BasicInfoDto { Name = i.Name, Id = i.Id })
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
}
