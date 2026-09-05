using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Application;

public interface IIdentityUnitOfWork : IUnitOfWork
{
    IUserRepository Users { get; }

    IRefreshTokenRepository RefreshTokens { get; }

    IAboutMeRepository AboutMe { get; }
}

public interface IAboutMeRepository
{
    Task<AboutMeReadModel?> GetAsync(int userId, CancellationToken cancellationToken = default);
}

public sealed record AboutMeReadModel(
    int Id,
    string Name,
    UserRole Role,
    string? TeamName,
    int Notifications);
