using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.YearService;

public class YearService : IYearService
{
    private readonly DataContext _context;

    public YearService(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetActiveYears()
    {
        var years = await _context.Years.Where(y => y.Active).ToListAsync();

        return new ResponseService<List<Responses.IDName>>
        {
            Data = years.Select(y => new Responses.IDName { Id = y.Id, Name = y.Number }).ToList(),
            Error = false,
            Message = "List of all years"
        };
    }
}
