namespace TaskManagementSystem.Api.Endpoints.Workflows.Steps;

public static class WorkflowStepEndpoints
{
    public static RouteGroupBuilder MapWorkflowStepEndpoints(this RouteGroupBuilder group)
    {
        var steps = group.MapGroup("/steps");

        UpdateStepEndpoint.Map(steps);
        ArchiveStepEndpoint.Map(steps);

        return group;
    }
}
