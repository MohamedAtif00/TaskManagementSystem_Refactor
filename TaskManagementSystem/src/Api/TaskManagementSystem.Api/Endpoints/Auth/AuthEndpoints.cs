namespace TaskManagementSystem.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        LoginEndpoint.Map(group);
        RefreshTokenEndpoint.Map(group);
        LogoutEndpoint.Map(group);
        AboutMeEndpoint.Map(group);

        return group;
    }
}
