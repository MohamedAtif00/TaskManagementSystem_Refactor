namespace TaskManagementSystem.Api.Endpoints.Curriculum.SubjectGroups;

public static class SubjectGroupEndpoints
{
    public static RouteGroupBuilder MapSubjectGroupEndpoints(this RouteGroupBuilder group)
    {
        var subjectGroups = group.MapGroup("/subject-groups");
        GetSubjectGroupByIdEndpoint.Map(subjectGroups);
        UpdateSubjectGroupEndpoint.Map(subjectGroups);
        ArchiveSubjectGroupEndpoint.Map(subjectGroups);
        ListSubjectsBySubjectGroupEndpoint.Map(subjectGroups);
        CreateSubjectEndpoint.Map(subjectGroups);
        return group;
    }
}
