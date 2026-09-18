namespace TaskManagementSystem.Api.Endpoints.Curriculum.Projects;

public static class ProjectEndpoints
{
    public static RouteGroupBuilder MapProjectEndpoints(this RouteGroupBuilder group)
    {
        var projects = group.MapGroup("/projects");
        GetProjectByIdEndpoint.Map(projects);
        UpdateProjectEndpoint.Map(projects);
        ArchiveProjectEndpoint.Map(projects);
        ListTermsByProjectEndpoint.Map(projects);
        CreateTermEndpoint.Map(projects);
        return group;
    }
}
