using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.AssignTicket;

public sealed class AssignTicketCommandHandler(
    ITicketUnitOfWork unitOfWork,
    IIdentityUserLookup identityUserLookup,
    IRealtimePublisher realtimePublisher)
    : IRequestHandler<AssignTicketCommand, Result<TicketDetailResult>>
{
    public async Task<Result<TicketDetailResult>> Handle(
        AssignTicketCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.TicketTasks.GetByIdTrackedAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TicketNotFound);
        }

        if (!await identityUserLookup.ActiveUserExistsAsync(request.UserId, cancellationToken))
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.UserNotFound);
        }

        var assignResult = ticket.Assign(request.UserId);
        if (!assignResult.IsSuccess)
        {
            return Result.Fail<TicketDetailResult>(assignResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);

        return Result.Ok(TicketDetailResult.From(ticket));
    }
}

