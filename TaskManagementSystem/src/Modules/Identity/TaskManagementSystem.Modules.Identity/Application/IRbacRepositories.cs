using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Application;

public interface IPermissionRepository
{
    Task<IReadOnlyList<Permission>> ListAsync(CancellationToken cancellationToken = default);

    Task<Permission?> GetByIdAsync(int permissionId, CancellationToken cancellationToken = default);

    Task<Permission?> GetByIdTrackedAsync(int permissionId, CancellationToken cancellationToken = default);

    Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Permission>> GetByIdsAsync(IReadOnlyCollection<int> permissionIds, CancellationToken cancellationToken = default);

    Task AddAsync(Permission permission, CancellationToken cancellationToken = default);

    void Remove(Permission permission);
}

public interface IRoleRepository
{
    Task<IReadOnlyList<Role>> ListWithPermissionsAsync(CancellationToken cancellationToken = default);

    Task<Role?> GetByIdWithPermissionsAsync(int roleId, CancellationToken cancellationToken = default);

    Task<Role?> GetByIdTrackedWithPermissionsAsync(int roleId, CancellationToken cancellationToken = default);

    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<int> GetNextIdAsync(CancellationToken cancellationToken = default);

    Task<bool> HasAssignedUsersAsync(int roleId, CancellationToken cancellationToken = default);

    Task AddAsync(Role role, CancellationToken cancellationToken = default);

    void Remove(Role role);
}
