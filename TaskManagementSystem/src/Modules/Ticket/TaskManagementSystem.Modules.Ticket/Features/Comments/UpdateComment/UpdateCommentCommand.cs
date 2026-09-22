using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Comments.UpdateComment;

public sealed record UpdateCommentCommand(int TicketId, int CommentId, int UserId, string Content)
    : ITicketCommand<Result<CommentListItemResult>>;
