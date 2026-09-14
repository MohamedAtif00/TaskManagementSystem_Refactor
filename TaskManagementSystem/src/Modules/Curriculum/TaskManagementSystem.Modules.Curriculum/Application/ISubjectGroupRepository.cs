using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Application;

public interface ISubjectGroupRepository
{
    Task<SubjectGroup?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SubjectGroup?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubjectGroup>> ListActiveByParentIdAsync(int termId, CancellationToken cancellationToken = default);
    Task<bool> ActiveTermExistsAsync(int termId, CancellationToken cancellationToken = default);
    Task<bool> ArchivedTermExistsAsync(int termId, CancellationToken cancellationToken = default);
    Task AddAsync(SubjectGroup subjectGroup, CancellationToken cancellationToken = default);
}
