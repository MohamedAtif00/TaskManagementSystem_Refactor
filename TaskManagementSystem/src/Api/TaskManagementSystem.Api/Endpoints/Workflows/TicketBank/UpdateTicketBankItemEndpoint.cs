using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.TicketBank.UpdateTicketBankItem;

namespace TaskManagementSystem.Api.Endpoints.Workflows.TicketBank;

/// <summary>
/// PUT /workflows/ticket-bank/{id} — Updates a task bank item.
/// </summary>
public static class UpdateTicketBankItemEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder taskBank)
    {
        taskBank.MapPut("/{id:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Update);

        return taskBank;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateTicketBankItemRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateTicketBankItemCommand(
                id,
                request.Name,
                request.Duration,
                request.Type,
                request.TeamLeaderOnly,
                request.TeamId),
            cancellationToken);

        return result.ToHttpResult(item => Results.Ok(WorkflowMapping.MapTicketBankListItem(item)));
    }
}
