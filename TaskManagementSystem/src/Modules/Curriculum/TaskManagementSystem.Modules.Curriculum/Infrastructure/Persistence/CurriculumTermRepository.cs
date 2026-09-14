using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence;

internal sealed class CurriculumTermRepository(CurriculumDbContext context)
    : GenericRepository<CurriculumTerm, CurriculumDbContext>(context), ICurriculumTermRepository
{
    public Task<CurriculumTerm?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(term => term.Id == id && !term.Archived, cancellationToken);

    public Task<CurriculumTerm?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(term => term.Id == id && !term.Archived, cancellationToken);

    public async Task<IReadOnlyList<CurriculumTerm>> ListActiveByParentIdAsync(
        int projectId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(term => term.ProjectId == projectId && !term.Archived)
            .OrderBy(term => term.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ActiveProjectExistsAsync(int projectId, CancellationToken cancellationToken = default) =>
        Context.CurriculumProjects.AsNoTracking().AnyAsync(project => project.Id == projectId && !project.Archived, cancellationToken);

    public Task<bool> ArchivedProjectExistsAsync(int projectId, CancellationToken cancellationToken = default) =>
        Context.CurriculumProjects.AsNoTracking().AnyAsync(project => project.Id == projectId && project.Archived, cancellationToken);

    public Task AddAsync(CurriculumTerm term, CancellationToken cancellationToken = default) =>
        AddEntityAsync(term, cancellationToken);
}
