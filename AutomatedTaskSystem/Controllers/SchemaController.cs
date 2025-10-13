using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.Schema;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.SchemaService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers;

[Route("schemas")]
[ApiController]
public class SchemaController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ISchemaService _schemaService;

    public SchemaController(DataContext context, ISchemaService schemaService)
    {
        _context = context;
        _schemaService = schemaService;
    }

    // DELETE:
    // Create Task Bank item
    [HttpDelete("task-bank/{id}")]
    public async Task<ActionResult<Responses.TaskBankDTO>> DeleteTaskBankItem(int id)
    {
        var item = await _context.TaskBank.FindAsync(id);
        if (item == null)
            return NotFound(new Responses.BadRequestsDTO("Task Bank Item not found"));

        item.Active = false;

        await _context.SaveChangesAsync();

        return Ok(new Responses.SuccessDTO("Done"));
    }

    // PATCH:
    // Edit Task Bank item
    [HttpPatch("task-bank/{id}")]
    public async Task<ActionResult<Responses.TaskBankDTO>> EditTBI(int id, Requests.TaskBankDTO req)
    {
        var item = await _context.TaskBank.FindAsync(id);
        if (item == null)
            return NotFound(new Responses.BadRequestsDTO("Task Bank Item not found"));

        var group = await _context.Groups.FindAsync(req.Group);
        if (group == null)
            return NotFound(new Responses.BadRequestsDTO("Group not found"));

        item.Type = req.Type;
        item.Group = group;
        item.GroupId = group.Id;
        item.TL = req.TL;
        item.Name = req.Name;
        item.Active = true;
        item.Duration = req.Duration;

        await _context.SaveChangesAsync();

        return Ok(
            new Responses.TaskBankDTO
            {
                Id = item.Id,
                Group = new Responses.IDName { Id = group.Id, Name = group.Name },
                Type = item.Type,
                Name = item.Name,
                TL = item.TL,
                Duration = item.Duration
            }
        );
    }

    // POST:
    // Create Task Bank item
    [HttpPost("task-bank")]
    public async Task<ActionResult<Responses.TaskBankDTO>> CreateTBI(Requests.TaskBankDTO req)
    {
        var group = await _context.Groups.FindAsync(req.Group);
        if (group == null)
            return NotFound(new Responses.BadRequestsDTO("Group not found"));

        var newTaskBank = new TaskBank
        {
            Type = req.Type,
            Group = group,
            GroupId = group.Id,
            TL = req.TL,
            Name = req.Name,
            Active = true,
            Duration = req.Duration
        };

        _context.TaskBank.Add(newTaskBank);
        await _context.SaveChangesAsync();

        return Ok(
            new Responses.TaskBankDTO
            {
                Id = newTaskBank.Id,
                Group = new Responses.IDName { Id = group.Id, Name = group.Name },
                Type = req.Type,
                Name = newTaskBank.Name,
                TL = newTaskBank.TL,
                Duration = newTaskBank.Duration
            }
        );
    }

    // GET:
    // Get all TaskBanks
    [HttpGet("task-bank")]
    public async Task<ActionResult<List<Responses.TaskBankDTO>>> GetTaskBank()
    {
        var bank = await _context.TaskBank.Include(_ => _.Group).Where(_ => _.Active).ToListAsync();

        var res = new List<Responses.TaskBankDTO> { };

        foreach (var item in bank)
            res.Add(
                new Responses.TaskBankDTO
                {
                    Name = item.Name,
                    Id = item.Id,
                    TL = item.TL,
                    Type = item.Type,
                    Group = new Responses.IDName { Id = item.Group.Id, Name = item.Group.Name },
                    Duration = item.Duration
                }
            );

        return res;
    }

    // GET:
    // Get all schemas(Simplified)
    [HttpGet("mini")]
    public async Task<ActionResult<List<Responses.IDName>>> GetSchemasMini()
    {
        var schemas = await _context.Schemas.Where(s => !s.Archived).ToListAsync();

        var res = new List<Responses.IDName> { };

        for (int i = 0; i < schemas.Count; i++)
        {
            var schema = schemas[i];
            var nodes = await _context.Nodes
                .Include(n => n.Steps)
                .Where(node => node.SchemaId == schema.Id && !node.Archived)
                .ToListAsync();

            int tasksCount = 0;
            nodes.ForEach(n =>
            {
                tasksCount += n.Steps.Count;
            });

            res.Add(new Responses.IDName { Id = schema.Id, Name = schema.Name });
        }

        return res;
    }

    // GET:
    // Get all schemas(Simplified)
    [HttpGet]
    public async Task<ActionResult<List<Responses.SchemaDTO>>> GetSchemas()
    {
        var schemas = await _context.Schemas
            .Where(s => !s.Archived)
            .Include(s => s.Type)
            .ToListAsync();

        var res = new List<Responses.SchemaDTO> { };

        foreach (var schema in schemas)
            res.Add(
                new Responses.SchemaDTO
                {
                    Description = schema.Description,
                    Id = schema.Id,
                    Name = schema.Name,
                    Type = schema.Type is null
                        ? null
                        : new Responses.IDName { Id = schema.Type.Id, Name = schema.Type.Name }
                }
            );

        return res;
    }

    // GET:
    // Get schema Info
    [HttpGet("{id}")]
    public async Task<ActionResult<ResponseService<Responses.SchemaDTO>>> GetSchemaDetailed(
        int id
    ) => await _schemaService.GetSchema(id);

    // POST:
    // Duplicate
    [HttpPost("{id}/duplicate")]
    public async Task<ActionResult<ResponseService<Responses.SchemaDTO>>> DuplicateSchema(int id) =>
        await _schemaService.DuplicateSchema(id);

    // POST:
    // Create new Schema
    [HttpPost]
    public async Task<ActionResult<ResponseService<Responses.SchemaDTO>>> CreateSchema(
        Requests.SchemaDTO req
    ) => await _schemaService.CreateSchema(req.Name, req.Description, req.TypeId);

    // PATCH:
    // Update Schema
    [HttpPatch("{id}")]
    public async Task<ActionResult<ResponseService<Responses.SchemaDTO>>> UpdateSchema(
        int id,
        Requests.SchemaDTO req
    ) => await _schemaService.EditSchema(id, req.Name, req.Description, req.TypeId);


	    // GET:
	    // Get archived schemas (Simplified)
	    [HttpGet("archived")]
	    public async Task<ActionResult<List<Responses.SchemaDTO>>> GetArchivedSchemas()
	    {
	        var schemas = await _context.Schemas
	            .Where(s => s.Archived)
	            .Include(s => s.Type)
	            .ToListAsync();

	        var res = new List<Responses.SchemaDTO> { };

	        foreach (var schema in schemas)
	            res.Add(
	                new Responses.SchemaDTO
	                {
	                    Description = schema.Description,
	                    Id = schema.Id,
	                    Name = schema.Name,
	                    Type = schema.Type is null
	                        ? null
	                        : new Responses.IDName { Id = schema.Type.Id, Name = schema.Type.Name }
	                }
	            );

	        return res;
	    }

    // GET:
    // Get Schema Types
    [HttpGet("types")]
    public async Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetSchemaTypes() =>
        await _schemaService.GetSchemaTypes();

    // DELETE:
    // Archive Schema
    [HttpDelete("{id}")]
    public async Task<ActionResult<BaseResponseService>> ArchiveSchema(int id) =>
        await _schemaService.DeleteSchema(id);


	    // POST:
	    // Unarchive Schema
	    [HttpPost("{id}/unarchive")]
	    public async Task<ActionResult<BaseResponseService>> UnarchiveSchema(int id) =>
	        await _schemaService.UnarchiveSchema(id);

    [HttpGet("{id}/points")]
    public async Task<ActionResult<ResponseService<List<GetNodePointDto>>>> GetSchemaPoints(
        int id
    ) => await _schemaService.GetSchemaPoints(id);
}
