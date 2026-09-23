namespace TaskManagementSystem.Modules.Ticket.Application;

public interface IIdentityUserLookup
{
    Task<bool> ActiveUserExistsAsync(int userId, CancellationToken cancellationToken = default);

    Task<ActiveUserRecord?> GetActiveUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, string>> ListNamesAsync(
        IReadOnlyCollection<int> userIds,
        CancellationToken cancellationToken = default);
}
