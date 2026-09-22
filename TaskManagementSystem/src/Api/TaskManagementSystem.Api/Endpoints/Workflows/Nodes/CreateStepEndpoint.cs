using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Steps.CreateStep;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Nodes;

/// <summary>
/// POST /workflows/nodes/{nodeId}/steps — Creates a step on a workflow node.
/// </summary>
public static class CreateStepEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder nodes)
    {
        nodes.MapPost("/{nodeId:int}/steps", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Create);

        return nodes;
    }

    private static async Task<IResult> HandleAsync(
        int nodeId,
        CreateStepRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateStepCommand(nodeId, request.TicketBankId, request.Duration, request.Priority),
            cancellationToken);

        return result.ToHttpResult(step =>
            Results.Created($"/workflows/steps/{step.Id}", WorkflowMapping.MapStepListItem(step)));
    }
}
