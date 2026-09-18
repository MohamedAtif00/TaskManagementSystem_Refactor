using TaskManagementSystem.Api.Endpoints.Workflows.SchemaTypes;
using TaskManagementSystem.Api.Endpoints.Workflows.Schemas;
using TaskManagementSystem.Api.Endpoints.Workflows.Nodes;
using TaskManagementSystem.Api.Endpoints.Workflows.Steps;
using TaskManagementSystem.Api.Endpoints.Workflows.TaskBank;

namespace TaskManagementSystem.Api.Endpoints.Workflows;

public static class WorkflowsEndpoints
{
    public static RouteGroupBuilder MapWorkflowsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/workflows").WithTags("Workflows").RequireAuthorization();

        ListSchemaTypesEndpoint.Map(group);
        group.MapWorkflowSchemaEndpoints();
        group.MapWorkflowNodeEndpoints();
        group.MapWorkflowStepEndpoints();
        group.MapWorkflowTaskBankEndpoints();

        return group;
    }
}
