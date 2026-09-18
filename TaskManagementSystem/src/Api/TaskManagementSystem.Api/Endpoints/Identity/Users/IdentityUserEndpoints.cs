namespace TaskManagementSystem.Api.Endpoints.Identity.Users;

public static class IdentityUserEndpoints
{
    public static RouteGroupBuilder MapIdentityUserEndpoints(this RouteGroupBuilder identity)
    {
        var users = identity.MapGroup("/users");

        ListUsersEndpoint.Map(users);
        GetTeamLeadersEndpoint.Map(users);
        GetUserByIdEndpoint.Map(users);
        CreateUserEndpoint.Map(users);
        UpdateUserEndpoint.Map(users);
        ArchiveUserEndpoint.Map(users);
        AssignUserRoleEndpoint.Map(users);

        return identity;
    }
}
