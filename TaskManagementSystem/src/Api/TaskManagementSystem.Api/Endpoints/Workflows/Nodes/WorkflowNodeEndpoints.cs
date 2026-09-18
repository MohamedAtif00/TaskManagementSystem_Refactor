namespace TaskManagementSystem.Api.Endpoints.Workflows.Nodes;

public static class WorkflowNodeEndpoints
{
    public static RouteGroupBuilder MapWorkflowNodeEndpoints(this RouteGroupBuilder group)
    {
        var nodes = group.MapGroup("/nodes");

        UpdateNodeEndpoint.Map(nodes);
        ArchiveNodeEndpoint.Map(nodes);
        ListStepsByNodeEndpoint.Map(nodes);
        CreateStepEndpoint.Map(nodes);

        return group;
    }
}
