using TaskManagementSystem.Modules.Organization.Domain;

namespace TaskManagementSystem.Modules.Organization.Application;

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Team?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Team>> ListActiveAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> AllActiveExistAsync(IReadOnlyCollection<int> teamIds, CancellationToken cancellationToken = default);

    Task AddAsync(Team team, CancellationToken cancellationToken = default);
}
