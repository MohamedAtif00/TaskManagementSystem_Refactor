using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
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
        string Description
    )
    {
        var doesExists = await CheckIfExists(Name);

        if (doesExists)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Schema already exists" }
            );

        var schema = new Schema
        {
            Archived = false,
            Name = Name,
            Description = Description,
            Nodes = { },
            LearningObjectives = { }
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
            }
        };
    }

    private async Task<bool> CheckIfExists(string Name) =>
        await _context.Schemas.Where(s => !s.Archived && s.Name == Name).AnyAsync();
}
