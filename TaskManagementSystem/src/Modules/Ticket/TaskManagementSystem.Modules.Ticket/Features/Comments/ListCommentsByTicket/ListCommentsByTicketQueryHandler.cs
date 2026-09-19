using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Comments.ListCommentsByTicket;

public sealed class ListCommentsByTicketQueryHandler(ITicketUnitOfWork unitOfWork)
    : IRequestHandler<ListCommentsByTicketQuery, Result<IReadOnlyList<CommentListItemResult>>>
{
    public async Task<Result<IReadOnlyList<CommentListItemResult>>> Handle(
        ListCommentsByTicketQuery request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.TicketTasks.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<IReadOnlyList<CommentListItemResult>>(TicketErrors.TicketNotFound);
        }

        var comments = await unitOfWork.Comments.ListByTicketAsync(request.TicketId, cancellationToken);
        return Result.Ok<IReadOnlyList<CommentListItemResult>>(comments.Select(CommentListItemResult.From).ToList());
    }
}

