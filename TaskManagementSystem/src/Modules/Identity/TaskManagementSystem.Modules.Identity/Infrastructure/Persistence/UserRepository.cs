using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;

internal sealed class UserRepository(IdentityDbContext context) : IUserRepository
{
    private IQueryable<User> AuthQuery() =>
        context.Users
            .AsNoTracking()
            .Include(user => user.Role!)
            .ThenInclude(role => role.Permissions);

    public Task<User?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        AuthQuery()
            .Active()
            .FirstOrDefaultAsync(user => user.Code == code, cancellationToken);

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default) =>
        AuthQuery()
            .Active()
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public Task<User?> GetByIdTrackedAsync(int userId, CancellationToken cancellationToken = default) =>
        context.Users
            .Include(user => user.Role!)
            .ThenInclude(role => role.Permissions)
            .Active()
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
}
