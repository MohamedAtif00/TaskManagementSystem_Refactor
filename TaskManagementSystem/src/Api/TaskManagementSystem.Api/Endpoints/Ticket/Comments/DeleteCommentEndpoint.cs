using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Comments.DeleteComment;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Comments;

public static class DeleteCommentEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:int}/comments/{commentId:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Update);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        int commentId,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var result = await mediator.Send(new DeleteCommentCommand(id, commentId, userId), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
