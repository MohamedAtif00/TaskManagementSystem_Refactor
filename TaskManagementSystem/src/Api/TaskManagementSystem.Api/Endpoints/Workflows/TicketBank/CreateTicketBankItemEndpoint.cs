using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.TicketBank.CreateTicketBankItem;

namespace TaskManagementSystem.Api.Endpoints.Workflows.TicketBank;

/// <summary>
/// POST /workflows/ticket-bank — Creates a task bank item.
/// </summary>
public static class CreateTicketBankItemEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder taskBank)
    {
        taskBank.MapPost("", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Create);

        return taskBank;
    }

    private static async Task<IResult> HandleAsync(
        CreateTicketBankItemRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateTicketBankItemCommand(
                request.Name,
                request.Duration,
                request.Type,
                request.TeamLeaderOnly,
                request.TeamId),
            cancellationToken);

        return result.ToHttpResult(item =>
            Results.Created($"/workflows/ticket-bank/{item.Id}", WorkflowMapping.MapTicketBankListItem(item)));
    }
}
