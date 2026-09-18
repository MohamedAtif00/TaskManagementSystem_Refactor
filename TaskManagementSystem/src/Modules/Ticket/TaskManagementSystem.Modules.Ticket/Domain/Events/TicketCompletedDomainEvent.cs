using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Ticket.Domain.Events;

public sealed record TicketCompletedDomainEvent(int TicketId, int AssignedUserId) : DomainEventBase;
