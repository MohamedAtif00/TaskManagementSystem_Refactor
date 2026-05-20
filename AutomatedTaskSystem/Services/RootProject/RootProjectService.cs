using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.EntityFrameworkCore;

namespace AutomatedTaskSystem.Services.RootProjectService;

public interface IRootProjectService
{
    Task<ResponseService<List<RootProjectListDto>>> GetAllAsync();
    Task<ResponseService<RootProjectDetailDto>> GetAsync(int rootProjectId);
    Task<ResponseService<RootProjectListDto>> CreateAsync(string name, string? description);
    Task<ResponseService<RootProjectDetailDto>> UpdateAsync(int rootProjectId, string name, string? description);
    Task<ResponseService<List<Responses.IDName>>> GetYearsAsync(int rootProjectId);
    Task<ResponseService<ProjectYearDetailDto>> GetYearAsync(int projectYearId);
    Task<ResponseService<Responses.IDName>> CreateYearAsync(int rootProjectId, string label);
    Task<ResponseService<ProjectYearDetailDto>> UpdateYearAsync(int projectYearId, string label);
    Task<ResponseService<List<ProjectTermListDto>>> GetTermsAsync(int projectYearId);
    Task<ResponseService<ProjectTermDetailDto>> GetTermAsync(int termId);
    Task<ResponseService<ProjectTermListDto>> CreateTermAsync(
        int projectYearId,
        string name,
        int order,
        DateTime? startDate,
        DateTime? endDate);
    Task<ResponseService<ProjectTermDetailDto>> UpdateTermAsync(
        int termId,
        string name,
        int order,
        DateTime? startDate,
        DateTime? endDate);
}

public class RootProjectService(DataContext context) : IRootProjectService
{
    public async Task<ResponseService<List<RootProjectListDto>>> GetAllAsync()
    {
        var list = await context.RootProjects
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RootProjectListDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description ?? "",
            })
            .ToListAsync();

        return new ResponseService<List<RootProjectListDto>>
        {
            Data = list,
            Error = false,
            Message = "Root projects",
        };
    }

    public async Task<ResponseService<RootProjectDetailDto>> GetAsync(int rootProjectId)
    {
        var entity = await context.RootProjects.AsNoTracking().FirstOrDefaultAsync(r => r.Id == rootProjectId);
        if (entity is null)
            return new ResponseService<RootProjectDetailDto> { Error = true, Message = "Root project not found" };

        return new ResponseService<RootProjectDetailDto>
        {
            Data = new RootProjectDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description ?? "",
            },
            Error = false,
            Message = "Root project",
        };
    }

    public async Task<ResponseService<RootProjectListDto>> CreateAsync(string name, string? description)
    {
        var entity = new RootProject { Name = name, Description = description };
        context.RootProjects.Add(entity);
        await context.SaveChangesAsync();

        return new ResponseService<RootProjectListDto>
        {
            Data = new RootProjectListDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description ?? "",
            },
            Error = false,
            Message = "Root project created",
        };
    }

    public async Task<ResponseService<RootProjectDetailDto>> UpdateAsync(
        int rootProjectId,
        string name,
        string? description)
    {
        var entity = await context.RootProjects.FirstOrDefaultAsync(r => r.Id == rootProjectId);
        if (entity is null)
            return new ResponseService<RootProjectDetailDto> { Error = true, Message = "Root project not found" };

        entity.Name = name;
        entity.Description = description;
        await context.SaveChangesAsync();

        return new ResponseService<RootProjectDetailDto>
        {
            Data = new RootProjectDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description ?? "",
            },
            Error = false,
            Message = "Root project updated",
        };
    }

    public async Task<ResponseService<List<Responses.IDName>>> GetYearsAsync(int rootProjectId)
    {
        var exists = await context.RootProjects.AnyAsync(r => r.Id == rootProjectId);
        if (!exists)
            return new ResponseService<List<Responses.IDName>> { Error = true, Message = "Root project not found" };

        var list = await context.ProjectYears
            .Where(y => y.RootProjectId == rootProjectId)
            .OrderBy(y => y.Label)
            .Select(y => new Responses.IDName { Id = y.Id, Name = y.Label })
            .ToListAsync();

        return new ResponseService<List<Responses.IDName>>
        {
            Data = list,
            Error = false,
            Message = "Project years",
        };
    }

    public async Task<ResponseService<ProjectYearDetailDto>> GetYearAsync(int projectYearId)
    {
        var year = await context.ProjectYears.AsNoTracking().FirstOrDefaultAsync(y => y.Id == projectYearId);
        if (year is null)
            return new ResponseService<ProjectYearDetailDto> { Error = true, Message = "Year not found" };

        return new ResponseService<ProjectYearDetailDto>
        {
            Data = new ProjectYearDetailDto
            {
                Id = year.Id,
                RootProjectId = year.RootProjectId,
                Label = year.Label,
            },
            Error = false,
            Message = "Project year",
        };
    }

    public async Task<ResponseService<Responses.IDName>> CreateYearAsync(int rootProjectId, string label)
    {
        var exists = await context.RootProjects.AnyAsync(r => r.Id == rootProjectId);
        if (!exists)
            return new ResponseService<Responses.IDName> { Error = true, Message = "Root project not found" };

        var year = new ProjectYear { RootProjectId = rootProjectId, Label = label };
        context.ProjectYears.Add(year);
        await context.SaveChangesAsync();

        return new ResponseService<Responses.IDName>
        {
            Data = new Responses.IDName { Id = year.Id, Name = year.Label },
            Error = false,
            Message = "Year created",
        };
    }

    public async Task<ResponseService<ProjectYearDetailDto>> UpdateYearAsync(int projectYearId, string label)
    {
        var year = await context.ProjectYears.FirstOrDefaultAsync(y => y.Id == projectYearId);
        if (year is null)
            return new ResponseService<ProjectYearDetailDto> { Error = true, Message = "Year not found" };

        year.Label = label;
        await context.SaveChangesAsync();

        return new ResponseService<ProjectYearDetailDto>
        {
            Data = new ProjectYearDetailDto
            {
                Id = year.Id,
                RootProjectId = year.RootProjectId,
                Label = year.Label,
            },
            Error = false,
            Message = "Year updated",
        };
    }

    public async Task<ResponseService<List<ProjectTermListDto>>> GetTermsAsync(int projectYearId)
    {
        var exists = await context.ProjectYears.AnyAsync(y => y.Id == projectYearId);
        if (!exists)
            return new ResponseService<List<ProjectTermListDto>> { Error = true, Message = "Year not found" };

        var list = await context.ProjectTerms
            .Where(t => t.ProjectYearId == projectYearId)
            .OrderBy(t => t.Order)
            .ThenBy(t => t.Name)
            .Select(t => new ProjectTermListDto
            {
                Id = t.Id,
                Name = t.Name,
                Order = t.Order,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
            })
            .ToListAsync();

        return new ResponseService<List<ProjectTermListDto>>
        {
            Data = list,
            Error = false,
            Message = "Terms",
        };
    }

    public async Task<ResponseService<ProjectTermDetailDto>> GetTermAsync(int termId)
    {
        var term = await context.ProjectTerms.AsNoTracking().FirstOrDefaultAsync(t => t.Id == termId);
        if (term is null)
            return new ResponseService<ProjectTermDetailDto> { Error = true, Message = "Term not found" };

        return new ResponseService<ProjectTermDetailDto>
        {
            Data = new ProjectTermDetailDto
            {
                Id = term.Id,
                ProjectYearId = term.ProjectYearId,
                Name = term.Name,
                Order = term.Order,
                StartDate = term.StartDate,
                EndDate = term.EndDate,
            },
            Error = false,
            Message = "Term",
        };
    }

    public async Task<ResponseService<ProjectTermListDto>> CreateTermAsync(
        int projectYearId,
        string name,
        int order,
        DateTime? startDate,
        DateTime? endDate)
    {
        var year = await context.ProjectYears.FirstOrDefaultAsync(y => y.Id == projectYearId);
        if (year is null)
            return new ResponseService<ProjectTermListDto> { Error = true, Message = "Year not found" };

        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
            return new ResponseService<ProjectTermListDto>
            {
                Error = true,
                Message = "End date must be on or after start date",
            };

        var term = new ProjectTerm
        {
            ProjectYearId = projectYearId,
            Name = name,
            Order = order,
            StartDate = startDate,
            EndDate = endDate,
        };
        context.ProjectTerms.Add(term);
        await context.SaveChangesAsync();

        return new ResponseService<ProjectTermListDto>
        {
            Data = new ProjectTermListDto
            {
                Id = term.Id,
                Name = term.Name,
                Order = term.Order,
                StartDate = term.StartDate,
                EndDate = term.EndDate,
            },
            Error = false,
            Message = "Term created",
        };
    }

    public async Task<ResponseService<ProjectTermDetailDto>> UpdateTermAsync(
        int termId,
        string name,
        int order,
        DateTime? startDate,
        DateTime? endDate)
    {
        var term = await context.ProjectTerms.FirstOrDefaultAsync(t => t.Id == termId);
        if (term is null)
            return new ResponseService<ProjectTermDetailDto> { Error = true, Message = "Term not found" };

        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
            return new ResponseService<ProjectTermDetailDto>
            {
                Error = true,
                Message = "End date must be on or after start date",
            };

        term.Name = name;
        term.Order = order;
        term.StartDate = startDate;
        term.EndDate = endDate;
        await context.SaveChangesAsync();

        return new ResponseService<ProjectTermDetailDto>
        {
            Data = new ProjectTermDetailDto
            {
                Id = term.Id,
                ProjectYearId = term.ProjectYearId,
                Name = term.Name,
                Order = term.Order,
                StartDate = term.StartDate,
                EndDate = term.EndDate,
            },
            Error = false,
            Message = "Term updated",
        };
    }
}
