using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AutomatedTaskSystem.Services.CurriculumService;

public interface ICurriculumService
{
    Task<ResponseService<List<CurriculumNodeDto>>> GetYearsAsync();
    Task<ResponseService<CurriculumNodeDto>> GetYearAsync(int yearId);
    Task<ResponseService<CurriculumNodeDto>> CreateYearAsync(string name, string? description);
    Task<ResponseService<CurriculumNodeDto>> UpdateYearAsync(int yearId, string name, string? description);
    Task<ResponseService<bool>> DeleteYearAsync(int yearId);

    Task<ResponseService<List<CurriculumNodeDto>>> GetYearTreeAsync(int yearId);

    Task<ResponseService<List<CurriculumNodeDto>>> GetProjectsAsync(int yearId);
    Task<ResponseService<CurriculumNodeDto>> GetProjectAsync(int projectId);
    Task<ResponseService<CurriculumNodeDto>> CreateProjectAsync(int yearId, string name, string? description);
    Task<ResponseService<CurriculumNodeDto>> UpdateProjectAsync(int projectId, string name, string? description);
    Task<ResponseService<bool>> DeleteProjectAsync(int projectId);

    Task<ResponseService<List<CurriculumNodeDto>>> GetTermsAsync(int projectId);
    Task<ResponseService<CurriculumNodeDto>> GetTermAsync(int termId);
    Task<ResponseService<CurriculumNodeDto>> CreateTermAsync(int projectId, string name, DateTime? startDate, DateTime? endDate);
    Task<ResponseService<CurriculumNodeDto>> UpdateTermAsync(int termId, string name, DateTime? startDate, DateTime? endDate);
    Task<ResponseService<bool>> DeleteTermAsync(int termId);

    Task<ResponseService<List<CurriculumNodeDto>>> GetSubjectGroupsAsync(int termId);
    Task<ResponseService<CurriculumNodeDto>> GetSubjectGroupAsync(int subjectGroupId);
    Task<ResponseService<CurriculumNodeDto>> CreateSubjectGroupAsync(int termId, string name);
    Task<ResponseService<CurriculumNodeDto>> UpdateSubjectGroupAsync(int subjectGroupId, string name);
    Task<ResponseService<bool>> DeleteSubjectGroupAsync(int subjectGroupId);

    Task<ResponseService<List<CurriculumNodeDto>>> GetSubjectGroupNodesWithSubjectsAsync();
    Task<string> BuildSubjectPathAsync(int subjectGroupId);
}

public class CurriculumService(DataContext context, IMemoryCache cache) : ICurriculumService
{
    public async Task<ResponseService<List<CurriculumNodeDto>>> GetYearsAsync()
    {
        var years = await context.AcademicYears.AsNoTracking().OrderBy(y => y.Name).ToListAsync();
        return Ok(years.Select(y => ToYearNode(y)).ToList());
    }

    public async Task<ResponseService<CurriculumNodeDto>> GetYearAsync(int yearId)
    {
        var year = await context.AcademicYears.AsNoTracking().FirstOrDefaultAsync(y => y.Id == yearId);
        return year is null ? NotFound<CurriculumNodeDto>() : Ok(ToYearNode(year));
    }

    public async Task<ResponseService<CurriculumNodeDto>> CreateYearAsync(string name, string? description)
    {
        var entity = new AcademicYear { Name = name.Trim(), Description = description?.Trim() };
        context.AcademicYears.Add(entity);
        await context.SaveChangesAsync();
        return Ok(ToYearNode(entity));
    }

    public async Task<ResponseService<CurriculumNodeDto>> UpdateYearAsync(int yearId, string name, string? description)
    {
        var year = await context.AcademicYears.FirstOrDefaultAsync(y => y.Id == yearId);
        if (year is null) return NotFound<CurriculumNodeDto>();
        year.Name = name.Trim();
        if (description is not null) year.Description = description.Trim();
        await context.SaveChangesAsync();
        return Ok(ToYearNode(year));
    }

    public async Task<ResponseService<bool>> DeleteYearAsync(int yearId)
    {
        var year = await context.AcademicYears
            .Include(y => y.Projects)
            .ThenInclude(p => p.Terms)
            .ThenInclude(t => t.SubjectGroups)
            .FirstOrDefaultAsync(y => y.Id == yearId);
        if (year is null) return NotFound<bool>();

        var groupIds = year.Projects
            .SelectMany(p => p.Terms)
            .SelectMany(t => t.SubjectGroups)
            .Select(g => g.Id);
        await RemoveSubjectsInGroupsAsync(groupIds);

        foreach (var project in year.Projects)
        {
            foreach (var term in project.Terms)
                context.SubjectGroups.RemoveRange(term.SubjectGroups);
            context.CurriculumTerms.RemoveRange(project.Terms);
        }
        context.CurriculumProjects.RemoveRange(year.Projects);
        context.AcademicYears.Remove(year);
        await context.SaveChangesAsync();
        return Ok(true);
    }

    /// <summary>All descendants of a year (projects, terms, subject groups) as one flat list.</summary>
    public async Task<ResponseService<List<CurriculumNodeDto>>> GetYearTreeAsync(int yearId)
    {
        var year = await context.AcademicYears.AsNoTracking().FirstOrDefaultAsync(y => y.Id == yearId);
        if (year is null) return NotFound<List<CurriculumNodeDto>>();

        var projects = await context.CurriculumProjects.AsNoTracking()
            .Where(p => p.YearId == yearId)
            .OrderBy(p => p.Name)
            .ToListAsync();
        var projectIds = projects.Select(p => p.Id).ToList();

        var terms = await context.CurriculumTerms.AsNoTracking()
            .Where(t => projectIds.Contains(t.ProjectId))
            .OrderBy(t => t.Name)
            .ToListAsync();
        var termIds = terms.Select(t => t.Id).ToList();

        var groups = await context.SubjectGroups.AsNoTracking()
            .Where(g => termIds.Contains(g.TermId))
            .OrderBy(g => g.Name)
            .ToListAsync();

        var nodes = new List<CurriculumNodeDto>();

        var projectPathById = new Dictionary<int, string>();
        foreach (var project in projects)
        {
            nodes.Add(ToProjectNode(project, year.Name));
            projectPathById[project.Id] = $"{year.Name} > {project.Name}";
        }

        var termPathById = new Dictionary<int, string>();
        foreach (var term in terms)
        {
            var prefix = projectPathById[term.ProjectId];
            nodes.Add(ToTermNode(term, prefix));
            termPathById[term.Id] = $"{prefix} > {term.Name}";
        }

        foreach (var group in groups)
            nodes.Add(ToSubjectGroupNode(group, termPathById[group.TermId]));

        return Ok(nodes);
    }

    public async Task<ResponseService<List<CurriculumNodeDto>>> GetProjectsAsync(int yearId)
    {
        if (!await context.AcademicYears.AnyAsync(y => y.Id == yearId))
            return NotFound<List<CurriculumNodeDto>>();
        var year = await context.AcademicYears.AsNoTracking().FirstAsync(y => y.Id == yearId);
        var projects = await context.CurriculumProjects.AsNoTracking()
            .Where(p => p.YearId == yearId)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return Ok(projects.Select(p => ToProjectNode(p, year.Name)).ToList());
    }

    public async Task<ResponseService<CurriculumNodeDto>> GetProjectAsync(int projectId)
    {
        var project = await context.CurriculumProjects.AsNoTracking()
            .Include(p => p.Year)
            .FirstOrDefaultAsync(p => p.Id == projectId);
        return project is null ? NotFound<CurriculumNodeDto>() : Ok(ToProjectNode(project, project.Year.Name));
    }

    public async Task<ResponseService<CurriculumNodeDto>> CreateProjectAsync(int yearId, string name, string? description)
    {
        var year = await context.AcademicYears.AsNoTracking().FirstOrDefaultAsync(y => y.Id == yearId);
        if (year is null) return NotFound<CurriculumNodeDto>();
        var entity = new CurriculumProject { YearId = yearId, Name = name.Trim(), Description = description?.Trim() };
        context.CurriculumProjects.Add(entity);
        await context.SaveChangesAsync();
        return Ok(ToProjectNode(entity, year.Name));
    }

    public async Task<ResponseService<CurriculumNodeDto>> UpdateProjectAsync(int projectId, string name, string? description)
    {
        var project = await context.CurriculumProjects.Include(p => p.Year).FirstOrDefaultAsync(p => p.Id == projectId);
        if (project is null) return NotFound<CurriculumNodeDto>();
        project.Name = name.Trim();
        if (description is not null) project.Description = description.Trim();
        await context.SaveChangesAsync();
        return Ok(ToProjectNode(project, project.Year.Name));
    }

    public async Task<ResponseService<bool>> DeleteProjectAsync(int projectId)
    {
        var project = await context.CurriculumProjects
            .Include(p => p.Terms)
            .ThenInclude(t => t.SubjectGroups)
            .FirstOrDefaultAsync(p => p.Id == projectId);
        if (project is null) return NotFound<bool>();

        var groupIds = project.Terms.SelectMany(t => t.SubjectGroups).Select(g => g.Id);
        await RemoveSubjectsInGroupsAsync(groupIds);

        foreach (var term in project.Terms)
            context.SubjectGroups.RemoveRange(term.SubjectGroups);
        context.CurriculumTerms.RemoveRange(project.Terms);
        context.CurriculumProjects.Remove(project);
        await context.SaveChangesAsync();
        return Ok(true);
    }

    public async Task<ResponseService<List<CurriculumNodeDto>>> GetTermsAsync(int projectId)
    {
        var project = await context.CurriculumProjects.AsNoTracking().Include(p => p.Year).FirstOrDefaultAsync(p => p.Id == projectId);
        if (project is null) return NotFound<List<CurriculumNodeDto>>();
        var prefix = $"{project.Year.Name} > {project.Name}";
        var terms = await context.CurriculumTerms.AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.Name)
            .ToListAsync();
        return Ok(terms.Select(t => ToTermNode(t, prefix)).ToList());
    }

    public async Task<ResponseService<CurriculumNodeDto>> GetTermAsync(int termId)
    {
        var term = await context.CurriculumTerms.AsNoTracking()
            .Include(t => t.Project).ThenInclude(p => p.Year)
            .FirstOrDefaultAsync(t => t.Id == termId);
        if (term is null) return NotFound<CurriculumNodeDto>();
        var prefix = $"{term.Project.Year.Name} > {term.Project.Name}";
        return Ok(ToTermNode(term, prefix));
    }

    public async Task<ResponseService<CurriculumNodeDto>> CreateTermAsync(int projectId, string name, DateTime? startDate, DateTime? endDate)
    {
        var project = await context.CurriculumProjects.AsNoTracking().Include(p => p.Year).FirstOrDefaultAsync(p => p.Id == projectId);
        if (project is null) return NotFound<CurriculumNodeDto>();
        var entity = new CurriculumTerm
        {
            ProjectId = projectId,
            Name = name.Trim(),
            StartDate = startDate,
            EndDate = endDate,
        };
        context.CurriculumTerms.Add(entity);
        await context.SaveChangesAsync();
        return Ok(ToTermNode(entity, $"{project.Year.Name} > {project.Name}"));
    }

    public async Task<ResponseService<CurriculumNodeDto>> UpdateTermAsync(int termId, string name, DateTime? startDate, DateTime? endDate)
    {
        var term = await context.CurriculumTerms.Include(t => t.Project).ThenInclude(p => p.Year).FirstOrDefaultAsync(t => t.Id == termId);
        if (term is null) return NotFound<CurriculumNodeDto>();
        term.Name = name.Trim();
        term.StartDate = startDate;
        term.EndDate = endDate;
        await context.SaveChangesAsync();
        return Ok(ToTermNode(term, $"{term.Project.Year.Name} > {term.Project.Name}"));
    }

    public async Task<ResponseService<bool>> DeleteTermAsync(int termId)
    {
        var term = await context.CurriculumTerms
            .Include(t => t.SubjectGroups)
            .FirstOrDefaultAsync(t => t.Id == termId);
        if (term is null) return NotFound<bool>();

        await RemoveSubjectsInGroupsAsync(term.SubjectGroups.Select(g => g.Id));
        context.SubjectGroups.RemoveRange(term.SubjectGroups);
        context.CurriculumTerms.Remove(term);
        await context.SaveChangesAsync();
        return Ok(true);
    }

    public async Task<ResponseService<List<CurriculumNodeDto>>> GetSubjectGroupsAsync(int termId)
    {
        var term = await context.CurriculumTerms.AsNoTracking()
            .Include(t => t.Project).ThenInclude(p => p.Year)
            .FirstOrDefaultAsync(t => t.Id == termId);
        if (term is null) return NotFound<List<CurriculumNodeDto>>();
        var prefix = $"{term.Project.Year.Name} > {term.Project.Name} > {term.Name}";
        var groups = await context.SubjectGroups.AsNoTracking()
            .Where(g => g.TermId == termId)
            .OrderBy(g => g.Name)
            .ToListAsync();
        return Ok(groups.Select(g => ToSubjectGroupNode(g, prefix)).ToList());
    }

    public async Task<ResponseService<CurriculumNodeDto>> GetSubjectGroupAsync(int subjectGroupId)
    {
        var group = await context.SubjectGroups.AsNoTracking()
            .Include(g => g.Term).ThenInclude(t => t.Project).ThenInclude(p => p.Year)
            .FirstOrDefaultAsync(g => g.Id == subjectGroupId);
        if (group is null) return NotFound<CurriculumNodeDto>();
        var prefix = $"{group.Term.Project.Year.Name} > {group.Term.Project.Name} > {group.Term.Name}";
        return Ok(ToSubjectGroupNode(group, prefix));
    }

    public async Task<ResponseService<CurriculumNodeDto>> CreateSubjectGroupAsync(int termId, string name)
    {
        var term = await context.CurriculumTerms.AsNoTracking()
            .Include(t => t.Project).ThenInclude(p => p.Year)
            .FirstOrDefaultAsync(t => t.Id == termId);
        if (term is null) return NotFound<CurriculumNodeDto>();
        var entity = new SubjectGroup { TermId = termId, Name = name.Trim() };
        context.SubjectGroups.Add(entity);
        await context.SaveChangesAsync();
        var prefix = $"{term.Project.Year.Name} > {term.Project.Name} > {term.Name}";
        return Ok(ToSubjectGroupNode(entity, prefix));
    }

    public async Task<ResponseService<CurriculumNodeDto>> UpdateSubjectGroupAsync(int subjectGroupId, string name)
    {
        var group = await context.SubjectGroups
            .Include(g => g.Term).ThenInclude(t => t.Project).ThenInclude(p => p.Year)
            .FirstOrDefaultAsync(g => g.Id == subjectGroupId);
        if (group is null) return NotFound<CurriculumNodeDto>();
        group.Name = name.Trim();
        await context.SaveChangesAsync();
        var prefix = $"{group.Term.Project.Year.Name} > {group.Term.Project.Name} > {group.Term.Name}";
        return Ok(ToSubjectGroupNode(group, prefix));
    }

    public async Task<ResponseService<bool>> DeleteSubjectGroupAsync(int subjectGroupId)
    {
        var group = await context.SubjectGroups
            .FirstOrDefaultAsync(g => g.Id == subjectGroupId);
        if (group is null) return NotFound<bool>();

        await RemoveSubjectsInGroupsAsync([subjectGroupId]);
        context.SubjectGroups.Remove(group);
        await context.SaveChangesAsync();
        return Ok(true);
    }

    public async Task<ResponseService<List<CurriculumNodeDto>>> GetSubjectGroupNodesWithSubjectsAsync()
    {
        var groupIds = await context.Subjects.Where(s => !s.Archived).Select(s => s.SubjectGroupId).Distinct().ToListAsync();
        var groups = await context.SubjectGroups.AsNoTracking()
            .Include(g => g.Term).ThenInclude(t => t.Project).ThenInclude(p => p.Year)
            .Where(g => groupIds.Contains(g.Id))
            .OrderBy(g => g.Name)
            .ToListAsync();
        return Ok(groups.Select(g =>
        {
            var prefix = $"{g.Term.Project.Year.Name} > {g.Term.Project.Name} > {g.Term.Name}";
            return ToSubjectGroupNode(g, prefix);
        }).ToList());
    }

    public async Task<string> BuildSubjectPathAsync(int subjectGroupId)
    {
        var group = await context.SubjectGroups.AsNoTracking()
            .Include(g => g.Term).ThenInclude(t => t.Project).ThenInclude(p => p.Year)
            .FirstOrDefaultAsync(g => g.Id == subjectGroupId);
        if (group is null) return string.Empty;
        return $"{group.Term.Project.Year.Name} > {group.Term.Project.Name} > {group.Term.Name} > {group.Name}";
    }

    private async System.Threading.Tasks.Task RemoveSubjectsInGroupsAsync(IEnumerable<int> subjectGroupIds)
    {
        var ids = subjectGroupIds.Distinct().ToList();
        if (ids.Count == 0) return;

        var subjects = await context.Subjects
            .Where(s => ids.Contains(s.SubjectGroupId))
            .Include(s => s.Users)
            .ToListAsync();

        foreach (var subject in subjects)
            subject.Users.Clear();

        if (subjects.Count == 0) return;

        context.Subjects.RemoveRange(subjects);
        cache.Remove("AllSubjects");
        await context.SaveChangesAsync();
    }

    private static CurriculumNodeDto ToYearNode(AcademicYear year) => new()
    {
        Id = year.Id,
        Name = year.Name,
        Description = year.Description,
        LevelName = CurriculumHierarchy.RootLevelName,
        Depth = 0,
        Path = year.Name,
        ParentId = null,
        NodeType = "year",
    };

    private static CurriculumNodeDto ToProjectNode(CurriculumProject project, string yearName) => new()
    {
        Id = project.Id,
        Name = project.Name,
        LevelName = CurriculumHierarchy.ChildLevelNames[0],
        Depth = 1,
        Path = $"{yearName} > {project.Name}",
        ParentId = project.YearId,
        NodeType = "project",
    };

    private static CurriculumNodeDto ToTermNode(CurriculumTerm term, string prefix) => new()
    {
        Id = term.Id,
        Name = term.Name,
        LevelName = CurriculumHierarchy.ChildLevelNames[1],
        Depth = 2,
        Path = $"{prefix} > {term.Name}",
        ParentId = term.ProjectId,
        NodeType = "term",
    };

    private static CurriculumNodeDto ToSubjectGroupNode(SubjectGroup group, string prefix) => new()
    {
        Id = group.Id,
        Name = group.Name,
        LevelName = CurriculumHierarchy.ChildLevelNames[2],
        Depth = 3,
        Path = $"{prefix} > {group.Name}",
        ParentId = group.TermId,
        NodeType = "subjectGroup",
    };

    private static ResponseService<T> Ok<T>(T data) => new() { Data = data, Error = false, Message = "OK" };

    private static ResponseService<T> NotFound<T>() => new() { Error = true, Message = "Not found" };

    private static ResponseService<T> Fail<T>(string message) => new() { Error = true, Message = message };
}
