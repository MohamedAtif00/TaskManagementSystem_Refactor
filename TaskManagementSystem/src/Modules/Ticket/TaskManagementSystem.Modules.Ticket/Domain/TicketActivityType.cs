namespace TaskManagementSystem.Modules.Ticket.Domain;

public enum TicketActivityType
{
    None = 0,
    Created = 1,
    StatusToDo = 2,
    StatusDoing = 3,
    StatusDone = 4,
    StatusRollback = 5,
    Pause = 6,
    Resume = 7,
    Flag = 8,
    Unflag = 9,
    Assign = 10,
    Comment = 11,
    Rollback = 12,
    PriorityChange = 13,
    Skip = 14,
    ProcessChange = 15,
    Jump = 16,
    ReactivateJump = 17,
    Reactivated = 18,
    EditComment = 19,
    DeleteComment = 20,
    ProcessChangeCreate = 21
}
