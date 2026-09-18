using TaskManagementSystem.Modules.Sprints.Domain;

namespace TaskManagementSystem.Modules.Sprints.Application;

public interface ISprintRepository
{
    Task<Sprint?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Sprint?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sprint>> ListAsync(bool? archived, CancellationToken cancellationToken = default);
    Task AddAsync(Sprint sprint, CancellationToken cancellationToken = default);
}
