namespace TaskManagementSystem.Api.Endpoints.Sprints.SprintLearningObjectives;

public static class SprintLearningObjectivesEndpoints
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        ListSprintLearningObjectivesEndpoint.Map(group);
        AddSprintLearningObjectivesEndpoint.Map(group);
        RemoveSprintLearningObjectiveEndpoint.Map(group);
        return group;
    }
}
