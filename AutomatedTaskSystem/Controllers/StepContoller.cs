using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers;
[Route("steps")]
[ApiController]
public class StepController : ControllerBase
{
    private readonly DataContext _context;

    public StepController(DataContext context)
    {
        _context = context;
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
            Duration = step.Duration
        };

        return Ok(res);
    }

    [HttpGet("multiple")]
    public async Task<ActionResult<List<Responses.NodeWithStepsDTO>>> GetNodesSteps(
        [FromQuery(Name = "nodeId")] int[] nodeIds
    )
    {
        var nodes = await _context.Nodes
            .Where(n => nodeIds.Contains(n.Id))
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
            .Where(n => n.Id == nodeId)
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
            Order = node.Steps.Count + 1,
            TaskBank = taskBankItem,
            TaskBankId = taskBankItem.Id,
            Archived = false,
            Duration = req.Duration
        };

        _context.Steps.Add(newStep);
        await _context.SaveChangesAsync();

        return await GetStep(newStep.Id);
    }

    [HttpDelete("{stepId}")]
    public async Task<ActionResult<Responses.SuccessDTO>> DeleteStep(int stepId)
    {
        var step = await _context.Steps.Where(s => s.Id == stepId).FirstOrDefaultAsync();

        if (step == null)
            return NotFound(new Responses.BadRequestsDTO("Step not found"));

		step.Archived = true;

        var nextSteps = await _context.Steps
            .Where(s => s.NodeId == step.NodeId && s.Order > step.Order)
            .ToListAsync();

        nextSteps.ForEach(s =>
        {
            s.Order = s.Order - 1;
        });

        await _context.SaveChangesAsync();

        return Ok(new Responses.SuccessDTO("Step Deleted"));
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
}
