using TaskManagementSystem.Api.Endpoints.Organization.Sections;
using TaskManagementSystem.Api.Endpoints.Organization.Teams;

namespace TaskManagementSystem.Api.Endpoints.Organization;

public static class OrganizationEndpoints
{
    public static RouteGroupBuilder MapOrganizationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/organization").WithTags("Organization").RequireAuthorization();

        group.MapOrganizationTeamEndpoints();
        group.MapOrganizationSectionEndpoints();

        return group;
    }
}
