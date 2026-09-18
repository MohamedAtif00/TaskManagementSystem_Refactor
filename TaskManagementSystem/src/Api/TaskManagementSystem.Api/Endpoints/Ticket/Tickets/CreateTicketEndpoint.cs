using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.CreateTicket;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

/// <summary>POST /tickets — create a ticket. Requires Tickets.Create permission.</summary>
public static class CreateTicketEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPost("", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Create);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        CreateTicketRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateTicketCommand(request.LearningObjectiveId, request.TaskBankItemId, request.UserId),
            cancellationToken);

        return result.ToHttpResult(ticket =>
            Results.Created($"/tickets/{ticket.Id}", TicketMapping.MapTicketDetail(ticket)));
    }
}
