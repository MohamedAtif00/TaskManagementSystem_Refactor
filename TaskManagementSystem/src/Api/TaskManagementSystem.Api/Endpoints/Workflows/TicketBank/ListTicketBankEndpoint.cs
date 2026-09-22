using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.TicketBank.ListTicketBank;

namespace TaskManagementSystem.Api.Endpoints.Workflows.TicketBank;

/// <summary>
/// GET /workflows/ticket-bank — Lists task bank items.
/// </summary>
public static class ListTicketBankEndpoint
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
        var result = await mediator.Send(new ListTicketBankQuery(), cancellationToken);
        return result.ToHttpResult(items =>
            Results.Ok(items.Select(WorkflowMapping.MapTicketBankListItem).ToList()));
    }
}
