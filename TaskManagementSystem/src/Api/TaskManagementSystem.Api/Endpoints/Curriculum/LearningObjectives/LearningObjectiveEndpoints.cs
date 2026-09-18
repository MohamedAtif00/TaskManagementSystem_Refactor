namespace TaskManagementSystem.Api.Endpoints.Curriculum.LearningObjectives;

public static class LearningObjectiveEndpoints
{
    public static RouteGroupBuilder MapLearningObjectiveEndpoints(this RouteGroupBuilder group)
    {
        var learningObjectives = group.MapGroup("/learning-objectives");
        GetLearningObjectiveByIdEndpoint.Map(learningObjectives);
        UpdateLearningObjectiveEndpoint.Map(learningObjectives);
        ArchiveLearningObjectiveEndpoint.Map(learningObjectives);
        return group;
    }
}
