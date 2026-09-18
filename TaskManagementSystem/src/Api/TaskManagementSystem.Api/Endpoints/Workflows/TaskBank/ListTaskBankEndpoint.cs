using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.TaskBank.ListTaskBank;

namespace TaskManagementSystem.Api.Endpoints.Workflows.TaskBank;

/// <summary>
/// GET /workflows/task-bank — Lists task bank items.
/// </summary>
public static class ListTaskBankEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder taskBank)
    {
        taskBank.MapGet("", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Read);

        return taskBank;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListTaskBankQuery(), cancellationToken);
        return result.ToHttpResult(items =>
            Results.Ok(items.Select(WorkflowMapping.MapTaskBankListItem).ToList()));
    }
}
