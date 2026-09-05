using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;

internal sealed class RefreshTokenRepository(IdentityDbContext context)
    : GenericRepository<RefreshToken, IdentityDbContext>(context), IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(refreshToken => refreshToken.Token == token, cancellationToken);

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default) =>
        AddEntityAsync(refreshToken, cancellationToken);
}
