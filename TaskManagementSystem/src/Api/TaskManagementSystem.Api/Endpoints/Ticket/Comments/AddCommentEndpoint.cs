using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Comments.AddComment;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Comments;

/// <summary>POST /tickets/{id}/comments — add a comment to a ticket. Requires Tickets.Create permission.</summary>
public static class AddCommentEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPost("/{id:int}/comments", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Create);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        AddCommentRequest request,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var result = await mediator.Send(new AddCommentCommand(id, userId, request.Content), cancellationToken);
        return result.ToHttpResult(comment => Results.Created($"/tickets/{id}/comments/{comment.Id}", TicketMapping.MapComment(comment)));
    }
}
