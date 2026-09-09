using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;

namespace TaskManagementSystem.TestCommon.Integration;

public static class RbacSeedData
{
    public static async Task SeedAsync(IdentityDbContext identityDb, CancellationToken cancellationToken = default)
    {
        if (await identityDb.Roles.AnyAsync(cancellationToken))
        {
            return;
        }

        var permissions = IdentityPermissionCodes.SystemCodes
            .Select(code => Permission.Create(
                code,
                code switch
                {
                    IdentityPermissionCodes.PermissionsManage => "Manage permissions",
                    IdentityPermissionCodes.RolesManage => "Manage roles",
                    _ => "Assign user roles"
                },
                null,
                isSystem: true).Value!)
            .ToList();

        var roles = new List<Role>
        {
            CreateSystemRole((int)UserRole.ProjectManger, nameof(UserRole.ProjectManger)),
            CreateSystemRole((int)UserRole.SectionHead, nameof(UserRole.SectionHead)),
            CreateSystemRole((int)UserRole.TeamLeader, nameof(UserRole.TeamLeader)),
            CreateSystemRole((int)UserRole.Member, nameof(UserRole.Member)),
            CreateSystemRole((int)UserRole.Owner, nameof(UserRole.Owner))
        };

        var ownerRole = roles.Single(role => role.Id == (int)UserRole.Owner);
        foreach (var permission in permissions)
        {
            ownerRole.Permissions.Add(permission);
        }

        identityDb.Permissions.AddRange(permissions);
        identityDb.Roles.AddRange(roles);
        await identityDb.SaveChangesAsync(cancellationToken);
    }

    private static Role CreateSystemRole(int id, string name)
    {
        var role = Role.Create(name, $"System role: {name}", isSystem: true).Value;
        role.Id = id;
        return role;
    }
}
