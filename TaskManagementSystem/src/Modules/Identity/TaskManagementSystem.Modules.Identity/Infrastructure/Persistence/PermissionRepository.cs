using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;

internal sealed class PermissionRepository(IdentityDbContext context) : IPermissionRepository
{
    public async Task<IReadOnlyList<Permission>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await context.Permissions
            .AsNoTracking()
            .OrderBy(permission => permission.Code)
            .ToListAsync(cancellationToken);
    }

    public Task<Permission?> GetByIdAsync(int permissionId, CancellationToken cancellationToken = default) =>
        context.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(permission => permission.Id == permissionId, cancellationToken);

    public Task<Permission?> GetByIdTrackedAsync(int permissionId, CancellationToken cancellationToken = default) =>
        context.Permissions.FirstOrDefaultAsync(permission => permission.Id == permissionId, cancellationToken);

    public Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        context.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(permission => permission.Code == code, cancellationToken);

    public async Task<IReadOnlyList<Permission>> GetByIdsAsync(
        IReadOnlyCollection<int> permissionIds,
        CancellationToken cancellationToken = default)
    {
        if (permissionIds.Count == 0)
        {
            return [];
        }

        return await context.Permissions
            .Where(permission => permissionIds.Contains(permission.Id))
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        context.Permissions.Add(permission);
        return Task.CompletedTask;
    }

    public void Remove(Permission permission) => context.Permissions.Remove(permission);
}
