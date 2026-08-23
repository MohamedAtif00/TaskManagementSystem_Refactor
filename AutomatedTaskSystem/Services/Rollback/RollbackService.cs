using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.Tasks;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.AuthService;
using AutomatedTaskSystem.Models;
using Microsoft.AspNetCore.Mvc;
using AutomatedTaskSystem.Dtos.Common;
using AutomatedTaskSystem.Helper;

namespace AutomatedTaskSystem.Services.RollbackService;

public class RollbackService : IRollbackService
{
    private readonly DataContext _context;
    private readonly IAuthService _authService; 
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly RollbackAttachmentHelper _attachmentHelper;

    public RollbackService(
        DataContext context,
        IAuthService authService,
        IWebHostEnvironment webHostEnvironment,
        RollbackAttachmentHelper attachmentHelper
    )
    {
        _context = context;
        _authService = authService;
        _webHostEnvironment = webHostEnvironment;
        _attachmentHelper = attachmentHelper;
    }

    public async Task<BaseResponseService> CreateRollback(
        int FromTaskId,
        int ToTaskId,
        int UserId,
        string? Clarification,
        List<RollbackLogDto> logs,
        List<IFormFile>? attachments = null
    )
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return new BaseResponseService { Error = true, Message = "Unauthorized" };

        // Reject every invalid file before anything is written to disk or the database,
        // so a bad attachment never leaves a half applied rollback behind.
        var attachmentError = _attachmentHelper.ValidateAttachments(attachments);
        if (attachmentError is not null)
            return new BaseResponseService { Error = true, Message = attachmentError };

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

        if (attachments is not null)
            foreach (var file in attachments)
            {
                var storedPath = await _attachmentHelper.SaveAttachment(file, _webHostEnvironment);

                var attachment = new RollbackAttachment
                {
                    Rollback = rollback,
                    RollbackId = rollback.Id,
                    FileName = RollbackAttachmentHelper.SanitizeFileName(file.FileName),
                    FilePath = storedPath,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    CreatedAt = DateTime.Now
                };

                rollback.Attachments.Add(attachment);
                _context.RollbackAttachments.Add(attachment);
            }

        await _context.SaveChangesAsync();

        return new BaseResponseService { Error = false, Message = "Roll Back log created" };
    }

    private static List<RollbackAttachmentDto> MapAttachments(Rollback rollback) =>
        rollback.Attachments
            .Select(
                a =>
                    new RollbackAttachmentDto
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                        ContentType = a.ContentType,
                        FileSize = a.FileSize
                    }
            )
            .ToList();

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
                .Include(r => r.Attachments)
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
                                Task = new BasicInfoDto { Id = r.ToTask.Id, Name = r.ToTask.Name },
                                Attachments = MapAttachments(r)
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
            .Include(r => r.Attachments)
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
                            Task = new BasicInfoDto { Id = r.Task.Id, Name = r.Task.Name },
                            Attachments = MapAttachments(r)
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
