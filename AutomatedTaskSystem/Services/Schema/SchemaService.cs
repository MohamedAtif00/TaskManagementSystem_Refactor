using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.Common;
using AutomatedTaskSystem.Dtos.Schema;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Models.SchemaTypesModel;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.SchemaService;

public class SchemaService : ISchemaService
{
    private readonly DataContext _context;

    public SchemaService(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResult<ResponseService<Responses.SchemaDTO>>> CreateSchema(
        string Name,
        string Description,
        int? TypeId
    )
    {
        var doesExists = await CheckIfExists(Name);

        if (doesExists)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Schema already exists" }
            );

        SchemaType? type;

        if (TypeId != 0)
        {
            type = await _context.SchemaTypes.Where(t => t.Id == TypeId).FirstOrDefaultAsync();

            if (type is null)
                return new NotFoundObjectResult(
                    new BaseResponseService { Error = true, Message = "Type is not found" }
                );
        }
        else
            type = null;

        var schema = new Schema
        {
            Archived = false,
            Name = Name,
            Description = Description,
            Nodes = { },
            LearningObjectives = { },
            Type = type,
            TypeId = type is null ? null : type.Id
        };

        _context.Schemas.Add(schema);

        await _context.SaveChangesAsync();

        return new ResponseService<Responses.SchemaDTO>
        {
            Data = new Responses.SchemaDTO
            {
                Description = schema.Description,
                Id = schema.Id,
                Name = schema.Name,
                Type = schema.Type is null
                    ? null
                    : new Responses.IDName { Id = schema.Type.Id, Name = schema.Type.Name }
            }
        };
    }

    public async Task<ActionResult<BaseResponseService>> DeleteSchema(int id)
    {
        var schema = await _context.Schemas
            .Where(s => s.Id == id && !s.Archived)
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Steps)
            .ThenInclude(s => s.TaskBank)
            .Include(s => s.LearningObjectives)
            .ThenInclude(lo => lo.Lesson)
            .ThenInclude(l => l.Unit)
            .ThenInclude(u => u.Project)
            .Include(s => s.LearningObjectives)
            .ThenInclude(lo => lo.Tasks)
            .FirstOrDefaultAsync();

        if (schema is null)
            return new BaseResponseService { Error = true, Message = "Schema is not found" };

        var activeTasks = new List<Models.Task> { };

        foreach (var lo in schema.LearningObjectives)
            foreach (var task in lo.Tasks)
                if (!task.Archived && (task.Status != TaskStatusEnum.Done || task.Status != TaskStatusEnum.Rollback))
                    activeTasks.Add(task);

        if (activeTasks.Count == 0)
        {
            foreach (var node in schema.Nodes)
            {
                foreach (var step in node.Steps)
                    step.Archived = true;
                node.Archived = true;
            }
            schema.Archived = true;

            await _context.SaveChangesAsync();

            return new BaseResponseService { Error = false, Message = "Schema archived" };
        }

        var projects = new List<Project> { };
        var units = new List<Unit> { };
        var lessons = new List<Lesson> { };
        var los = new List<LearningObjective> { };

        foreach (var task in activeTasks)
        {
            if (!projects.Any(p => task.LearningObjective.Lesson.Unit.ProjectId == p.Id))
                projects.Add(task.LearningObjective.Lesson.Unit.Project);

            if (!units.Any(u => task.LearningObjective.Lesson.UnitId == u.Id))
                units.Add(task.LearningObjective.Lesson.Unit);
            if (!lessons.Any(u => task.LearningObjective.LessonId == u.Id))
                lessons.Add(task.LearningObjective.Lesson);
            if (!los.Any(u => task.LearningObjectiveId == u.Id))
                los.Add(task.LearningObjective);
        }

        var res = new List<Responses.ActiveProjectDTO> { };

        res = projects
            .Select(
                p =>
                    new Responses.ActiveProjectDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Units = units
                            .Where(u => u.ProjectId == p.Id)
                            .ToList()
                            .Select(
                                u =>
                                    new Responses.ActiveProjectDTO.ActiveUnitDTO
                                    {
                                        Id = u.Id,
                                        Name = u.Name,
                                        Lessons = lessons
                                            .Where(l => l.UnitId == u.Id)
                                            .ToList()
                                            .Select(
                                                l =>
                                                    new Responses.ActiveProjectDTO.ActiveUnitDTO.ActiveLessonDTO
                                                    {
                                                        Id = l.Id,
                                                        Name = l.Name,
                                                        LearningObjectives = los.Where(
                                                                lo => lo.LessonId == l.Id
                                                            )
                                                            .ToList()
                                                            .Select(
                                                                lo =>
                                                                    new Responses.ActiveProjectDTO.ActiveUnitDTO.ActiveLessonDTO.ActiveLearningObjectiveDTO
                                                                    {
                                                                        Id = lo.Id,
                                                                        Name = lo.Name
                                                                    }
                                                            )
                                                            .ToList()
                                                    }
                                            )
                                            .ToList()
                                    }
                            )
                            .ToList()
                    }
            )
            .ToList();

        return new BadRequestObjectResult(
            new ResponseService<List<Responses.ActiveProjectDTO>>
            {
                Data = res,
                Error = true,
                Message = "Schema is unarchiveable due to still running tasks"
            }
        );
    }

    public async Task<ActionResult<ResponseService<Responses.SchemaDTO>>> DuplicateSchema(int id)
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
            .Include(s => s.Type)
            .FirstOrDefaultAsync();

        if (schema is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Schema is not found" }
            );

        var newSchema = new Schema
        {
            Archived = false,
            Name = $"{schema.Name} - Duplicate",
            Description = schema.Description,
            Type = schema.Type,
            TypeId = schema.TypeId
        };
        _context.Schemas.Add(newSchema);
        var newNodes = new List<Node> { };
        var oldNodes = schema.Nodes;
        foreach (var n in oldNodes)
        {
            var nn = new Node
            {
                Order = n.Order,
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

        return new ResponseService<Responses.SchemaDTO>
        {
            Data = new Responses.SchemaDTO
            {
                Description = newSchema.Description,
                Id = newSchema.Id,
                Name = newSchema.Name
            },
            Error = false,
            Message = "Schema is not duplicated"
        };
    }

    public async Task<ActionResult<ResponseService<Responses.SchemaDTO>>> EditSchema(
        int id,
        string Name,
        string Description,
        int? typeId
    )
    {
        var schema = await _context.Schemas
            .Where(s => !s.Archived && s.Id == id)
            .Include(s => s.Type)
            .FirstOrDefaultAsync();

        if (schema is null)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Schema is not found" }
            );

        if (Name == "")
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Name cannot be empty" }
            );

        if (typeId == 0)
            schema.Type = null;
        else
        {
            var type = await _context.SchemaTypes.Where(t => t.Id == typeId).FirstOrDefaultAsync();

            if (type is null)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Type is not found" }
                );

            schema.Type = type;
        }

        schema.Name = Name;
        schema.Description = Description;

        await _context.SaveChangesAsync();

        return new ResponseService<Responses.SchemaDTO>
        {
            Data = new Responses.SchemaDTO
            {
                Description = schema.Description,
                Id = schema.Id,
                Name = schema.Name,
                Type = schema.Type is not null
                    ? new Responses.IDName { Id = schema.Type.Id, Name = schema.Type.Name }
                    : null
            },
            Error = false,
            Message = "schema edited"
        };
    }

    public async Task<ActionResult<ResponseService<Responses.SchemaDTO>>> GetSchema(int id)
    {
        var schema = await _context.Schemas
            .Include(s => s.Type)
            .Where(s => s.Id == id)
            .FirstOrDefaultAsync();

        if (schema is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Schema is not found" }
            );

        return new ResponseService<Responses.SchemaDTO>
        {
            Data = new Responses.SchemaDTO
            {
                Description = schema.Description,
                Id = schema.Id,
                Name = schema.Name,
                Type = schema.Type is null
                    ? null
                    : new Responses.IDName { Id = schema.Type.Id, Name = schema.Type.Name }
            },
            Error = false,
            Message = "Schema found"
        };
    }

    public async Task<ActionResult<ResponseService<List<GetNodePointDto>>>> GetSchemaPoints(int id)
    {

        var schema = await _context.Schemas
            .Where(s => s.Id == id && !s.Archived)
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Previous)
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Next)
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Steps)
            .ThenInclude(s => s.TaskBank)
            .ThenInclude(s => s.Group)
            .FirstOrDefaultAsync();

        if (schema is null)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Schema is not active" }
            );

        var nodesRes = new List<GetNodePointDto> { };

        var nodesAhead = (
            from node in schema.Nodes
            where !node.Archived
            orderby node.Order
            select node
        ).ToList();

        foreach (var node in nodesAhead)
        {
            nodesRes.Add(
                new GetNodePointDto
                {
                    PreviousNodes = node.Previous
                        .Where(n => !n.Archived)
                        .Select(s => new BasicInfoDto { Id = s.Id, Name = s.Name })
                        .ToList(),
                    NextNodes = node.Next
                        .Where(n => !n.Archived)
                        .Select(s => new BasicInfoDto { Id = s.Id, Name = s.Name })
                        .ToList(),
                    Id = node.Id,
                    Name = node.Name,
                    Order = node.Order,
                    Steps = node.Steps
                        .Where(s => !s.Archived)
                        .OrderBy(s => s.Order)
                        .Select(s =>
                        {
                            return new GetStepPointDto
                            {
                                Id = s.Id,
                                Name = s.TaskBank.Name,
                                Group = new BasicInfoDto
                                {
                                    Name = s.TaskBank.Group.Name,
                                    Id = s.TaskBank.Group.Id
                                },
                            };
                        })
                        .ToList(),
                }
            );
        }

        return new ResponseService<List<GetNodePointDto>>
        {
            Data = nodesRes,
            Message = "List of All Nodes"
        };
    }

    public async Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetSchemaTypes()
    {
        var types = await _context.SchemaTypes.ToListAsync();
        return new ResponseService<List<Responses.IDName>>
        {
            Message = "List of schema types",
            Error = false,
            Data = types.Select(t => new Responses.IDName { Name = t.Name, Id = t.Id }).ToList()
        };
    }

    private async Task<bool> CheckIfExists(string Name) =>
        await _context.Schemas.Where(s => !s.Archived && s.Name == Name).AnyAsync();
}
