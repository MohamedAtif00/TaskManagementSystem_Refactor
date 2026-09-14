using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence;

internal sealed class CurriculumProjectRepository(CurriculumDbContext context)
    : GenericRepository<CurriculumProject, CurriculumDbContext>(context), ICurriculumProjectRepository
{
    public Task<CurriculumProject?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(project => project.Id == id && !project.Archived, cancellationToken);

    public Task<CurriculumProject?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(project => project.Id == id && !project.Archived, cancellationToken);

    public async Task<IReadOnlyList<CurriculumProject>> ListActiveByParentIdAsync(
        int yearId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(project => project.YearId == yearId && !project.Archived)
            .OrderBy(project => project.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ActiveYearExistsAsync(int yearId, CancellationToken cancellationToken = default) =>
        Context.AcademicYears.AsNoTracking().AnyAsync(year => year.Id == yearId && !year.Archived, cancellationToken);

    public Task<bool> ArchivedYearExistsAsync(int yearId, CancellationToken cancellationToken = default) =>
        Context.AcademicYears.AsNoTracking().AnyAsync(year => year.Id == yearId && year.Archived, cancellationToken);

    public Task AddAsync(CurriculumProject project, CancellationToken cancellationToken = default) =>
        AddEntityAsync(project, cancellationToken);
}
