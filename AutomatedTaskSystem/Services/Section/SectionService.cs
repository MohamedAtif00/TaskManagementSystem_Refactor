using System.Xml.Linq;
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
    string name,
    int headId,
    List<int> groupIds
    )
    {
        if (await CheckIfExists(name))
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Section already exists" }
            );

        var head = await _context.Users
            .Where(u => u.Id == headId && !u.Archived)
            .FirstOrDefaultAsync();

        if (head is null)
            return new BadRequestObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"User of id:{headId} is not found"
                }
            );



        var groups = await _context.Groups
            .Where(g => groupIds.Contains(g.Id))
            .ToListAsync();

        var newSection = new Section
        {
            Name = name,
            Head = head,
            HeadId = head.Id,
            Archived = false,
            SectionGroups = groups.Select(g => new SectionGroup
            {
                GroupId = g.Id
            }).ToList()
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


    public async Task<ActionResult<ResponseService<Responses.SectionDTO>>> GetSection(int id)
    {
        var section = await _context.Sections
            .Where(s => s.Id == id)
            .Include(s => s.SectionGroups)
                .ThenInclude(sg => sg.Group)
            .Include(s => s.Head)
            .FirstOrDefaultAsync();

        if (section is null)
            return new NotFoundObjectResult(new BaseResponseService
            {
                Message = "Section not found.",
                Error = true
            });

        var groupList = section.SectionGroups
            .Select(sg => new Responses.IDName
            {
                Id = sg.Group.Id,
                Name = sg.Group.Name
            }).ToList();

        var response = new Responses.SectionDTO
        {
            Id = section.Id,
            Name = section.Name,
            Groups = groupList,
            Head = new Responses.IDName
            {
                Id = section.Head.Id,
                Name = section.Head.Name
            }
        };

        return new ResponseService<Responses.SectionDTO>
        {
            Message = "Section fetched successfully.",
            Data = response,
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

    public async Task<ActionResult<ResponseService<Responses.SectionDTO>>> EditSection(int id, Requests.SectionDTO request)
    {
        var head = await _context.Users
            .Where(u => u.Id == request.HeadId && !u.Archived)
            .FirstOrDefaultAsync();

        if (head is null)
        {
            return new BadRequestObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"User of id:{request.HeadId} is not found"
                });
        }

        var groups = await _context.Groups
            .Where(g => request.Groups.Contains(g.Id))
            .ToListAsync();

        var getSection = await _context.Sections
            .Include(s => s.SectionGroups)
            .ThenInclude(sg => sg.Group)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (getSection == null)
            return new NotFoundObjectResult(
                new BaseResponseService
                {
                    Message = "Section not found.",
                    Error = true
                }
                );

        // Update properties
        getSection.Name = request.Name;
        getSection.Head = head;
        getSection.HeadId = head.Id;
        getSection.Archived = false;

        // Remove existing SectionGroups
        _context.SectionGroups.RemoveRange(getSection.SectionGroups);

        // Add new SectionGroups
        getSection.SectionGroups = groups.Select(g => new SectionGroup
        {
            SectionId = id,
            GroupId = g.Id
        }).ToList();

        await _context.SaveChangesAsync();

        return new ResponseService<Responses.SectionDTO>
        {
            Message = "Section updated.",
            Data = new Responses.SectionDTO
            {
                Id = getSection.Id,
                Name = getSection.Name,
                Head = new Responses.IDName
                {
                    Id = getSection.HeadId,
                    Name = getSection.Head?.Name ?? ""
                },
                Groups = getSection.SectionGroups.Select(sg => new Responses.IDName
                {
                    Id = sg.Group.Id,
                    Name = sg.Group.Name
                }).ToList()
            },
            Error = false
        };
    }


    public async Task<ActionResult<BaseResponseService>> DeleteSection(int id)
    {
        var section = await _context.Sections.FirstOrDefaultAsync(s => s.Id == id && !s.Archived);

        if (section is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Message = "Section not found.", Error = true }
            );

        section.Archived = true;
        await _context.SaveChangesAsync();

        return new BaseResponseService { Message = "Section deleted.", Error = false };
    }

    private async Task<bool> CheckIfExists(string Name) =>
        await _context.Sections.AnyAsync(g => g.Name.ToLower() == Name.ToLower());
}