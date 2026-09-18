using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.IntegrationEvents.Ticket;

public sealed record TicketCompletedIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    int TicketId,
    int AssignedUserId) : IIntegrationEvent;
