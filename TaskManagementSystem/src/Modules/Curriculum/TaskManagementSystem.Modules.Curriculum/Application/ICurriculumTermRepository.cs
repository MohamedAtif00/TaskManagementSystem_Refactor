using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Application;

public interface ICurriculumTermRepository
{
    Task<CurriculumTerm?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CurriculumTerm?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CurriculumTerm>> ListActiveByParentIdAsync(int projectId, CancellationToken cancellationToken = default);
    Task<bool> ActiveProjectExistsAsync(int projectId, CancellationToken cancellationToken = default);
    Task<bool> ArchivedProjectExistsAsync(int projectId, CancellationToken cancellationToken = default);
    Task AddAsync(CurriculumTerm term, CancellationToken cancellationToken = default);
}
