using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.Common;
using AutomatedTaskSystem.Dtos.Steps;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TaskService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers;

[Route("steps")]
[ApiController]
public class StepController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ITaskService _taskService;

    public StepController(DataContext context, ITaskService taskService)
    {
        _context = context;
        _taskService = taskService;
    }

    private async Task<ActionResult<Responses.StepDTO>> GetStep(int id)
    {
        var step = await _context.Steps
            .Where(s => s.Id == id)
            .Include(s => s.TaskBank)
            .ThenInclude(t => t.Group)
            .Include(s => s.Node)
            .FirstOrDefaultAsync();

        if (step == null)
            return NotFound(new Responses.BadRequestsDTO("Step not found"));

        var res = new Responses.StepDTO
        {
            Id = step.Id,
            Order = step.Order,
            Reviewable = step.TaskBank.TypeId == 3,
            TL = step.TaskBank.TL,
            Group = new Responses.IDName
            {
                Id = step.TaskBank.GroupId,
                Name = step.TaskBank.Group.Name
            },
            Name = step.TaskBank.Name,
            Duration = step.Duration,
            Priority = step.Priority
        };

        return Ok(res);
    }

    [HttpGet("multiple")]
    public async Task<ActionResult<List<Responses.NodeWithStepsDTO>>> GetNodesSteps(
        [FromQuery(Name = "nodeId")] int[] nodeIds
    )
    {
        var nodes = await _context.Nodes
            .Where(n => nodeIds.Contains(n.Id) && !n.Archived)
            .Include(n => n.Steps)
            .ThenInclude(s => s.TaskBank)
            .ToListAsync();

        var res = new List<Responses.NodeWithStepsDTO> { };

        foreach (var node in nodes)
        {
            var steps = new List<Responses.IDName> { };
            foreach (var step in node.Steps)
                steps.Add(new Responses.IDName { Name = step.TaskBank.Name, Id = step.Id });
            res.Add(
                new Responses.NodeWithStepsDTO
                {
                    Id = node.Id,
                    Name = node.Name,
                    Steps = steps
                }
            );
        }

        return res;
    }

    [HttpPost("{nodeId}")]
    public async Task<ActionResult<Responses.StepDTO>> CreateStep(int nodeId, Requests.StepDTO req)
    {
        var node = await _context.Nodes
            .Where(n => n.Id == nodeId && !n.Archived)
            .Include(n => n.Steps)
            .FirstOrDefaultAsync();
        if (node == null)
            return NotFound(new Responses.BadRequestsDTO("Node not found"));

        var taskBankItem = await _context.TaskBank
            .Include(_ => _.Group)
            .Include(_ => _.Type)
            .Where(_ => _.Id == req.TaskBankItem)
            .FirstOrDefaultAsync();

        if (taskBankItem == null)
            return NotFound(new Responses.BadRequestsDTO("Task Bank Item not found"));

        var newStep = new Step
        {
            Node = node,
            NodeId = node.Id,
            Order = node.Steps.Where(s => !s.Archived).ToList().Count + 1,
            TaskBank = taskBankItem,
            TaskBankId = taskBankItem.Id,
            Archived = false,
            Duration = req.Duration,
        };

        _context.Steps.Add(newStep);
        await _context.SaveChangesAsync();

        return await GetStep(newStep.Id);
    }

    [HttpOptions("{id}/delete")]
    public async Task<ActionResult<ResponseService<GetStepDeleteCheckDto>>> DeleteCheck(int id)
    {
        var step = await _context.Steps
            .Where(s => s.Id == id && !s.Archived)
            .Include(s => s.Tasks)
            .Include(s => s.TaskBank)
            .FirstOrDefaultAsync();

        if (step is null)
            return NotFound(
                new BaseResponseService { Error = false, Message = "Step is not found" }
            );

        foreach (var task in step.Tasks)
            if (task.StatusId != 4 && task.StatusId != 5 && !task.Archived)
                return new ResponseService<GetStepDeleteCheckDto>
                {
                    Message = "Step contains active tasks",
                    Error = false,
                    Data = new GetStepDeleteCheckDto
                    {
                        Id = step.Id,
                        Name = step.TaskBank.Name,
                        isSafeToDelete = false
                    }
                };
        return new ResponseService<GetStepDeleteCheckDto>
        {
            Message = "Step contains no active tasks",
            Error = false,
            Data = new GetStepDeleteCheckDto
            {
                Id = step.Id,
                NodeId = step.NodeId,
                Name = step.TaskBank.Name,
                isSafeToDelete = true
            }
        };
    }

    [HttpDelete("{stepId}")]
    public async Task<ActionResult<Responses.SuccessDTO>> DeleteStep(int stepId)
    {
        var step = await _context.Steps
            .Where(s => s.Id == stepId && !s.Archived)
            .FirstOrDefaultAsync();

        if (step == null)
            return NotFound(new Responses.BadRequestsDTO("Step not found"));

        step.Archived = true;

        var nextSteps = await _context.Steps
            .Where(s => s.NodeId == step.NodeId && s.Order > step.Order && !s.Archived)
            .ToListAsync();

        foreach (var s in nextSteps)
            s.Order = s.Order - 1;

        var tasks = await _context.Tasks
            .Where(t => !t.Archived && t.StepId == step.Id && t.StatusId != 4 && t.StatusId != 5)
            .ToListAsync();

        foreach (var task in tasks)
        {
            await _taskService.CreateNext(task.Id);
            task.Archived = true;
        }

        await _context.SaveChangesAsync();

        return Ok(new Responses.SuccessDTO("Step Deleted"));
    }

    [HttpPatch("{stepId}/priority")]
    public async Task<ActionResult<ResponseService<Responses.StepDTO>>> EditStep(
        int stepId,
        Requests.PriorityUpdateDto req
    )
    {
        var step = await _context.Steps
            .Where(s => s.Id == stepId)
            .Include(s => s.TaskBank)
            .ThenInclude(t => t.Group)
            .Include(s => s.Node)
            .FirstOrDefaultAsync();

        if (step == null)
            return NotFound(new Responses.BadRequestsDTO("Step not found"));

        if (req.Priority == 1 || req.Priority == 2 || req.Priority == 3 || req.Priority is null)
        {
            if (req.Priority != step.Priority)
            {
                step.Priority = req.Priority;
                await _context.SaveChangesAsync();
            }
        }
        else
            return BadRequest(
                new BaseResponseService { Error = true, Message = "Invalid Priority" }
            );

        return new ResponseService<Responses.StepDTO>
        {
            Data = new Responses.StepDTO
            {
                Duration = step.Duration,
                Group = new Responses.IDName
                {
                    Id = step.TaskBank.GroupId,
                    Name = step.TaskBank.Group.Name
                },
                Id = step.Id,
                Name = step.TaskBank.Name,
                Order = step.Order,
                Priority = step.Priority,
                Reviewable = step.TaskBank.TypeId == 3,
                TL = step.TaskBank.TL
            },
            Error = false,
            Message = "Priority updated"
        };
    }

    [HttpPatch("{stepId}")]
    public async Task<ActionResult<Responses.StepDTO>> EditStep(int stepId, Requests.StepDTO req)
    {
        var step = await _context.Steps
            .Where(s => s.Id == stepId)
            .Include(s => s.Node)
            .FirstOrDefaultAsync();

        if (step == null)
            return NotFound(new Responses.BadRequestsDTO("Step not found"));

        var taskBankItem = await _context.TaskBank
            .Include(tb => tb.Group)
            .Include(tb => tb.Type)
            .Where(_ => _.Id == req.TaskBankItem)
            .FirstOrDefaultAsync();

        if (taskBankItem == null)
            return NotFound(new Responses.BadRequestsDTO("Task Bank Item not found"));

        step.Duration = req.Duration;
        step.TaskBankId = taskBankItem.Id;
        step.TaskBank = taskBankItem;

        await _context.SaveChangesAsync();

        return await GetStep(step.Id);
    }

    [HttpPatch("{stepId}/up")]
    public async Task<ActionResult<BaseResponseService>> StepUp(int stepId)
    {
        var step = await _context.Steps
            .Where(s => s.Id == stepId && !s.Archived)
            .FirstOrDefaultAsync();

        if (step is null)
            return BadRequest(
                new BaseResponseService { Error = true, Message = "Step is not found" }
            );

        var steps = await _context.Steps
            .Where(s => s.NodeId == step.NodeId && !s.Archived)
            .ToListAsync();

        if (step.Order > 1)
        {
            var prevStep = steps.Where(n => n.Order + 1 == step.Order).FirstOrDefault();

            if (prevStep is not null)
                prevStep.Order++;
            step.Order--;
        }

        await _context.SaveChangesAsync();

        return new BaseResponseService { Error = false, Message = "Done" };

        throw new NotImplementedException("");
    }

    [HttpPatch("{stepId}/down")]
    public async Task<ActionResult<BaseResponseService>> StepDown(int stepId)
    {
        var step = await _context.Steps
            .Where(s => s.Id == stepId && !s.Archived)
            .FirstOrDefaultAsync();

        if (step is null)
            return BadRequest(
                new BaseResponseService { Error = true, Message = "Step is not found" }
            );

        var steps = await _context.Steps
            .Where(s => s.NodeId == step.NodeId && !step.Archived)
            .ToListAsync();

        if (steps.Any(n => n.Order > step.Order))
        {
            var prevNode = steps.Where(n => n.Order - 1 == step.Order).FirstOrDefault();

            if (prevNode is not null)
                prevNode.Order--;
            step.Order++;
        }
        await _context.SaveChangesAsync();

        return new BaseResponseService { Error = false, Message = "Done" };
    }

    [HttpGet("{stepId}/rollback-points")]
    public async Task<ActionResult<ResponseService<List<BasicInfoDto>>>> GetRollbackPoints(
        int stepId
    )
    {
        var step = await _context.Steps
            .Where(s => s.Id == stepId && !s.Archived)
            .Include(s => s.Rollbacks)
            .ThenInclude(s => s.TaskBank)
            .FirstOrDefaultAsync();

        if (step is null)
            return BadRequest(
                new BaseResponseService { Error = true, Message = "Step is not found" }
            );

        var list = new List<BasicInfoDto> { };

        foreach (var s in step.Rollbacks)
            list.Add(new BasicInfoDto { Id = s.Id, Name = s.TaskBank.Name });

        return new ResponseService<List<BasicInfoDto>>
        {
            Error = false,
            Message = "List of rollback points",
            Data = list
        };
    }

    [HttpPatch("{stepId}/add-rollback-points")]
    public async Task<ActionResult<BaseResponseService>> AddToRollbacks(
        int stepId,
        UpdateRollbackStepsDto req
    )
    {
        var step = await _context.Steps
            .Where(s => s.Id == stepId && !s.Archived)
            .Include(s => s.Rollbacks)
            .FirstOrDefaultAsync();

        if (step is null)
            return BadRequest(
                new BaseResponseService { Error = true, Message = "Step is not found" }
            );

        var steps = await _context.Steps
            .Where(s => !s.Archived && req.ids.Contains(s.Id))
            .ToListAsync();

        foreach (var s in steps)
        {
            if (!s.Rollbacks.Any(_ => _.Id == s.Id))
            {
                step.Rollbacks.Add(s);
                s.From.Add(step);
            }
        }

        await _context.SaveChangesAsync();

        return new BaseResponseService { Error = false, Message = "Step added rollback points" };
    }

    [HttpPatch("{stepId}/remove-rollback-points")]
    public async Task<ActionResult<BaseResponseService>> RemoveFromRollbacks(
        int stepId,
        UpdateRollbackStepsDto req
    )
    {
        var step = await _context.Steps
            .Where(s => s.Id == stepId && !s.Archived)
            .Include(s => s.Rollbacks)
            .FirstOrDefaultAsync();

        if (step is null)
            return BadRequest(
                new BaseResponseService { Error = true, Message = "Step is not found" }
            );

        var steps = step.Rollbacks.Where(s => !s.Archived && req.ids.Contains(s.Id)).ToList();

        foreach (var s in steps)
        {
            step.Rollbacks.Remove(s);
            s.From.Remove(step);
        }

        await _context.SaveChangesAsync();

        return new BaseResponseService { Error = false, Message = "Step remove rollback points" };
    }
}
