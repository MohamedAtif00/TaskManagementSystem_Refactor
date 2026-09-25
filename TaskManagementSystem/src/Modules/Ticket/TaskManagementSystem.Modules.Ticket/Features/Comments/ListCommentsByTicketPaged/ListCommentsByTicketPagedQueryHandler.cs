using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Features;

namespace TaskManagementSystem.Modules.Ticket.Features.Comments.ListCommentsByTicketPaged;

public sealed class ListCommentsByTicketPagedQueryHandler(ITicketUnitOfWork unitOfWork)
    : IRequestHandler<ListCommentsByTicketPagedQuery, Result<CommentListPageResult>>
{
    public async Task<Result<CommentListPageResult>> Handle(
        ListCommentsByTicketPagedQuery request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<CommentListPageResult>(TicketErrors.TicketNotFound);
        }

        var paging = PagingValidation.Resolve(request.Page, request.PageSize);
        if (paging.IsFailure)
        {
            return Result.Fail<CommentListPageResult>(paging.Error);
        }

        var (page, pageSize, _) = paging.Value;
        var (items, totalCount) = await unitOfWork.Comments.ListByTicketPagedAsync(
            request.TicketId,
            page,
            pageSize,
            cancellationToken);

        return Result.Ok(new CommentListPageResult(
            items.Select(CommentListItemResult.From).ToList(),
            page,
            pageSize,
            totalCount));
    }
}
