using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence;

internal sealed class SubjectGroupRepository(CurriculumDbContext context)
    : GenericRepository<SubjectGroup, CurriculumDbContext>(context), ISubjectGroupRepository
{
    public Task<SubjectGroup?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(group => group.Id == id && !group.Archived, cancellationToken);

    public Task<SubjectGroup?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(group => group.Id == id && !group.Archived, cancellationToken);

    public async Task<IReadOnlyList<SubjectGroup>> ListActiveByParentIdAsync(
        int termId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(group => group.TermId == termId && !group.Archived)
            .OrderBy(group => group.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ActiveTermExistsAsync(int termId, CancellationToken cancellationToken = default) =>
        Context.CurriculumTerms.AsNoTracking().AnyAsync(term => term.Id == termId && !term.Archived, cancellationToken);

    public Task<bool> ArchivedTermExistsAsync(int termId, CancellationToken cancellationToken = default) =>
        Context.CurriculumTerms.AsNoTracking().AnyAsync(term => term.Id == termId && term.Archived, cancellationToken);

    public Task AddAsync(SubjectGroup subjectGroup, CancellationToken cancellationToken = default) =>
        AddEntityAsync(subjectGroup, cancellationToken);
}
