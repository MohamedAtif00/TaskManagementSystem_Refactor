using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Comments.UpdateComment;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Comments;

public static class UpdateCommentEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPatch("/{id:int}/comments/{commentId:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Update);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        int commentId,
        AddCommentRequest request,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var result = await mediator.Send(new UpdateCommentCommand(id, commentId, userId, request.Content), cancellationToken);
        return result.ToHttpResult(comment => Results.Ok(TicketMapping.MapComment(comment)));
    }
}
