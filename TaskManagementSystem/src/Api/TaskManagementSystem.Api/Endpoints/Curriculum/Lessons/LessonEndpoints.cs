namespace TaskManagementSystem.Api.Endpoints.Curriculum.Lessons;

public static class LessonEndpoints
{
    public static RouteGroupBuilder MapLessonEndpoints(this RouteGroupBuilder group)
    {
        var lessons = group.MapGroup("/lessons");
        GetLessonByIdEndpoint.Map(lessons);
        UpdateLessonEndpoint.Map(lessons);
        ArchiveLessonEndpoint.Map(lessons);
        ListLearningObjectivesByLessonEndpoint.Map(lessons);
        CreateLearningObjectiveEndpoint.Map(lessons);
        return group;
    }
}
