using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;

internal sealed class UserRepository(IdentityDbContext context)
    : GenericRepository<User, IdentityDbContext>(context), IUserRepository
{
    public Task<User?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(Set.Active(), user => user.Code == code, cancellationToken);

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(Set.Active(), user => user.Id == userId, cancellationToken);
}
