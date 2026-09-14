namespace TaskManagementSystem.Modules.Ticket.Application;

public interface IOrganizationTeamLookup
{
    Task<bool> ActiveTeamExistsAsync(int teamId, CancellationToken cancellationToken = default);
}
