using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Comments.DeleteComment;

public sealed class DeleteCommentCommandHandler(ITicketUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCommentCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        DeleteCommentCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<NoValue>(TicketErrors.TicketNotFound);
        }

        var comment = await unitOfWork.Comments.GetByIdTrackedAsync(request.CommentId, cancellationToken);
        if (comment is null || comment.TicketId != request.TicketId)
        {
            return Result.Fail<NoValue>(new ResultError("comment_not_found", "Comment not found."));
        }

        var deleteResult = comment.Archive(request.UserId);
        if (!deleteResult.IsSuccess)
        {
            return Result.Fail<NoValue>(deleteResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
