namespace TaskManagementSystem.Modules.Ticket.Application;

public interface IIdentityUserLookup
{
    Task<bool> ActiveUserExistsAsync(int userId, CancellationToken cancellationToken = default);
}
