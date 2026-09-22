using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Comments.UpdateComment;

public sealed class UpdateCommentCommandHandler(ITicketUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCommentCommand, Result<CommentListItemResult>>
{
    public async Task<Result<CommentListItemResult>> Handle(
        UpdateCommentCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<CommentListItemResult>(TicketErrors.TicketNotFound);
        }

        var comment = await unitOfWork.Comments.GetByIdTrackedAsync(request.CommentId, cancellationToken);
        if (comment is null || comment.TicketId != request.TicketId)
        {
            return Result.Fail<CommentListItemResult>(new ResultError("comment_not_found", "Comment not found."));
        }

        var updateResult = comment.UpdateContent(request.Content, request.UserId);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<CommentListItemResult>(updateResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(CommentListItemResult.From(comment));
    }
}
