using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Modules.Identity.Application;

public interface IIdentityUnitOfWork : IUnitOfWork
{
    IUserRepository Users { get; }

    IRefreshTokenRepository RefreshTokens { get; }

    IAboutMeRepository AboutMe { get; }

    IPermissionRepository Permissions { get; }

    IRoleRepository Roles { get; }
}

public interface IAboutMeRepository
{
    Task<AboutMeReadModel?> GetAsync(int userId, CancellationToken cancellationToken = default);
}

public sealed record AboutMeReadModel(
    int Id,
    string Name,
    int RoleId,
    string RoleName,
    IReadOnlyList<string> Permissions,
    string? TeamName,
    int Notifications);
