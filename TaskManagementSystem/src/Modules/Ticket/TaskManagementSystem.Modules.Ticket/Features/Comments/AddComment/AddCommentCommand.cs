using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Comments.AddComment;

public sealed record AddCommentCommand(int TicketId, int UserId, string Content)
    : ITicketCommand<Result<CommentListItemResult>>;

public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Content).NotEmpty();
    }
}

public sealed class AddCommentCommandHandler(
    ITicketUnitOfWork unitOfWork,
    IIdentityUserLookup identityUserLookup,
    IRealtimePublisher realtimePublisher)
    : IRequestHandler<AddCommentCommand, Result<CommentListItemResult>>
{
    public async Task<Result<CommentListItemResult>> Handle(
        AddCommentCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.TicketTasks.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<CommentListItemResult>(TicketErrors.TicketNotFound);
        }

        if (!await identityUserLookup.ActiveUserExistsAsync(request.UserId, cancellationToken))
        {
            return Result.Fail<CommentListItemResult>(TicketErrors.UserNotFound);
        }

        var timestamp = DateTime.UtcNow;
        var createResult = Comment.Create(
            request.Content,
            ticket.LearningObjectiveId,
            request.TicketId,
            request.UserId,
            timestamp);

        if (!createResult.IsSuccess)
        {
            return Result.Fail<CommentListItemResult>(createResult.Error);
        }

        await unitOfWork.Comments.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var trackedTicket = await unitOfWork.TicketTasks.GetByIdAsync(request.TicketId, cancellationToken);
        if (trackedTicket is not null)
        {
            await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, trackedTicket, cancellationToken);
        }

        return Result.Ok(CommentListItemResult.From(createResult.Value));
    }
}
