using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.Tasks;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.AuthService;
using AutomatedTaskSystem.Models;
using Microsoft.AspNetCore.Mvc;
using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Services.RollbackService;

public class RollbackService : IRollbackService
{
    private readonly DataContext _context;
    private readonly IAuthService _authService;

    public RollbackService(DataContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<BaseResponseService> CreateRollback(
        int FromTaskId,
        int ToTaskId,
        int UserId,
        string? Clarification,
        List<RollbackLogDto> logs
    )
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return new BaseResponseService { Error = true, Message = "Unauthorized" };

        var fromTask = await _context.Tasks
            .Where(t => !t.Archived && t.Id == FromTaskId)
            .FirstOrDefaultAsync();
        if (fromTask is null)
            return new BaseResponseService { Error = true, Message = "From task is not found" };

        var toTask = await _context.Tasks
            .Where(t => !t.Archived && t.Id == ToTaskId)
            .FirstOrDefaultAsync();
        if (toTask is null)
            return new BaseResponseService { Error = true, Message = "To task is not found" };

        List<int> stepIds = logs.Select(l => l.StepId).ToList();

        var steps = await _context.Steps
            .Where(s => stepIds.Contains(s.Id) && !s.Archived)
            .ToListAsync();
        if (steps.Count() != logs.Count())
            return new BaseResponseService { Error = true, Message = "Steps are not found" };

        var Issues = new List<RollbackIssue> { };
        var rollback = new Rollback
        {
            Task = fromTask,
            TaskId = fromTask.Id,
            ToTask = toTask,
            ToTaskId = toTask.Id,
            User = user,
            UserId = user.Id,
            Clarification = Clarification,
            RollbackIssues = Issues
        };
        _context.Rollbacks.Add(rollback);

        for (int i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var note = logs[i].Note;

            var issue = new RollbackIssue
            {
                Note = note,
                Rollback = rollback,
                RollbackId = rollback.Id,
                Step = step,
                StepId = step.Id
            };

            Issues.Add(issue);
            _context.RollbackIssues.Add(issue);
        }

        await _context.SaveChangesAsync();

        return new BaseResponseService { Error = false, Message = "Roll Back log created" };
    }

    public async Task<ActionResult<ResponseService<GetRollbackHistoryDto>>> GetRollbackHistory(
        int id
    )
    {
        var task = await _context.Tasks.Where(t => t.Id == id && !t.Archived).FirstOrDefaultAsync();

        if (task is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Task is not found" }
            );

        if (task.IsReview)
        {
            var rollbacksTo = await _context.Rollbacks
                .Where(r => r.TaskId == task.Id)
                .Include(r => r.ToTask)
                .Include(r => r.RollbackIssues)
                .ThenInclude(i => i.Step)
                .ThenInclude(i => i.TaskBank)
                .ToListAsync();

            var issuesList = new List<GetIssueDto> { };

            foreach (var item in rollbacksTo)
                foreach (var issue in item.RollbackIssues)
                    issuesList.Add(
                        new GetIssueDto
                        {
                            Id = issue.Id,
                            Note = issue.Note,
                            Task = new BasicInfoDto
                            {
                                Id = issue.Step.Id,
                                Name = issue.Step.TaskBank.Name
                            }
                        }
                    );

            var res1 = new GetRollbackHistoryDto
            {
                Rollbacks = rollbacksTo
                    .Select(
                        r =>
                            new GetRollbackDto
                            {
                                Id = r.Id,
                                Clarification = r.Clarification,
                                Task = new BasicInfoDto { Id = r.ToTask.Id, Name = r.ToTask.Name }
                            }
                    )
                    .ToList(),
                Issues = issuesList
            };
            return new ResponseService<GetRollbackHistoryDto>
            {
                Data = res1,
                Error = false,
                Message = "Task Rollback History"
            };
        }

        var rollbacksFrom = await _context.Rollbacks
            .Where(r => r.ToTaskId == task.Id)
            .Include(r => r.Task)
            .ToListAsync();
        var issues = await _context.RollbackIssues
            .Where(i => i.StepId == task.StepId)
            .Include(i => i.Rollback)
            .ThenInclude(r => r.Task)
            .ToListAsync();

        var res = new GetRollbackHistoryDto
        {
            Rollbacks = rollbacksFrom
                .Select(
                    r =>
                        new GetRollbackDto
                        {
                            Id = r.Id,
                            Clarification = r.Clarification,
                            Task = new BasicInfoDto { Id = r.Task.Id, Name = r.Task.Name }
                        }
                )
                .ToList(),
            Issues = issues
                .Select(
                    i =>
                        new GetIssueDto
                        {
                            Id = i.Id,
                            Note = i.Note,
                            Task = new BasicInfoDto
                            {
                                Id = i.Rollback.Task.Id,
                                Name = i.Rollback.Task.Name
                            }
                        }
                )
                .ToList()
        };
        return new ResponseService<GetRollbackHistoryDto>
        {
            Data = res,
            Error = false,
            Message = "Task Rollback History"
        };

        throw new NotImplementedException();
    }
}
