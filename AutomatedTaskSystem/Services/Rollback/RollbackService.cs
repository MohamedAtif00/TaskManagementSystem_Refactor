using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.Tasks;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.AuthService;
using AutomatedTaskSystem.Models;

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

        var stepIds = logs.Select(l => l.StepId).ToList();

        var steps = await _context.Steps
            .Where(s => stepIds.Contains(s.Id) && !s.Archived)
            .ToListAsync();
        if (steps.Count() == logs.Count())
            return new BaseResponseService { Error = true, Message = "Steps are not found" };

        var Issues = new List<RollbackIssue> { };
        var rollback = new Rollback
        {
            FromTask = fromTask,
            FromTaskId = fromTask.Id,
            ToTask = toTask,
            ToTaskId = toTask.Id,
            User = user,
            UserId = user.Id,
            Clarification = Clarification,
            RollbackIssues = Issues,
			Resolved = false
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
}
