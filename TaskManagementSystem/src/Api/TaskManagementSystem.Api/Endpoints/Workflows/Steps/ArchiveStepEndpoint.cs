using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Steps.ArchiveStep;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Steps;

/// <summary>
/// DELETE /workflows/steps/{id} — Archives a workflow step.
/// </summary>
public static class ArchiveStepEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder steps)
    {
        steps.MapDelete("/{id:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Delete);

        return steps;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveStepCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
