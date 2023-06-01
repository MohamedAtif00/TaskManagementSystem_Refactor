using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.TaskService;

public class TaskService : ITaskService
{
    private readonly DataContext _context;

    public TaskService(DataContext context)
    {
        _context = context;
    }

    public async Task<Models.Task> CreateTask(TaskBank taskBank, LearningObjective lo)
    {
        return await createTask(taskBank, lo);
    }

    public async Task<Models.Task> CreateTaskWithStep(Step step, LearningObjective lo)
    {
        var backLogStatus = await _context.Statuses.Where(s => s.Id == 1).FirstOrDefaultAsync();

        var todoStatus = await _context.Statuses.Where(s => s.Id == 2).FirstOrDefaultAsync();

        var newTask = await createTask(step: step, learningObjective: lo, null);

        return newTask;
    }

    public async Task<Models.Task> CreateTaskWithStep(
        Step step,
        LearningObjective lo,
        Models.Task? from
    )
    {
        var backLogStatus = await _context.Statuses.Where(s => s.Id == 1).FirstOrDefaultAsync();

        var todoStatus = await _context.Statuses.Where(s => s.Id == 2).FirstOrDefaultAsync();

        var newTask = await createTask(step: step, learningObjective: lo, from);

        return newTask;
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
            User = null,
            UserId = null,
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
}
