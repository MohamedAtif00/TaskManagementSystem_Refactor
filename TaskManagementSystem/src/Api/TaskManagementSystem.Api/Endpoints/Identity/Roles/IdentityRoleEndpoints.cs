namespace TaskManagementSystem.Api.Endpoints.Identity.Roles;

public static class IdentityRoleEndpoints
{
    public static RouteGroupBuilder MapIdentityRoleEndpoints(this RouteGroupBuilder identity)
    {
        var roles = identity.MapGroup("/roles");

        ListRolesEndpoint.Map(roles);
        CreateRoleEndpoint.Map(roles);
        UpdateRoleEndpoint.Map(roles);
        DeleteRoleEndpoint.Map(roles);
        SetRolePermissionsEndpoint.Map(roles);
        AddRolePermissionEndpoint.Map(roles);
        RemoveRolePermissionEndpoint.Map(roles);

        return identity;
    }
}
