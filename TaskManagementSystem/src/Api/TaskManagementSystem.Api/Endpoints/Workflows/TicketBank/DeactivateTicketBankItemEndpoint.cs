using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.TicketBank.DeactivateTicketBankItem;

namespace TaskManagementSystem.Api.Endpoints.Workflows.TicketBank;

/// <summary>
/// DELETE /workflows/ticket-bank/{id} — Deactivates a task bank item.
/// </summary>
public static class DeactivateTicketBankItemEndpoint
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
        var result = await mediator.Send(new DeactivateTicketBankItemCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
