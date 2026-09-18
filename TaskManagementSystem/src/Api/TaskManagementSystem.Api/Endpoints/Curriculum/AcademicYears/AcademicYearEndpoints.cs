namespace TaskManagementSystem.Api.Endpoints.Curriculum.AcademicYears;

public static class AcademicYearEndpoints
{
    public static RouteGroupBuilder MapAcademicYearEndpoints(this RouteGroupBuilder group)
    {
        var years = group.MapGroup("/years");
        ListAcademicYearsEndpoint.Map(years);
        GetAcademicYearByIdEndpoint.Map(years);
        CreateAcademicYearEndpoint.Map(years);
        UpdateAcademicYearEndpoint.Map(years);
        ArchiveAcademicYearEndpoint.Map(years);
        GetYearTreeEndpoint.Map(years);
        ListProjectsByYearEndpoint.Map(years);
        CreateProjectEndpoint.Map(years);
        return group;
    }
}
