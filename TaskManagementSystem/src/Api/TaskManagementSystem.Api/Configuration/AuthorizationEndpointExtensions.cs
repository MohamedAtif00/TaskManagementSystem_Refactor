using TaskManagementSystem.Api.Security;

namespace TaskManagementSystem.Api.Configuration;

public static class AuthorizationEndpointExtensions
{
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, string policyName) =>
        builder.RequireAuthorization(policyName);
}
