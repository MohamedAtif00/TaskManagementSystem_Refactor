using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence;

internal sealed class LessonRepository(CurriculumDbContext context)
    : GenericRepository<Lesson, CurriculumDbContext>(context), ILessonRepository
{
    public Task<Lesson?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(lesson => lesson.Id == id && !lesson.Archived, cancellationToken);

    public Task<Lesson?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(lesson => lesson.Id == id && !lesson.Archived, cancellationToken);

    public async Task<IReadOnlyList<Lesson>> ListActiveByParentIdAsync(
        int unitId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(lesson => lesson.UnitId == unitId && !lesson.Archived)
            .OrderBy(lesson => lesson.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ActiveUnitExistsAsync(int unitId, CancellationToken cancellationToken = default) =>
        Context.Units.AsNoTracking().AnyAsync(unit => unit.Id == unitId && !unit.Archived, cancellationToken);

    public Task<bool> ArchivedUnitExistsAsync(int unitId, CancellationToken cancellationToken = default) =>
        Context.Units.AsNoTracking().AnyAsync(unit => unit.Id == unitId && unit.Archived, cancellationToken);

    public Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default) =>
        AddEntityAsync(lesson, cancellationToken);
}
