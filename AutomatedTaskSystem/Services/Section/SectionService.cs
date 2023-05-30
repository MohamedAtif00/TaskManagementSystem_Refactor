using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.SectionService;

public class SectionService : ISectionService
{
    private readonly DataContext _context;

    public SectionService(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResult<ResponseService<Responses.IDName>>> CreateSection(
        string Name,
        int HeadId,
        List<int> Groups
    )
    {
        if (await CheckIfExists(Name))
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Section already exists" }
            );

        var head = await _context.Users
            .Where(u => u.Id == HeadId && !u.Archived)
            .FirstOrDefaultAsync();

        if (head is null)
            return new BadRequestObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"User of id:{HeadId} is not found"
                }
            );

        var groups = await _context.Groups.Where(g => Groups.Contains(g.Id)).ToListAsync();

        var newSection = new Section
        {
            Archived = false,
            Groups = groups,
            Head = head,
            HeadId = head.Id,
            Name = Name
        };

        _context.Sections.Add(newSection);
        await _context.SaveChangesAsync();

        return new ResponseService<Responses.IDName>
        {
            Message = "Section created.",
            Data = new Responses.IDName { Id = newSection.Id, Name = newSection.Name },
            Error = false
        };
    }

    public async Task<ActionResult<ResponseService<Responses.SectionDTO>>> GetSection(int Id)
    {
        var section = await _context.Sections
            .Where(s => s.Id == Id)
            .Include(s => s.Groups)
            .Include(s => s.Head)
            .FirstOrDefaultAsync();

        if (section is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Message = "Section is not found.", Error = true }
            );

        return new ResponseService<Responses.SectionDTO>
        {
            Message = "Section created.",
            Data = new Responses.SectionDTO
            {
                Id = section.Id,
                Name = section.Name,
                Groups = section.Groups
                    .Select(g => new Responses.IDName { Id = g.Id, Name = g.Name })
                    .ToList(),
                Head = new Responses.IDName { Id = section.Head.Id, Name = section.Head.Name }
            },
            Error = false
        };
    }

    public async Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetSections()
    {
        var sections = await _context.Sections.Where(s => !s.Archived).ToListAsync();

        return new ResponseService<List<Responses.IDName>>
        {
            Message = "List of sections",
            Data = sections.Select(g => new Responses.IDName { Id = g.Id, Name = g.Name }).ToList(),
            Error = false
        };
    }

    private async Task<bool> CheckIfExists(string Name) =>
        await _context.Sections.AnyAsync(g => g.Name.ToLower() == Name.ToLower());
}
