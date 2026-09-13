using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Domain;

namespace TaskManagementSystem.Modules.Organization.Infrastructure.Persistence;

internal sealed class TeamRepository(OrganizationDbContext context)
    : GenericRepository<Team, OrganizationDbContext>(context), ITeamRepository
{
    public Task<Team?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(team => team.Id == id && !team.Archived, cancellationToken);

    public Task<Team?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(team => team.Id == id && !team.Archived, cancellationToken);

    public async Task<IReadOnlyList<Team>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(team => !team.Archived)
            .OrderBy(team => team.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Team>> GetActiveByIdsAsync(
        IReadOnlyCollection<int> teamIds,
        CancellationToken cancellationToken = default)
    {
        if (teamIds.Count == 0)
        {
            return [];
        }

        var distinctIds = teamIds.Distinct().ToArray();
        return await Set.AsNoTracking()
            .Where(team => !team.Archived && distinctIds.Contains(team.Id))
            .OrderBy(team => team.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsActiveByIdAsync(int id, CancellationToken cancellationToken = default) =>
        ExistsReadOnlyAsync(team => team.Id == id && !team.Archived, cancellationToken);

    public async Task<bool> AllActiveExistAsync(
        IReadOnlyCollection<int> teamIds,
        CancellationToken cancellationToken = default)
    {
        if (teamIds.Count == 0)
        {
            return true;
        }

        var distinctIds = teamIds.Distinct().ToArray();
        var count = await Set.AsNoTracking()
            .CountAsync(team => !team.Archived && distinctIds.Contains(team.Id), cancellationToken);

        return count == distinctIds.Length;
    }

    public Task AddAsync(Team team, CancellationToken cancellationToken = default) =>
        AddEntityAsync(team, cancellationToken);
}
