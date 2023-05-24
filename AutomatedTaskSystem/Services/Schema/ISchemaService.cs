using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.SchemaService;

public interface ISchemaService
{
    Task<ActionResult<ResponseService<Responses.SchemaDTO>>> CreateSchema(
        string Name,
        string Description
    );
}
