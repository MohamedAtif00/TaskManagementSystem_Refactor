namespace TaskManagementSystem.Api.Endpoints.Sprints.Sprints;

public static class SprintCrudEndpoints
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        ListSprintsEndpoint.Map(group);
        CreateSprintEndpoint.Map(group);
        GetSprintByIdEndpoint.Map(group);
        UpdateSprintEndpoint.Map(group);
        ArchiveSprintEndpoint.Map(group);
        return group;
    }
}
