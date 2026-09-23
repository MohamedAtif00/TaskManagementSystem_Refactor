using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.AssignTicket;

public sealed class AssignTicketCommandHandler(
    ITicketUnitOfWork unitOfWork,
    IIdentityUserLookup identityUserLookup,
    ITicketActivityWriter activityWriter,
    IRealtimePublisher realtimePublisher)
    : IRequestHandler<AssignTicketCommand, Result<TicketDetailResult>>
{
    public async Task<Result<TicketDetailResult>> Handle(
        AssignTicketCommand request,
        CancellationToken cancellationToken)
    {
        if (TicketRoles.IsMember(request.ActorRole))
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TicketUnauthorized);
        }

        var ticket = await unitOfWork.Tickets.GetByIdTrackedAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TicketNotFound);
        }

        var assignee = await identityUserLookup.GetActiveUserAsync(request.UserId, cancellationToken);
        if (assignee is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.UserNotFound);
        }

        if (!TicketRoles.IsOwnerOrProjectManager(request.ActorRole) && assignee.TeamId != ticket.TeamId)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.AssigneeTeamMismatch);
        }

        var wasDoing = ticket.Status == TicketStatus.Doing;
        var assignResult = ticket.Assign(request.UserId);
        if (!assignResult.IsSuccess)
        {
            return Result.Fail<TicketDetailResult>(assignResult.Error);
        }

        if (wasDoing)
        {
            await TicketWorkClock.CloseOpenAsync(unitOfWork, ticket.Id, cancellationToken);
        }

        await activityWriter.WriteAsync(
            ticket.Id,
            TicketActivityType.Assign,
            request.ActorUserId,
            cancellationToken,
            actorTwoId: request.UserId);
        await unitOfWork.CommitAsync(cancellationToken);
        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);

        return Result.Ok(TicketDetailResult.From(ticket));
    }
}
