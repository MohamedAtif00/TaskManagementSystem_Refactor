namespace TaskManagementSystem.Api.Endpoints.Curriculum.Subjects;

public static class SubjectEndpoints
{
    public static RouteGroupBuilder MapSubjectEndpoints(this RouteGroupBuilder group)
    {
        var subjects = group.MapGroup("/subjects");
        ListSubjectsPagedEndpoint.Map(subjects);
        GetSubjectByIdEndpoint.Map(subjects);
        UpdateSubjectEndpoint.Map(subjects);
        ArchiveSubjectEndpoint.Map(subjects);
        UpdateSubjectStatusEndpoint.Map(subjects);
        ListSubjectUsersEndpoint.Map(subjects);
        AssignSubjectUsersEndpoint.Map(subjects);
        UnassignSubjectUsersEndpoint.Map(subjects);
        ListUnitsBySubjectEndpoint.Map(subjects);
        CreateUnitEndpoint.Map(subjects);
        return group;
    }
}
