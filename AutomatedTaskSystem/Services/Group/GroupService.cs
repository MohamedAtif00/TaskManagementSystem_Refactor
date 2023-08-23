using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.GroupService;

public class GroupService : IGroupService
{
	private readonly DataContext _context;
	public GroupService(DataContext context)
	{
		_context = context;
	}
	public async Task<ActionResult<ResponseService<Responses.GroupDTO>>> CreateGroup(string Name, string ColorCode, Section Section)
	{
		if (await checkIfGroupExists(Name))
			return new BadRequestObjectResult(new BaseResponseService
			{
				Error = true,
				Message = "Group already exists"
			});

		var group = new Group
		{
			Archived = false,
			ColorCode = ColorCode,
			Name = Name,
			Section = Section,
			SectionId = Section.Id
		};

		_context.Groups.Add(group);
		await _context.SaveChangesAsync();

		return new ResponseService<Responses.GroupDTO>
		{
			Error = false,
			Message = "Group created.",
			Data = new Responses.GroupDTO
			{
				ColorCode = group.ColorCode,
				Id = group.Id,
				Members = 0,
				Name = group.Name
			}
		};
	}
	public async Task<ActionResult<ResponseService<Responses.GroupDTO>>> CreateGroup(string Name, string ColorCode)
	{
		var groupExists = await _context.Groups
			.AnyAsync(g => g.Name.ToLower() == Name.ToLower());
		if (groupExists)
			return new BadRequestObjectResult(new BaseResponseService
			{
				Error = true,
				Message = "Group already exists"
			});

		var group = new Group
		{
			Archived = false,
			ColorCode = ColorCode,
			Name = Name,
			Section = null,
			SectionId = null
		};

		_context.Groups.Add(group);
		await _context.SaveChangesAsync();

		return new ResponseService<Responses.GroupDTO>
		{
			Error = false,
			Message = "Group created.",
			Data = new Responses.GroupDTO
			{
				ColorCode = group.ColorCode,
				Id = group.Id,
				Members = 0,
				Name = group.Name
			}
		};
	}
	public async Task<ActionResult<ResponseService<Responses.GroupDTO>>> EditGroup(int Id, string Name, string ColorCode)
	{
		var group = await _context.Groups
			.Where(g => g.Id == Id)
			.Include(g => g.Users)
			.FirstOrDefaultAsync();

		if (group is null)
			return new NotFoundObjectResult(new BaseResponseService
			{
				Error = true,
				Message = "Group already exists"
			});

		group.Name = Name;
		group.ColorCode = ColorCode;

		await _context.SaveChangesAsync();

		return new ResponseService<Responses.GroupDTO>
		{
			Data = new Responses.GroupDTO
			{
				Id = group.Id,
				Name = group.Name,
				Members = group.Users.Count,
				ColorCode = group.ColorCode
			},
			Error = false,
			Message = "Group Found"
		};
	}
	public async Task<ActionResult<ResponseService<List<Responses.GroupDTO>>>> GetAllGroups()
	{
		var groups = await _context.Groups
			.Where(g => !g.Archived)
			.Include(g => g.Users)
			.AsNoTracking()
			.ToListAsync();

		return new ResponseService<List<Responses.GroupDTO>>
		{
			Data = groups.Select(g => new Responses.GroupDTO
			{
				Id = g.Id,
				Name = g.Name,
				ColorCode = g.ColorCode,
				Members = g.Users.Count
			}).ToList(),
			Error = false,
			Message = "List of all non-archived Groups"
		};
	}

	private async Task<bool> checkIfGroupExists(string Name) =>
		await _context.Groups
			.AsNoTracking()
			.AnyAsync(g => g.Name.ToLower() == Name.ToLower());

	public async Task<ActionResult<ResponseService<Responses.GroupDTO>>> FindGroup(int Id)
	{
		var group = await _context.Groups
			.Where(g => g.Id == Id)
			.Include(g => g.Users)
			.AsNoTracking()
			.FirstOrDefaultAsync();

		if (group is null)
			return new NotFoundObjectResult(new BaseResponseService
			{
				Error = true,
				Message = "Group already exists."
			});

		return new ResponseService<Responses.GroupDTO>
		{
			Error = false,
			Data = new Responses.GroupDTO
			{
				Id = group.Id,
				Name = group.Name,
				Members = group.Users.Count,
				ColorCode = group.ColorCode
			},
			Message = "Group is found."
		};
	}

	public async Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetAllGroupsSimple()
	{
		var groups = await _context.Groups
			.Where(g => !g.Archived)
			.Include(g => g.Users)
			.AsNoTracking()
			.ToListAsync();

		return new ResponseService<List<Responses.IDName>>
		{
			Data = groups.Select(g => new Responses.IDName
			{
				Id = g.Id,
				Name = g.Name,
			}).ToList(),
			Error = false,
			Message = "List of all non-archived Groups"
		};
	}
}
