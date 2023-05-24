using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.SchemaService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers
{
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

        private async Task<ActionResult<Responses.SchemaDTO>> GetSimpleSchema(int id)
        {
            var schema = await _context.Schemas
                .Where(s => s.Id == id)
                .Include(s => s.LearningObjectives)
                .FirstOrDefaultAsync();

            if (schema == null)
                return NotFound(new Responses.BadRequestsDTO("Schema not found"));

            var res = new Responses.SchemaDTO
            {
                Description = schema.Description,
                Id = schema.Id,
                Name = schema.Name,
            };

            return res;
        }

        private async Task<ActionResult<Responses.DetailedSchemaDTO>> GetSchema(int id)
        {
            var schema = await _context.Schemas.Where(s => s.Id == id).FirstOrDefaultAsync();

            if (schema == null)
                return NotFound(new Responses.BadRequestsDTO("Schema not found"));

            var res = new Responses.DetailedSchemaDTO
            {
                Description = schema.Description,
                Id = schema.Id,
                Name = schema.Name
            };

            return res;
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
        public async Task<ActionResult<Responses.TaskBankDTO>> EditTBI(
            int id,
            Requests.TaskBankDTO req
        )
        {
            var item = await _context.TaskBank.FindAsync(id);
            if (item == null)
                return NotFound(new Responses.BadRequestsDTO("Task Bank Item not found"));

            var type = await _context.Types.FindAsync(req.Type);
            if (type == null)
                return NotFound(new Responses.BadRequestsDTO("Type not found"));
            var group = await _context.Groups.FindAsync(req.Group);
            if (group == null)
                return NotFound(new Responses.BadRequestsDTO("Group not found"));

            item.Type = type;
            item.TypeId = type.Id;
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
                    Type = new Responses.IDName { Id = type.Id, Name = type.Name },
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
            var type = await _context.Types.FindAsync(req.Type);
            if (type == null)
                return NotFound(new Responses.BadRequestsDTO("Type not found"));
            var group = await _context.Groups.FindAsync(req.Group);
            if (group == null)
                return NotFound(new Responses.BadRequestsDTO("Group not found"));

            var newTaskBank = new TaskBank
            {
                Type = type,
                TypeId = type.Id,
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
                    Type = new Responses.IDName { Id = type.Id, Name = type.Name },
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
            var bank = await _context.TaskBank
                .Include(tb => tb.Type)
                .Include(_ => _.Group)
                .Where(_ => _.Active)
                .ToListAsync();

            var res = new List<Responses.TaskBankDTO> { };

            foreach (var item in bank)
                res.Add(
                    new Responses.TaskBankDTO
                    {
                        Name = item.Name,
                        Id = item.Id,
                        TL = item.TL,
                        Type = new Responses.IDName { Id = item.Type.Id, Name = item.Type.Name },
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
            var schemas = await _context.Schemas.Where(s => !s.Archived).ToListAsync();

            var res = new List<Responses.SchemaDTO> { };

            for (int i = 0; i < schemas.Count; i++)
            {
                var schema = schemas[i];

                res.Add(
                    new Responses.SchemaDTO
                    {
                        Description = schema.Description,
                        Id = schema.Id,
                        Name = schema.Name,
                    }
                );
            }

            return res;
        }

        // GET:
        // Get schema Info
        [HttpGet("{id}")]
        public async Task<ActionResult<Responses.DetailedSchemaDTO>> GetSchemaDetailed(int id)
        {
            return await GetSchema(id);
        }

        // POST:
        // Duplicate
        [HttpPost("{id}/duplicate")]
        public async Task<ActionResult<Responses.SchemaDTO>> DuplicateSchema(int id)
        {
            var schema = await _context.Schemas
                .Where(s => s.Id == id && !s.Archived)
                .Include(s => s.Nodes)
                .ThenInclude(n => n.Steps)
                .ThenInclude(s => s.TaskBank)
                .Include(s => s.Nodes)
                .ThenInclude(n => n.Previous)
                .Include(s => s.Nodes)
                .ThenInclude(n => n.Requires)
                .FirstOrDefaultAsync();

            if (schema == null)
                return NotFound(new Responses.BadRequestsDTO("Schema not found"));

            var newSchema = new Schema
            {
                Archived = false,
                Name = $"{schema.Name} - Duplicate",
                Description = schema.Description,
            };
            _context.Schemas.Add(newSchema);
            var newNodes = new List<Node> { };
            var oldNodes = schema.Nodes;
            foreach (var n in oldNodes)
            {
                var nn = new Node
                {
                    Schema = newSchema,
                    SchemaId = newSchema.Id,
                    isEnd = n.isEnd,
                    isStart = n.isStart,
                    Name = n.Name,
                };
                var steps = new List<Step> { };
                n.Steps.ForEach(s =>
                {
                    steps.Add(
                        new Step
                        {
                            Archived = s.Archived,
                            Order = s.Order,
                            Node = nn,
                            TaskBank = s.TaskBank,
                            TaskBankId = s.TaskBankId,
                            NodeId = nn.Id,
                        }
                    );
                });
                nn.Steps = steps;
                newNodes.Add(nn);
                newSchema.Nodes.Add(nn);
                _context.Nodes.Add(nn);
            }

            for (int i = 0; i < oldNodes.Count; i++)
            {
                var oldNode = oldNodes[i];
                foreach (var item in oldNode.Previous)
                {
                    var index = oldNodes.FindIndex(n => n.Id == item.Id);
                    if (index >= 0)
                    {
                        newNodes[i].Previous.Add(newNodes[index]);
                        newNodes[index].Next.Add(newNodes[i]);
                    }
                }
                foreach (var item in oldNode.Requires)
                {
                    var index = oldNodes.FindIndex(n => n.Id == item.Id);
                    if (index >= 0)
                    {
                        newNodes[i].Requires.Add(newNodes[index]);
                        newNodes[index].Required.Add(newNodes[i]);
                    }
                }
            }
            await _context.SaveChangesAsync();

            return Ok(
                new Responses.SchemaDTO
                {
                    Name = newSchema.Name,
                    Description = newSchema.Description,
                    Id = newSchema.Id,
                }
            );
        }

        // POST:
        // Create new Schema
        [HttpPost]
        public async Task<ActionResult<ResponseService<Responses.SchemaDTO>>> CreateSchema(
            Requests.SchemaDTO req
        ) => await _schemaService.CreateSchema(req.Name, req.Description);

        // PATCH:
        // Update Schema
        [HttpPatch("{id}")]
        public async Task<ActionResult<Responses.SchemaDTO>> UpdateSchema(
            int id,
            Requests.SchemaDTO req
        )
        {
            var schema = await _context.Schemas
                .Where(s => !s.Archived && s.Id == id)
                .FirstOrDefaultAsync();

            if (schema == null)
                return BadRequest(new Responses.BadRequestsDTO("Schema not found"));

            if (req.Name != "")
                schema.Name = req.Name;

            schema.Description = req.Description;

            await _context.SaveChangesAsync();

            return await GetSimpleSchema(schema.Id);
        }

        // DELETE:
        // Archive Schema
        // TODO: Archive dependant tables
        [HttpDelete("{id}")]
        public async Task<ActionResult<Responses.SuccessDTO>> ArchiveSchema(int id)
        {
            var schema = await _context.Schemas
                .Where(s => !s.Archived && s.Id == id)
                .FirstOrDefaultAsync();

            if (schema == null)
                return BadRequest(new Responses.BadRequestsDTO("Schema not found"));

            schema.Archived = true;

            await _context.SaveChangesAsync();

            return new Responses.SuccessDTO($"Schema '{schema.Name}' has been archived");
        }
    }
}
