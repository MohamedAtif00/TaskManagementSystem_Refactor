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

    Task<ResponseService<List<ArchivedCurriculumTreeNodeDto>>> GetArchivedCurriculumAsync();
    Task<ResponseService<bool>> RestoreYearAsync(int yearId, IReadOnlyList<int>? subjectIds = null);
    Task<ResponseService<bool>> RestoreProjectAsync(int projectId, IReadOnlyList<int>? subjectIds = null);
    Task<ResponseService<bool>> RestoreTermAsync(int termId, IReadOnlyList<int>? subjectIds = null);
    Task<ResponseService<bool>> RestoreSubjectGroupAsync(int subjectGroupId, IReadOnlyList<int>? subjectIds = null);
}

public class CurriculumService(DataContext context, IMemoryCache cache) : ICurriculumService
{
    public async Task<ResponseService<List<CurriculumNodeDto>>> GetYearsAsync()
    {
        var years = await context.AcademicYears.AsNoTracking()
            .Where(y => !y.Archived)
            .OrderBy(y => y.Name)
            .ToListAsync();
        return Ok(years.Select(y => ToYearNode(y)).ToList());
    }

    public async Task<ResponseService<CurriculumNodeDto>> GetYearAsync(int yearId)
    {
        var year = await context.AcademicYears.AsNoTracking()
            .FirstOrDefaultAsync(y => y.Id == yearId && !y.Archived);
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
        var year = await context.AcademicYears.FirstOrDefaultAsync(y => y.Id == yearId && !y.Archived);
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
            .FirstOrDefaultAsync(y => y.Id == yearId && !y.Archived);
        if (year is null) return NotFound<bool>();

        year.Archived = true;
        foreach (var project in year.Projects)
        {
            project.Archived = true;
            foreach (var term in project.Terms)
            {
                term.Archived = true;
                foreach (var group in term.SubjectGroups)
                    group.Archived = true;
            }
        }

        var groupIds = year.Projects
            .SelectMany(p => p.Terms)
            .SelectMany(t => t.SubjectGroups)
            .Select(g => g.Id);
        await ArchiveSubjectsInGroupsAsync(groupIds);
        await context.SaveChangesAsync();
        return Ok(true);
    }

    public async Task<ResponseService<List<CurriculumNodeDto>>> GetYearTreeAsync(int yearId)
    {
        var year = await context.AcademicYears.AsNoTracking()
            .FirstOrDefaultAsync(y => y.Id == yearId && !y.Archived);
        if (year is null) return NotFound<List<CurriculumNodeDto>>();

        var projects = await context.CurriculumProjects.AsNoTracking()
            .Where(p => p.YearId == yearId && !p.Archived)
            .OrderBy(p => p.Name)
            .ToListAsync();
        var projectIds = projects.Select(p => p.Id).ToList();

        var terms = await context.CurriculumTerms.AsNoTracking()
            .Where(t => projectIds.Contains(t.ProjectId) && !t.Archived)
            .OrderBy(t => t.Name)
            .ToListAsync();
        var termIds = terms.Select(t => t.Id).ToList();

        var groups = await context.SubjectGroups.AsNoTracking()
            .Where(g => termIds.Contains(g.TermId) && !g.Archived)
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
        var year = await context.AcademicYears.AsNoTracking()
            .FirstOrDefaultAsync(y => y.Id == yearId && !y.Archived);
        if (year is null) return NotFound<List<CurriculumNodeDto>>();

        var projects = await context.CurriculumProjects.AsNoTracking()
            .Where(p => p.YearId == yearId && !p.Archived)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return Ok(projects.Select(p => ToProjectNode(p, year.Name)).ToList());
    }

    public async Task<ResponseService<CurriculumNodeDto>> GetProjectAsync(int projectId)
    {
        var project = await context.CurriculumProjects.AsNoTracking()
            .Include(p => p.Year)
            .FirstOrDefaultAsync(p => p.Id == projectId && !p.Archived);
        return project is null ? NotFound<CurriculumNodeDto>() : Ok(ToProjectNode(project, project.Year.Name));
    }

    public async Task<ResponseService<CurriculumNodeDto>> CreateProjectAsync(int yearId, string name, string? description)
    {
        var year = await context.AcademicYears.AsNoTracking()
            .FirstOrDefaultAsync(y => y.Id == yearId && !y.Archived);
        if (year is null) return NotFound<CurriculumNodeDto>();
        var entity = new CurriculumProject { YearId = yearId, Name = name.Trim(), Description = description?.Trim() };
        context.CurriculumProjects.Add(entity);
        await context.SaveChangesAsync();
        return Ok(ToProjectNode(entity, year.Name));
    }

    public async Task<ResponseService<CurriculumNodeDto>> UpdateProjectAsync(int projectId, string name, string? description)
    {
        var project = await context.CurriculumProjects
            .Include(p => p.Year)
            .FirstOrDefaultAsync(p => p.Id == projectId && !p.Archived);
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
            .FirstOrDefaultAsync(p => p.Id == projectId && !p.Archived);
        if (project is null) return NotFound<bool>();

        project.Archived = true;
        foreach (var term in project.Terms)
        {
            term.Archived = true;
            foreach (var group in term.SubjectGroups)
                group.Archived = true;
        }

        var groupIds = project.Terms.SelectMany(t => t.SubjectGroups).Select(g => g.Id);
        await ArchiveSubjectsInGroupsAsync(groupIds);
        await context.SaveChangesAsync();
        return Ok(true);
    }

    public async Task<ResponseService<List<CurriculumNodeDto>>> GetTermsAsync(int projectId)
    {
        var project = await context.CurriculumProjects.AsNoTracking()
            .Include(p => p.Year)
            .FirstOrDefaultAsync(p => p.Id == projectId && !p.Archived);
        if (project is null) return NotFound<List<CurriculumNodeDto>>();
        var prefix = $"{project.Year.Name} > {project.Name}";
        var terms = await context.CurriculumTerms.AsNoTracking()
            .Where(t => t.ProjectId == projectId && !t.Archived)
            .OrderBy(t => t.Name)
            .ToListAsync();
        return Ok(terms.Select(t => ToTermNode(t, prefix)).ToList());
    }

    public async Task<ResponseService<CurriculumNodeDto>> GetTermAsync(int termId)
    {
        var term = await context.CurriculumTerms.AsNoTracking()
            .Include(t => t.Project).ThenInclude(p => p.Year)
            .FirstOrDefaultAsync(t => t.Id == termId && !t.Archived);
        if (term is null) return NotFound<CurriculumNodeDto>();
        var prefix = $"{term.Project.Year.Name} > {term.Project.Name}";
        return Ok(ToTermNode(term, prefix));
    }

    public async Task<ResponseService<CurriculumNodeDto>> CreateTermAsync(int projectId, string name, DateTime? startDate, DateTime? endDate)
    {
        var project = await context.CurriculumProjects.AsNoTracking()
            .Include(p => p.Year)
            .FirstOrDefaultAsync(p => p.Id == projectId && !p.Archived);
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
        var term = await context.CurriculumTerms
            .Include(t => t.Project).ThenInclude(p => p.Year)
            .FirstOrDefaultAsync(t => t.Id == termId && !t.Archived);
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
            .FirstOrDefaultAsync(t => t.Id == termId && !t.Archived);
        if (term is null) return NotFound<bool>();

        term.Archived = true;
        foreach (var group in term.SubjectGroups)
            group.Archived = true;

        await ArchiveSubjectsInGroupsAsync(term.SubjectGroups.Select(g => g.Id));
        await context.SaveChangesAsync();
        return Ok(true);
    }

    public async Task<ResponseService<List<CurriculumNodeDto>>> GetSubjectGroupsAsync(int termId)
    {
        var term = await context.CurriculumTerms.AsNoTracking()
            .Include(t => t.Project).ThenInclude(p => p.Year)
            .FirstOrDefaultAsync(t => t.Id == termId && !t.Archived);
        if (term is null) return NotFound<List<CurriculumNodeDto>>();
        var prefix = $"{term.Project.Year.Name} > {term.Project.Name} > {term.Name}";
        var groups = await context.SubjectGroups.AsNoTracking()
            .Where(g => g.TermId == termId && !g.Archived)
            .OrderBy(g => g.Name)
            .ToListAsync();
        return Ok(groups.Select(g => ToSubjectGroupNode(g, prefix)).ToList());
    }

    public async Task<ResponseService<CurriculumNodeDto>> GetSubjectGroupAsync(int subjectGroupId)
    {
        var group = await context.SubjectGroups.AsNoTracking()
            .Include(g => g.Term).ThenInclude(t => t.Project).ThenInclude(p => p.Year)
            .FirstOrDefaultAsync(g => g.Id == subjectGroupId && !g.Archived);
        if (group is null) return NotFound<CurriculumNodeDto>();
        var prefix = $"{group.Term.Project.Year.Name} > {group.Term.Project.Name} > {group.Term.Name}";
        return Ok(ToSubjectGroupNode(group, prefix));
    }

    public async Task<ResponseService<CurriculumNodeDto>> CreateSubjectGroupAsync(int termId, string name)
    {
        var term = await context.CurriculumTerms.AsNoTracking()
            .Include(t => t.Project).ThenInclude(p => p.Year)
            .FirstOrDefaultAsync(t => t.Id == termId && !t.Archived);
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
            .FirstOrDefaultAsync(g => g.Id == subjectGroupId && !g.Archived);
        if (group is null) return NotFound<CurriculumNodeDto>();
        group.Name = name.Trim();
        await context.SaveChangesAsync();
        var prefix = $"{group.Term.Project.Year.Name} > {group.Term.Project.Name} > {group.Term.Name}";
        return Ok(ToSubjectGroupNode(group, prefix));
    }

    public async Task<ResponseService<bool>> DeleteSubjectGroupAsync(int subjectGroupId)
    {
        var group = await context.SubjectGroups
            .FirstOrDefaultAsync(g => g.Id == subjectGroupId && !g.Archived);
        if (group is null) return NotFound<bool>();

        group.Archived = true;
        await ArchiveSubjectsInGroupsAsync([subjectGroupId]);
        await context.SaveChangesAsync();
        return Ok(true);
    }

    public async Task<ResponseService<List<CurriculumNodeDto>>> GetSubjectGroupNodesWithSubjectsAsync()
    {
        var groupIds = await context.Subjects
            .Where(s => !s.Archived)
            .Select(s => s.SubjectGroupId)
            .Distinct()
            .ToListAsync();
        var groups = await context.SubjectGroups.AsNoTracking()
            .Include(g => g.Term).ThenInclude(t => t.Project).ThenInclude(p => p.Year)
            .Where(g => groupIds.Contains(g.Id) && !g.Archived)
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
            .FirstOrDefaultAsync(g => g.Id == subjectGroupId && !g.Archived);
        if (group is null) return string.Empty;
        if (group.Term.Archived || group.Term.Project.Archived || group.Term.Project.Year.Archived)
            return string.Empty;
        return $"{group.Term.Project.Year.Name} > {group.Term.Project.Name} > {group.Term.Name} > {group.Name}";
    }

    public async Task<ResponseService<List<ArchivedCurriculumTreeNodeDto>>> GetArchivedCurriculumAsync()
    {
        var years = await context.AcademicYears.AsNoTracking().ToListAsync();
        var projects = await context.CurriculumProjects.AsNoTracking().ToListAsync();
        var terms = await context.CurriculumTerms.AsNoTracking().ToListAsync();
        var groups = await context.SubjectGroups.AsNoTracking().ToListAsync();

        var subjectsByGroup = await context.Subjects.AsNoTracking()
            .Where(s => s.Archived && s.ArchivedWithFolder)
            .GroupBy(s => s.SubjectGroupId)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(s => new ArchivedSubjectDto { Id = s.Id, Name = s.Name }).ToList());

        var yearById = years.ToDictionary(y => y.Id);
        var projectById = projects.ToDictionary(p => p.Id);
        var termById = terms.ToDictionary(t => t.Id);

        var forest = new List<ArchivedCurriculumTreeNodeDto>();

        foreach (var year in years.Where(y => y.Archived))
            forest.Add(BuildArchivedYearNode(year, projects, terms, groups, subjectsByGroup));

        foreach (var project in projects.Where(p => p.Archived && yearById.TryGetValue(p.YearId, out var y) && !y.Archived))
        {
            var year = yearById[project.YearId];
            forest.Add(new ArchivedCurriculumTreeNodeDto
            {
                Id = year.Id,
                Name = year.Name,
                NodeType = "year",
                Path = year.Name,
                Depth = 0,
                Archived = false,
                Children = [BuildArchivedProjectNode(project, terms, groups, subjectsByGroup, year.Name)],
            });
        }

        foreach (var term in terms.Where(t => t.Archived && projectById.TryGetValue(t.ProjectId, out var p) && !p.Archived))
        {
            var project = projectById[term.ProjectId];
            var year = yearById[project.YearId];
            var projectPath = $"{year.Name} > {project.Name}";
            forest.Add(new ArchivedCurriculumTreeNodeDto
            {
                Id = year.Id,
                Name = year.Name,
                NodeType = "year",
                Path = year.Name,
                Depth = 0,
                Archived = false,
                Children =
                [
                    new ArchivedCurriculumTreeNodeDto
                    {
                        Id = project.Id,
                        Name = project.Name,
                        NodeType = "project",
                        Path = projectPath,
                        Depth = 1,
                        Archived = false,
                        Children = [BuildArchivedTermNode(term, groups, subjectsByGroup, projectPath)],
                    },
                ],
            });
        }

        foreach (var group in groups.Where(g => g.Archived && termById.TryGetValue(g.TermId, out var t) && !t.Archived))
        {
            var term = termById[group.TermId];
            var project = projectById[term.ProjectId];
            var year = yearById[project.YearId];
            forest.Add(BuildArchivedForestRootForSubjectGroup(group, year, project, term, subjectsByGroup));
        }

        foreach (var group in groups.Where(g =>
                     !g.Archived
                     && GroupHasPendingArchivedSubjects(g.Id, subjectsByGroup)
                     && termById.TryGetValue(g.TermId, out var t)
                     && !t.Archived))
        {
            var term = termById[group.TermId];
            var project = projectById[term.ProjectId];
            var year = yearById[project.YearId];
            forest.Add(BuildArchivedForestRootForSubjectGroup(group, year, project, term, subjectsByGroup));
        }

        return Ok(forest.OrderBy(n => n.Path).ToList());
    }

    private static ArchivedCurriculumTreeNodeDto BuildArchivedForestRootForSubjectGroup(
        SubjectGroup group,
        AcademicYear year,
        CurriculumProject project,
        CurriculumTerm term,
        Dictionary<int, List<ArchivedSubjectDto>> subjectsByGroup)
    {
        var projectPath = $"{year.Name} > {project.Name}";
        var termPath = $"{projectPath} > {term.Name}";
        return new ArchivedCurriculumTreeNodeDto
        {
            Id = year.Id,
            Name = year.Name,
            NodeType = "year",
            Path = year.Name,
            Depth = 0,
            Archived = false,
            Children =
            [
                new ArchivedCurriculumTreeNodeDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    NodeType = "project",
                    Path = projectPath,
                    Depth = 1,
                    Archived = false,
                    Children =
                    [
                        new ArchivedCurriculumTreeNodeDto
                        {
                            Id = term.Id,
                            Name = term.Name,
                            NodeType = "term",
                            Path = termPath,
                            Depth = 2,
                            Archived = false,
                            Children = [BuildArchivedSubjectGroupNode(group, subjectsByGroup, termPath)],
                        },
                    ],
                },
            ],
        };
    }

    public async Task<ResponseService<bool>> RestoreYearAsync(int yearId, IReadOnlyList<int>? subjectIds = null)
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
            .Select(g => g.Id)
            .ToList();
        var hasPendingSubjects = await context.Subjects
            .AnyAsync(s => groupIds.Contains(s.SubjectGroupId) && s.Archived && s.ArchivedWithFolder);
        if (!year.Archived && !hasPendingSubjects)
            return NotFound<bool>();

        await UnarchiveSubjectsInGroupsAsync(groupIds, subjectIds);

        year.Archived = false;
        foreach (var project in year.Projects)
        {
            project.Archived = false;
            foreach (var term in project.Terms)
            {
                term.Archived = false;
                foreach (var group in term.SubjectGroups)
                    group.Archived = false;
            }
        }

        await context.SaveChangesAsync();
        return Ok(true);
    }

    public async Task<ResponseService<bool>> RestoreProjectAsync(int projectId, IReadOnlyList<int>? subjectIds = null)
    {
        var project = await context.CurriculumProjects
            .Include(p => p.Year)
            .Include(p => p.Terms)
            .ThenInclude(t => t.SubjectGroups)
            .FirstOrDefaultAsync(p => p.Id == projectId);
        if (project is null) return NotFound<bool>();

        var groupIds = project.Terms.SelectMany(t => t.SubjectGroups).Select(g => g.Id).ToList();
        var hasPendingSubjects = await context.Subjects
            .AnyAsync(s => groupIds.Contains(s.SubjectGroupId) && s.Archived && s.ArchivedWithFolder);
        if (!project.Archived && !hasPendingSubjects)
            return NotFound<bool>();

        if (project.Archived && project.Year.Archived)
            return Fail<bool>("Cannot restore project while its year is archived. Restore the year first.");

        await UnarchiveSubjectsInGroupsAsync(groupIds, subjectIds);

        project.Archived = false;
        foreach (var term in project.Terms)
        {
            term.Archived = false;
            foreach (var group in term.SubjectGroups)
                group.Archived = false;
        }

        await context.SaveChangesAsync();
        return Ok(true);
    }

    public async Task<ResponseService<bool>> RestoreTermAsync(int termId, IReadOnlyList<int>? subjectIds = null)
    {
        var term = await context.CurriculumTerms
            .Include(t => t.Project).ThenInclude(p => p.Year)
            .Include(t => t.SubjectGroups)
            .FirstOrDefaultAsync(t => t.Id == termId);
        if (term is null) return NotFound<bool>();

        var groupIds = term.SubjectGroups.Select(g => g.Id).ToList();
        var hasPendingSubjects = await context.Subjects
            .AnyAsync(s => groupIds.Contains(s.SubjectGroupId) && s.Archived && s.ArchivedWithFolder);
        if (!term.Archived && !hasPendingSubjects)
            return NotFound<bool>();

        if (term.Archived)
        {
            if (term.Project.Archived)
                return Fail<bool>("Cannot restore term while its project is archived. Restore the project first.");
            if (term.Project.Year.Archived)
                return Fail<bool>("Cannot restore term while its year is archived. Restore the year first.");
        }

        await UnarchiveSubjectsInGroupsAsync(groupIds, subjectIds);

        term.Archived = false;
        foreach (var group in term.SubjectGroups)
            group.Archived = false;

        await context.SaveChangesAsync();
        return Ok(true);
    }

    public async Task<ResponseService<bool>> RestoreSubjectGroupAsync(int subjectGroupId, IReadOnlyList<int>? subjectIds = null)
    {
        var group = await context.SubjectGroups
            .Include(g => g.Term).ThenInclude(t => t.Project).ThenInclude(p => p.Year)
            .FirstOrDefaultAsync(g => g.Id == subjectGroupId);
        if (group is null) return NotFound<bool>();

        var hasPendingSubjects = await context.Subjects
            .AnyAsync(s => s.SubjectGroupId == subjectGroupId && s.Archived && s.ArchivedWithFolder);
        if (!group.Archived && !hasPendingSubjects)
            return NotFound<bool>();

        if (group.Archived)
        {
            if (group.Term.Archived)
                return Fail<bool>("Cannot restore subject group while its term is archived. Restore the term first.");
            if (group.Term.Project.Archived)
                return Fail<bool>("Cannot restore subject group while its project is archived. Restore the project first.");
            if (group.Term.Project.Year.Archived)
                return Fail<bool>("Cannot restore subject group while its year is archived. Restore the year first.");
        }

        await UnarchiveSubjectsInGroupsAsync([subjectGroupId], subjectIds);

        // Keep the folder active so restored subjects are visible in the main tree.
        group.Archived = false;

        await context.SaveChangesAsync();
        return Ok(true);
    }

    private static bool GroupHasPendingArchivedSubjects(
        int groupId,
        Dictionary<int, List<ArchivedSubjectDto>> subjectsByGroup) =>
        subjectsByGroup.ContainsKey(groupId);

    private static bool TermHasPendingArchivedSubjects(
        int termId,
        List<SubjectGroup> groups,
        Dictionary<int, List<ArchivedSubjectDto>> subjectsByGroup) =>
        groups.Any(g => g.TermId == termId && GroupHasPendingArchivedSubjects(g.Id, subjectsByGroup));

    private static bool ProjectHasPendingArchivedSubjects(
        int projectId,
        List<CurriculumTerm> terms,
        List<SubjectGroup> groups,
        Dictionary<int, List<ArchivedSubjectDto>> subjectsByGroup) =>
        terms.Where(t => t.ProjectId == projectId)
            .Any(t => TermHasPendingArchivedSubjects(t.Id, groups, subjectsByGroup));

    private static ArchivedCurriculumTreeNodeDto BuildArchivedYearNode(
        AcademicYear year,
        List<CurriculumProject> allProjects,
        List<CurriculumTerm> allTerms,
        List<SubjectGroup> allGroups,
        Dictionary<int, List<ArchivedSubjectDto>> subjectsByGroup)
    {
        var yearPath = year.Name;
        var projectNodes = allProjects
            .Where(p => p.YearId == year.Id && (p.Archived || ProjectHasPendingArchivedSubjects(p.Id, allTerms, allGroups, subjectsByGroup)))
            .OrderBy(p => p.Name)
            .Select(p => BuildArchivedProjectNode(p, allTerms, allGroups, subjectsByGroup, yearPath))
            .ToList();

        return new ArchivedCurriculumTreeNodeDto
        {
            Id = year.Id,
            Name = year.Name,
            NodeType = "year",
            Path = yearPath,
            Depth = 0,
            Archived = true,
            Children = projectNodes,
        };
    }

    private static ArchivedCurriculumTreeNodeDto BuildArchivedProjectNode(
        CurriculumProject project,
        List<CurriculumTerm> allTerms,
        List<SubjectGroup> allGroups,
        Dictionary<int, List<ArchivedSubjectDto>> subjectsByGroup,
        string yearPath)
    {
        var projectPath = $"{yearPath} > {project.Name}";
        var termNodes = allTerms
            .Where(t => t.ProjectId == project.Id && (t.Archived || TermHasPendingArchivedSubjects(t.Id, allGroups, subjectsByGroup)))
            .OrderBy(t => t.Name)
            .Select(t => BuildArchivedTermNode(t, allGroups, subjectsByGroup, projectPath))
            .ToList();

        return new ArchivedCurriculumTreeNodeDto
        {
            Id = project.Id,
            Name = project.Name,
            NodeType = "project",
            Path = projectPath,
            Depth = 1,
            Archived = true,
            Children = termNodes,
        };
    }

    private static ArchivedCurriculumTreeNodeDto BuildArchivedTermNode(
        CurriculumTerm term,
        List<SubjectGroup> allGroups,
        Dictionary<int, List<ArchivedSubjectDto>> subjectsByGroup,
        string projectPath)
    {
        var termPath = $"{projectPath} > {term.Name}";
        var groupNodes = allGroups
            .Where(g => g.TermId == term.Id && (g.Archived || GroupHasPendingArchivedSubjects(g.Id, subjectsByGroup)))
            .OrderBy(g => g.Name)
            .Select(g => BuildArchivedSubjectGroupNode(g, subjectsByGroup, termPath))
            .ToList();

        return new ArchivedCurriculumTreeNodeDto
        {
            Id = term.Id,
            Name = term.Name,
            NodeType = "term",
            Path = termPath,
            Depth = 2,
            Archived = true,
            Children = groupNodes,
        };
    }

    private static ArchivedCurriculumTreeNodeDto BuildArchivedSubjectGroupNode(
        SubjectGroup group,
        Dictionary<int, List<ArchivedSubjectDto>> subjectsByGroup,
        string termPath)
    {
        var groupPath = $"{termPath} > {group.Name}";
        subjectsByGroup.TryGetValue(group.Id, out var subjects);
        var hasPendingSubjects = subjects is { Count: > 0 };

        return new ArchivedCurriculumTreeNodeDto
        {
            Id = group.Id,
            Name = group.Name,
            NodeType = "subjectGroup",
            Path = groupPath,
            Depth = 3,
            Archived = group.Archived || hasPendingSubjects,
            Subjects = subjects ?? [],
            Children = [],
        };
    }

    private async System.Threading.Tasks.Task ArchiveSubjectsInGroupsAsync(IEnumerable<int> subjectGroupIds)
    {
        var ids = subjectGroupIds.Distinct().ToList();
        if (ids.Count == 0) return;

        var subjects = await context.Subjects
            .Where(s => ids.Contains(s.SubjectGroupId) && !s.Archived)
            .Include(s => s.Units)
            .ThenInclude(u => u.Lessons)
            .ThenInclude(l => l.LearningObjectives)
            .ThenInclude(lo => lo.Tasks)
            .ToListAsync();

        foreach (var subject in subjects)
            ArchiveSubjectTree(subject, archivedWithFolder: true);

        if (subjects.Count > 0)
            cache.Remove("AllSubjects");
    }

    private async System.Threading.Tasks.Task UnarchiveSubjectsInGroupsAsync(
        IEnumerable<int> subjectGroupIds,
        IReadOnlyList<int>? subjectIds = null)
    {
        var ids = subjectGroupIds.Distinct().ToList();
        if (ids.Count == 0) return;

        var query = context.Subjects
            .Where(s => ids.Contains(s.SubjectGroupId) && s.Archived && s.ArchivedWithFolder);

        if (subjectIds is not null)
        {
            if (subjectIds.Count == 0)
                return;

            query = query.Where(s => subjectIds.Contains(s.Id));
        }

        var subjects = await query
            .Include(s => s.Units)
            .ThenInclude(u => u.Lessons)
            .ThenInclude(l => l.LearningObjectives)
            .ThenInclude(lo => lo.Tasks)
            .ToListAsync();

        foreach (var subject in subjects)
            UnarchiveSubjectTree(subject);

        if (subjects.Count > 0)
            cache.Remove("AllSubjects");
    }

    private static void ArchiveSubjectTree(Subject subject, bool archivedWithFolder = false)
    {
        foreach (var unit in subject.Units)
        {
            foreach (var lesson in unit.Lessons)
            {
                foreach (var lo in lesson.LearningObjectives)
                {
                    foreach (var task in lo.Tasks)
                        task.Archived = true;
                    lo.Archived = true;
                }
                lesson.Archived = true;
            }
            unit.Archived = true;
        }
        subject.Archived = true;
        subject.ArchivedWithFolder = archivedWithFolder;
    }

    private static void UnarchiveSubjectTree(Subject subject)
    {
        foreach (var unit in subject.Units)
        {
            foreach (var lesson in unit.Lessons)
            {
                foreach (var lo in lesson.LearningObjectives)
                {
                    foreach (var task in lo.Tasks)
                        task.Archived = false;
                    lo.Archived = false;
                }
                lesson.Archived = false;
            }
            unit.Archived = false;
        }
        subject.Archived = false;
        subject.ArchivedWithFolder = false;
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
