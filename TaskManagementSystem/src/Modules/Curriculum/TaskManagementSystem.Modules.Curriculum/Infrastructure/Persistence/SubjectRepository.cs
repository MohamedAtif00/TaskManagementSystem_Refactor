using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence;

internal sealed class SubjectRepository(CurriculumDbContext context)
    : GenericRepository<Subject, CurriculumDbContext>(context), ISubjectRepository
{
    public Task<Subject?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(subject => subject.Id == id && !subject.Archived, cancellationToken);

    public Task<Subject?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(subject => subject.Id == id && !subject.Archived, cancellationToken);

    public async Task<IReadOnlyList<Subject>> ListActiveByParentIdAsync(
        int subjectGroupId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(subject => subject.SubjectGroupId == subjectGroupId && !subject.Archived)
            .OrderBy(subject => subject.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ActiveSubjectGroupExistsAsync(int subjectGroupId, CancellationToken cancellationToken = default) =>
        Context.SubjectGroups.AsNoTracking().AnyAsync(group => group.Id == subjectGroupId && !group.Archived, cancellationToken);

    public Task<bool> ArchivedSubjectGroupExistsAsync(int subjectGroupId, CancellationToken cancellationToken = default) =>
        Context.SubjectGroups.AsNoTracking().AnyAsync(group => group.Id == subjectGroupId && group.Archived, cancellationToken);

    public Task AddAsync(Subject subject, CancellationToken cancellationToken = default) =>
        AddEntityAsync(subject, cancellationToken);
}
