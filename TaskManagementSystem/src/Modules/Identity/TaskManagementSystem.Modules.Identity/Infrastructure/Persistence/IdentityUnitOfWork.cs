using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;

internal sealed class IdentityUnitOfWork(IdentityDbContext context)
    : UnitOfWork<IdentityDbContext>(context), IIdentityUnitOfWork
{
    private readonly Lazy<UserRepository> _users =
        LazyRepositoryFactory.Create(() => new UserRepository(context));

    private readonly Lazy<RefreshTokenRepository> _refreshTokens =
        LazyRepositoryFactory.Create(() => new RefreshTokenRepository(context));

    private readonly Lazy<AboutMeRepository> _aboutMe =
        LazyRepositoryFactory.Create(() => new AboutMeRepository(context));

    public IUserRepository Users => _users.Value;

    public IRefreshTokenRepository RefreshTokens => _refreshTokens.Value;

    public IAboutMeRepository AboutMe => _aboutMe.Value;
}
