using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Application;

public interface IUserRepository
{
    Task<User?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<User?> GetByIdTrackedAsync(int userId, CancellationToken cancellationToken = default);
}
