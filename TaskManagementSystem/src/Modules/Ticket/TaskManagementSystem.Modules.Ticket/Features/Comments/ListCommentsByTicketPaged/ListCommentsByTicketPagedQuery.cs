using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;

namespace TaskManagementSystem.Modules.Ticket.Features.Comments.ListCommentsByTicketPaged;

public sealed record ListCommentsByTicketPagedQuery(
    int TicketId,
    int? Page = null,
    int? PageSize = null) : IQuery<Result<CommentListPageResult>>;
