using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutomatedTaskSystem.Services.TaskService;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using AutomatedTaskSystem.Dtos.Tasks;
using AutomatedTaskSystem.Dtos.Common;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Models.Enums.TaskBankType;
using AutomatedTaskSystem.Services.RollbackService;
using AutomatedTaskSystem.Dtos.Projects;
using System.Text.Json;

namespace AutomatedTaskSystem.Controllers;

[Route("tasks")]
[ApiController]
public class TaskController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly ITaskService _taskService;
    private readonly IRollbackService _rollbackService;

    public TaskController(
        DataContext context,
        ITokenService authService,
        ITaskService taskService,
        IRollbackService rollbackService
    )
    {
        _context = context;
        _tokenService = authService;
        _taskService = taskService;
        _rollbackService = rollbackService;
    }

    [HttpGet("/creatables/{projectId}")]
    public async Task<ActionResult<ResponseService<GetCreatableTasksDto>>> CreatableTasks(
        int projectId
    ) => await _taskService.CreatableTasks(projectId);

    [HttpPatch("{id}/priority")]
    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> EditTask(int id, Requests.PriorityUpdateDto req) => await _taskService.UpdateTaskPriority(id, req.Priority);

    // POST:
    // Add Task
    [Authorize, HttpPost]
    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> AddTask(
        Requests.NewTaskDTO req
    ) =>
        await _taskService.CreateTask(
            taskBankId: req.TaskBankItemId,
            loId: req.LearningObjectiveId,
            userId: req.UserId
        );

    // POST:
    // Add comment to task
    [Authorize, HttpPost("{id}/comment")]
    public async Task<ActionResult<ResponseService<TaskCommentDto>>> AddComment(int id, Requests.CommentDTO req) => await _taskService.AddComment(id, req.Comment);

    // GET:
    // Fetch currently assigned user to task (if any) and users that can work on the task
    [HttpGet("{id}/assigned")]
    public async Task<ActionResult<ResponseService<GetTaskAssignmentDto>>> GetAssignment(int id) =>
        await _taskService.GetTaskAssignment(id);

    // GET:
    // Get one Task
    [HttpGet("{id}")]
    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> GetTask(int id) =>
        await _taskService.GetTaskDetails(id);

    // GET:
    // Get one Task
    [HttpGet("sprint/{id}/task/{taskid}")]
    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> GetTaskForSprint(int taskid,int id) =>
        await _taskService.GetTaskDetailsAdjustedForSprint(taskid,id);

    [HttpGet("/sprints/{loid}/{sprintid}/tasks/cards")]
    public async Task<ActionResult<ResponseService<List<GetTaskCardDto>>>> GetCardTasksByLearningObject(int loid,int sprintid)
    {
        return await _taskService.GetTasksByLearningObjectiveId(loid,sprintid);
    }


    [HttpGet("/sprints/{sprintid}/tasks/cards")]
    public async Task<ActionResult<ResponseService<List<GetTaskCardDto>>>> GetCardTasksForSprint( int sprintid)
    {
        return await _taskService.GetTasksBySprintId( sprintid);
    }

    [HttpGet("/sprints/{sprintid}/tasks/cards/stream")]
    public async System.Threading.Tasks.Task GetCardTasksForSprintStream(int sprintid)
    {
        // Disable response buffering to ensure streaming works
        Response.ContentType = "application/json";
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("X-Accel-Buffering", "no"); // Disable nginx buffering if present
        
        // Disable response buffering
        var bodyFeature = Response.HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>();
        if (bodyFeature != null)
        {
            bodyFeature.DisableBuffering();
        }
        
        // Write opening bracket for JSON array
        await Response.WriteAsync("[");
        
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        
        bool isFirst = true;
        
        await foreach (var item in _taskService.GetTasksBySprintIdStream(sprintid))
        {
            if (!isFirst)
            {
                await Response.WriteAsync(",");
            }
            isFirst = false;
            
            // Serialize and write each item as it comes
            var json = JsonSerializer.Serialize(item, jsonOptions);
            await Response.WriteAsync(json);
            
            // Flush to ensure data is sent immediately
            await Response.Body.FlushAsync();
        }
        
        // Write closing bracket for JSON array
        await Response.WriteAsync("]");
        await Response.Body.FlushAsync();
    }

    // GET:
    // Get Project Tasks as Card
    [Authorize]
    [HttpGet("/projects/{id}/tasks/cards")]
    public async Task<ActionResult<ResponseService<List<GetTaskCardDto>>>> GetCardTasks(int id) =>
        await _taskService.GetProjectTask(id);

    // GET:
    // Get Project Tasks as Sheet
    [Authorize]
    [HttpGet("/projects/{id}/tasks/sheet")]
    public async Task<ActionResult<ResponseService<GetProjectSheetDto>>> GetTasksSheet(int id) =>
        await _taskService.GetProjectTaskChips(id);

    // HttpPatch:
    // Proceed with task
    [Authorize]
    [HttpPatch("{id}/proceed")]
    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> ProceedTask(int id) =>
        await _taskService.ProceedTask(taskId: id);

    // HttpPatch:
    // Proceed with task
    [Authorize]
    [HttpPatch("{id}/complete")]
    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> CompleteTask(int id) =>
        await _taskService.CompleteTask(taskId: id);

    // POST:
    // Rollback Task
    [Authorize]
    [HttpPost("rollback/{id}")]
    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> RollbackTask(int id, Requests.RollbackDTO req) =>
        await _taskService.RollbackTask(
            taskId: id,
            stepId: req.StepId,
            logs: req.Logs,
            clarification: req.Clarification
        );

    // GET:
    // Returns list of and steps
    [Authorize]
    [HttpGet("previous/{taskId}")]
    public async Task<ActionResult<List<BasicInfoDto>>> GetPreviousTasks(int taskId)
    {
        var task = await _context.Tasks
            .Where(t => t.Id == taskId)
            .Include(t => t.Step)
            .FirstOrDefaultAsync();

        if (task == null)
            return BadRequest(new Responses.BadRequestsDTO("Task not found"));

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

        var res = new List<BasicInfoDto> { };

        if (task.Step is null)
            return Ok(res);

        var taskStep = await _context.Steps
            .Where(s => !s.Archived && s.Id == task.Step.Id)
            .Include(s => s.Rollbacks)
            .ThenInclude(s => s.TaskBank)
            .FirstOrDefaultAsync();

        if (
            taskStep is not null
            && taskStep.Rollbacks.Count > 0
            && user.Role != UserRoleEnum.ProjectManger
        )
        {
            foreach (var step in taskStep.Rollbacks)
                res.Add(new BasicInfoDto { Id = step.Id, Name = step.TaskBank.Name });
            return res;
        }

        var schema = await _context.Schemas
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Steps)
            .ThenInclude(s => s.TaskBank)
            .ThenInclude(tb => tb.Group)
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Previous)
            .Where(s => !s.Archived && s.Nodes.Any(n => !n.Archived && n.Id == task.Step.NodeId))
            .FirstOrDefaultAsync();

        if (schema is null)
            return Ok(res);

        var nodes = new List<Node>
        {
            schema.Nodes.Where(n => !n.Archived && n.Steps.Any(s => s.Id == task.StepId)).First()
        };

        while (true)
        {
            var prevList = new List<Node> { };

            foreach (var item in nodes)
                foreach (var prev in item.Previous)
                {
                    var check = nodes.Any(n => !n.Archived && n.Id == prev.Id);
                    if (!check)
                        prevList.Add(prev);
                }

            if (prevList.Count == 0)
                break;

            foreach (var item in prevList)
                nodes.Add(item);
        }

        foreach (var item in nodes)
            foreach (var step in item.Steps)
                if (
                    !step.Archived
                    && step.TaskBank.Type == TaskBankTypeEnum.Creation
                    && (
                        step.NodeId != task.Step.NodeId
                        || (step.NodeId == task.Step.NodeId && step.Order < task.Step.Order)
                    )
                )
                    res.Add(new BasicInfoDto { Id = step.Id, Name = step.TaskBank.Name });

        return Ok(res);
    }

    // POST:
    // Assign Task to user
    [Authorize]
    [HttpPost("{id}/assign")]
    public async Task<ActionResult<BaseResponseService>> AssignUser(int id, Requests.TaskUserAssignDTO req) => await _taskService.AssignUser(id, req.UserId);

    // PATCH:
    // Toggle Flag Route
    [HttpPatch("{id}/flag")]
    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> ToggleFlat(int id) =>
        await _taskService.ToggleFlag(id);

    // PATCH:
    // Toggle Pause Route
    [HttpPatch("{id}/pause")]
    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> TogglePause(int id) =>
        await _taskService.TogglePause(id);

    // POST:
    // Skip task
    [HttpPost("{id}/skip")]
    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> SkipTask(int id) =>
        await _taskService.SkipTask(id);

    // GET:
    // Get Jump Point
    [HttpGet("{id}/jump-points")]
    public async Task<ActionResult<ResponseService<List<GetNodeAheadDto>>>> GetJumpPoints(int id) =>
        await _taskService.GetSchemaSteps(id);

    // PUT:
    // Jump Task
    [HttpPut("{id}/jump")]
    public async Task<ActionResult<ResponseService<GetTaskDetailsDto>>> JumpTask(int id, List<PutJumpedTaskDto> req) => await _taskService.JumpTask(id, req);

    // PATCH:
    // Edit comment
    [Authorize, HttpPatch("{id}/comment")]
    public async Task<ActionResult<ResponseService<TaskCommentDto>>> EditComment(int id, EditCommentDto req) => await _taskService.EditComment(id, req.CommentId, req.Comment);

    // DELETE:
    // Delete comment
    [Authorize, HttpDelete("{id}/comment")]
    public async Task<ActionResult<ResponseService<TaskCommentDto>>> DeleteComment(int id, DeleteCommentDto req) => await _taskService.DeleteComment(id, req.CommentId);

    // GET:
    // Get Task Rollback History
    [HttpGet("{id}/history")]
    public async Task<ActionResult<ResponseService<GetRollbackHistoryDto>>> GetHistory(int id) =>
        await _rollbackService.GetRollbackHistory(id);
}
