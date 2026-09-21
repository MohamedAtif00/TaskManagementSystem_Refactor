using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Ticket.Application;

public static class TicketErrors
{
    public static ResultError TicketNotFound =>
        new("ticket_not_found", "Ticket not found.");

    public static ResultError CommentNotFound =>
        new("comment_not_found", "Comment not found.");

    public static ResultError WorkTimeNotFound =>
        new("work_time_not_found", "Work time not found.");

    public static ResultError LearningObjectiveNotFound =>
        new("learning_objective_not_found", "Learning objective not found.");

    public static ResultError TaskBankNotFound =>
        new("task_bank_not_found", "Task bank item not found.");

    public static ResultError StepNotFound =>
        new("step_not_found", "Workflow step not found.");

    public static ResultError UserNotFound =>
        new("user_not_found", "User not found.");

    public static ResultError TeamInvalid =>
        new("team_invalid", "Team is invalid.");

    public static ResultError WorkTimeAlreadyOpen =>
        new("work_time_already_open", "An open work time already exists for this user on this ticket.");

    public static ResultError TicketCannotRollback =>
        new("ticket_cannot_rollback", "Backlog tickets cannot be rolled back.");
}
