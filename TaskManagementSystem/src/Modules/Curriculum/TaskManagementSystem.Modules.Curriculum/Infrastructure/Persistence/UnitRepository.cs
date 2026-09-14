using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence;

internal sealed class UnitRepository(CurriculumDbContext context)
    : GenericRepository<Unit, CurriculumDbContext>(context), IUnitRepository
{
    public Task<Unit?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(unit => unit.Id == id && !unit.Archived, cancellationToken);

    public Task<Unit?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(unit => unit.Id == id && !unit.Archived, cancellationToken);

    public async Task<IReadOnlyList<Unit>> ListActiveByParentIdAsync(
        int subjectId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(unit => unit.SubjectId == subjectId && !unit.Archived)
            .OrderBy(unit => unit.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ActiveSubjectExistsAsync(int subjectId, CancellationToken cancellationToken = default) =>
        Context.Subjects.AsNoTracking().AnyAsync(subject => subject.Id == subjectId && !subject.Archived, cancellationToken);

    public Task<bool> ArchivedSubjectExistsAsync(int subjectId, CancellationToken cancellationToken = default) =>
        Context.Subjects.AsNoTracking().AnyAsync(subject => subject.Id == subjectId && subject.Archived, cancellationToken);

    public Task AddAsync(Unit unit, CancellationToken cancellationToken = default) =>
        AddEntityAsync(unit, cancellationToken);
}
