namespace TaskManagementSystem.Api.Endpoints.Curriculum.Terms;

public static class TermEndpoints
{
    public static RouteGroupBuilder MapTermEndpoints(this RouteGroupBuilder group)
    {
        var terms = group.MapGroup("/terms");
        GetTermByIdEndpoint.Map(terms);
        UpdateTermEndpoint.Map(terms);
        ArchiveTermEndpoint.Map(terms);
        ListSubjectGroupsByTermEndpoint.Map(terms);
        CreateSubjectGroupEndpoint.Map(terms);
        return group;
    }
}
