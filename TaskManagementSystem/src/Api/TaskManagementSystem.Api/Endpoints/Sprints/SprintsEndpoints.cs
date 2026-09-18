using TaskManagementSystem.Api.Endpoints.Sprints.SprintLearningObjectives;
using TaskManagementSystem.Api.Endpoints.Sprints.Sprints;

namespace TaskManagementSystem.Api.Endpoints.Sprints;

public static class SprintsEndpoints
{
    public static RouteGroupBuilder MapSprintsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/sprints").WithTags("Sprints").RequireAuthorization();

        SprintCrudEndpoints.Map(group);
        SprintLearningObjectivesEndpoints.Map(group);

        return group;
    }
}
