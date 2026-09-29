using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure;

internal sealed class TicketActivityWriter(ITicketUnitOfWork unitOfWork) : ITicketActivityWriter
{
    public async Task WriteAsync(
        int ticketId,
        TicketActivityType type,
        int? actorOneId,
        CancellationToken cancellationToken = default,
        int? actorTwoId = null,
        string? additionalInfo = null,
        int? ticketSecondaryId = null)
    {
        var activity = TicketActivity.Create(
            ticketId,
            type,
            actorOneId,
            DateTime.UtcNow,
            actorTwoId,
            additionalInfo,
            ticketSecondaryId);
        await unitOfWork.TaskActivities.AddAsync(activity, cancellationToken);
    }
}
