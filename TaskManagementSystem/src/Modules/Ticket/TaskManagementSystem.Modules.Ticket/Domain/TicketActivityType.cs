namespace TaskManagementSystem.Modules.Ticket.Domain;

public enum TicketActivityType
{
    Created = 1,
    Assigned = 2,
    StatusChange = 3,
    Completed = 4,
    Flagged = 5,
    Unflagged = 6,
    Rollback = 7,
    Comment = 8,
    PriorityChange = 9,
    Skip = 10,
    Jump = 11,
    Proceed = 12,
}
