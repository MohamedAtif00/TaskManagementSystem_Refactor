namespace TaskManagementSystem.Api.Endpoints.Organization.Sections;

public static class OrganizationSectionEndpoints
{
    public static RouteGroupBuilder MapOrganizationSectionEndpoints(this RouteGroupBuilder organization)
    {
        var sections = organization.MapGroup("/sections");

        ListSectionsEndpoint.Map(sections);
        GetSectionByIdEndpoint.Map(sections);
        CreateSectionEndpoint.Map(sections);
        UpdateSectionEndpoint.Map(sections);
        ArchiveSectionEndpoint.Map(sections);

        return organization;
    }
}
