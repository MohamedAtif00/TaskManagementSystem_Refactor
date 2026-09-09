using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;

internal sealed class RoleRepository(IdentityDbContext context) : IRoleRepository
{
    public async Task<IReadOnlyList<Role>> ListWithPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await context.Roles
            .AsNoTracking()
            .Include(role => role.Permissions)
            .OrderBy(role => role.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Role?> GetByIdWithPermissionsAsync(int roleId, CancellationToken cancellationToken = default) =>
        context.Roles
            .AsNoTracking()
            .Include(role => role.Permissions)
            .FirstOrDefaultAsync(role => role.Id == roleId, cancellationToken);

    public Task<Role?> GetByIdTrackedWithPermissionsAsync(int roleId, CancellationToken cancellationToken = default) =>
        context.Roles
            .Include(role => role.Permissions)
            .FirstOrDefaultAsync(role => role.Id == roleId, cancellationToken);

    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(role => role.Name == name, cancellationToken);

    public async Task<int> GetNextIdAsync(CancellationToken cancellationToken = default)
    {
        var maxId = await context.Roles.MaxAsync(role => (int?)role.Id, cancellationToken);
        return (maxId ?? -1) + 1;
    }

    public Task<bool> HasAssignedUsersAsync(int roleId, CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(user => user.RoleId == roleId, cancellationToken);

    public Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        context.Roles.Add(role);
        return Task.CompletedTask;
    }

    public void Remove(Role role) => context.Roles.Remove(role);
}
