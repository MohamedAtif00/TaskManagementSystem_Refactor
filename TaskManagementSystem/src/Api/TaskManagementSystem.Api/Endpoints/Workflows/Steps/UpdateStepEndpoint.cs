using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Steps.UpdateStep;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Steps;

/// <summary>
/// PUT /workflows/steps/{id} — Updates a workflow step.
/// </summary>
public static class UpdateStepEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder steps)
    {
        steps.MapPut("/{id:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Update);

        return steps;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateStepRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateStepCommand(id, request.TaskBankId, request.Duration, request.Priority),
            cancellationToken);

        return result.ToHttpResult(step => Results.Ok(WorkflowMapping.MapStepListItem(step)));
    }
}
