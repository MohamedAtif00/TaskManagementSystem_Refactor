using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Comments.AddComment;

public sealed record AddCommentCommand(int TicketId, int UserId, string Content)
    : ITicketCommand<Result<CommentListItemResult>>;

