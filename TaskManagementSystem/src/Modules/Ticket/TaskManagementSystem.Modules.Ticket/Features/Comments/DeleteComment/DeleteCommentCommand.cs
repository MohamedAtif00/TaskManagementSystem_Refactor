using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Comments.DeleteComment;

public sealed record DeleteCommentCommand(int TicketId, int CommentId, int UserId)
    : ITicketCommand<Result<NoValue>>;
