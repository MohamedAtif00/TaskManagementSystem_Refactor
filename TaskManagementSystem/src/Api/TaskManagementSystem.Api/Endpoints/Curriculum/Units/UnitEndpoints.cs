namespace TaskManagementSystem.Api.Endpoints.Curriculum.Units;

public static class UnitEndpoints
{
    public static RouteGroupBuilder MapUnitEndpoints(this RouteGroupBuilder group)
    {
        var units = group.MapGroup("/units");
        GetUnitByIdEndpoint.Map(units);
        UpdateUnitEndpoint.Map(units);
        ArchiveUnitEndpoint.Map(units);
        ListLessonsByUnitEndpoint.Map(units);
        CreateLessonEndpoint.Map(units);
        return group;
    }
}
