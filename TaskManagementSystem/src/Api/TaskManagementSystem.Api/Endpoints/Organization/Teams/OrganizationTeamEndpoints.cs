namespace TaskManagementSystem.Api.Endpoints.Organization.Teams;

public static class OrganizationTeamEndpoints
{
    public static RouteGroupBuilder MapOrganizationTeamEndpoints(this RouteGroupBuilder organization)
    {
        var teams = organization.MapGroup("/teams");

        ListTeamsEndpoint.Map(teams);
        GetTeamByIdEndpoint.Map(teams);
        CreateTeamEndpoint.Map(teams);
        UpdateTeamEndpoint.Map(teams);
        ArchiveTeamEndpoint.Map(teams);

        return organization;
    }
}
