using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Application;

public static class TicketWorkClock
{
    public static async Task CloseOpenAsync(
        ITicketUnitOfWork unitOfWork,
        int ticketId,
        CancellationToken cancellationToken)
    {
        var open = await unitOfWork.TicketWorkTimes.ListOpenByTicketTrackedAsync(ticketId, cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var workTime in open)
        {
            workTime.Stop(now);
        }
    }

    public static async Task OpenAsync(
        ITicketUnitOfWork unitOfWork,
        int ticketId,
        int userId,
        CancellationToken cancellationToken)
    {
        var existing = await unitOfWork.TicketWorkTimes.GetOpenByTicketAndUserTrackedAsync(
            ticketId,
            userId,
            cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var created = TicketWorkTime.Start(ticketId, userId, DateTime.UtcNow);
        if (created.IsSuccess)
        {
            await unitOfWork.TicketWorkTimes.AddAsync(created.Value, cancellationToken);
        }
    }
}
