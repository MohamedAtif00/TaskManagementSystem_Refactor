using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Application;

public interface ISubjectRepository
{
    Task<Subject?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Subject?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Subject>> ListActiveByParentIdAsync(int subjectGroupId, CancellationToken cancellationToken = default);
    Task<bool> ActiveSubjectGroupExistsAsync(int subjectGroupId, CancellationToken cancellationToken = default);
    Task<bool> ArchivedSubjectGroupExistsAsync(int subjectGroupId, CancellationToken cancellationToken = default);
    Task AddAsync(Subject subject, CancellationToken cancellationToken = default);
}
