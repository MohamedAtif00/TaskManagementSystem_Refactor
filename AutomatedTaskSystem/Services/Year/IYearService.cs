using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.YearService;

public interface IYearService
{
    Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetActiveYears();
}
