namespace TaskManagementSystem.Modules.Workflows.Application;

public interface IOrganizationTeamLookup
{
    Task<bool> ActiveTeamExistsAsync(int teamId, CancellationToken cancellationToken = default);
}
