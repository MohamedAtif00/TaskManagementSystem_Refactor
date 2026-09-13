using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Application;

public interface IUserRepository
{
    Task<User?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<User?> GetByIdTrackedAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> ListActiveAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveByEmailAsync(
        string email,
        int? excludeUserId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken = default);

    Task<bool> IsActiveTeamLeaderOrSectionHeadAsync(int userId, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
