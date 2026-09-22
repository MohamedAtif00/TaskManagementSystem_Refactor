using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Contracts;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features;

internal static class TicketRealtimeNotifier
{
    public static Task PublishUpdateAsync(
        IRealtimePublisher publisher,
        Domain.Ticket ticket,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            TicketId = ticket.Id,
            Title = ticket.Name,
            Status = ticket.Status.ToString()
        };

        return publisher.PublishToGroupAsync(
            GroupName(ticket.LearningObjectiveId),
            TicketRealtime.SilentUpdate(payload),
            cancellationToken);
    }

    internal static string GroupName(int learningObjectiveId) => $"tickets-lo-{learningObjectiveId}";
}
