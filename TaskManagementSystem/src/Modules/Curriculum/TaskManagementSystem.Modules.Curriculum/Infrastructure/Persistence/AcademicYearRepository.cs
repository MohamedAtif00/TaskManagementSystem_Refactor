using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence;

internal sealed class AcademicYearRepository(CurriculumDbContext context)
    : GenericRepository<AcademicYear, CurriculumDbContext>(context), IAcademicYearRepository
{
    public Task<AcademicYear?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(year => year.Id == id && !year.Archived, cancellationToken);

    public Task<AcademicYear?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(year => year.Id == id && !year.Archived, cancellationToken);

    public async Task<IReadOnlyList<AcademicYear>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(year => !year.Archived)
            .OrderBy(year => year.Name)
            .ToListAsync(cancellationToken);

    public Task AddAsync(AcademicYear academicYear, CancellationToken cancellationToken = default) =>
        AddEntityAsync(academicYear, cancellationToken);
}
