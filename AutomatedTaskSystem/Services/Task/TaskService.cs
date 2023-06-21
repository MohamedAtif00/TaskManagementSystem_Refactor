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

        var parseStatus = Int32.TryParse(authRes.Message, out int userId);
        if (!parseStatus)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Request" }
            );

        var authedUser = await _context.Users.Where(u => u.Id == uid).FirstOrDefaultAsync();
        if (authedUser is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Request" }
            );

        if (authedUser.RoleId == 4)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Unauthorized" }
            );

        var user = await _context.Users
            .Where(u => !u.Archived && u.Id == uid)
            .FirstOrDefaultAsync();

        if (user is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "User is not found" }
            );

        var task = await _context.Tasks.Where(t => !t.Archived && t.Id == id).FirstOrDefaultAsync();
        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        if (user.GroupId != task.GroupId)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Cannot assign user to task" }
            );

        task.User = user;

        await _context.SaveChangesAsync();

        return new BaseResponseService { Error = false, Message = "User assigned" };
    }

    public async Task<Models.Task> CreateTask(TaskBank taskBank, LearningObjective lo) =>
        await createTask(taskBank, lo);

    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> CreateTask(
        int taskBankId,
        int loId,
        int? userId
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
            .Where(g => g.Id == taskBankId)
            .Include(tb => tb.Group)
            .FirstOrDefaultAsync();
        if (TaskBankItem is null)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Task Bank Item is not found" }
            );

        var user = userId is null
            ? null
            : await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();

        if (user is null && userId is not null)
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

        var status = await _context.Statuses
            .Where(s => s.Id == (user == null ? Statuses.Backlog : Statuses.ToDo))
            .FirstOrDefaultAsync();
        if (status is null)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "User status does not exist" }
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

    public async Task<ActionResult<ResponseService<GetTaskAssignmentDto>>> GetTaskAssignment(int id)
    {
        var task = await _context.Tasks
            .Where(t => t.Id == id)
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
            .Where(u => u.Id != task.UserId && u.GroupId == task.GroupId)
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
            .Where(t => t.Id == TaskId)
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
            .Where(s => s.Id == (taskBank.TL ? 2 : 1))
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
            Comments = new List<Comment> { },
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
                                t.StepId == step.Id && t.LearningObjectiveId == learningObjective.Id
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
            Comments = new List<Comment> { },
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
            .Include(t => t.Comments)
            .ThenInclude(c => c.User)
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
                Comments = task.Comments
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
}
