using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Ticket.Domain.Events;

public sealed record TicketAssignedDomainEvent(int TicketId, int AssignedUserId) : DomainEventBase;
