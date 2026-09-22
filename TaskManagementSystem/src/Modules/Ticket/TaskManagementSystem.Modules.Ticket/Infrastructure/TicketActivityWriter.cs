using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure;

internal sealed class TicketActivityWriter(ITicketUnitOfWork unitOfWork) : ITicketActivityWriter
{
    public async Task WriteAsync(
        int ticketId,
        TicketActivityType type,
        string message,
        int? actorUserId,
        CancellationToken cancellationToken = default)
    {
        var activity = TicketActivity.Create(ticketId, type, message, actorUserId, DateTime.UtcNow);
        await unitOfWork.TaskActivities.AddAsync(activity, cancellationToken);
    }
}
