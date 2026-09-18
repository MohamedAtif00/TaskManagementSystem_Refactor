using TaskManagementSystem.Api.Endpoints.Identity.Permissions;
using TaskManagementSystem.Api.Endpoints.Identity.Roles;
using TaskManagementSystem.Api.Endpoints.Identity.Users;

namespace TaskManagementSystem.Api.Endpoints.Identity;

public static class IdentityEndpoints
{
    public static RouteGroupBuilder MapIdentityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/identity").WithTags("Identity").RequireAuthorization();

        ListPermissionsEndpoint.Map(group);
        group.MapIdentityRoleEndpoints();
        group.MapIdentityUserEndpoints();

        return group;
    }
}
