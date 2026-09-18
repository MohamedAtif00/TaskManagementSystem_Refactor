using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.TaskBank.DeactivateTaskBankItem;

namespace TaskManagementSystem.Api.Endpoints.Workflows.TaskBank;

/// <summary>
/// DELETE /workflows/task-bank/{id} — Deactivates a task bank item.
/// </summary>
public static class DeactivateTaskBankItemEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder taskBank)
    {
        taskBank.MapDelete("/{id:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Delete);

        return taskBank;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeactivateTaskBankItemCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
