using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Ticket.Application;

public static class TicketErrors
{
    public static ResultError TicketNotFound =>
        new("ticket_not_found", "Domain.Ticket not found.");

    public static ResultError CommentNotFound =>
        new("comment_not_found", "Comment not found.");

    public static ResultError WorkTimeNotFound =>
        new("work_time_not_found", "Work time not found.");

    public static ResultError LearningObjectiveNotFound =>
        new("learning_objective_not_found", "Learning objective not found.");

    public static ResultError SubjectNotFound =>
        new("subject_not_found", "Subject not found.");

    public static ResultError SprintNotFound =>
        new("sprint_not_found", "Sprint not found.");

    public static ResultError TicketBankNotFound =>
        new("ticket_bank_not_found", "Domain.Ticket bank item not found.");

    public static ResultError StepNotFound =>
        new("step_not_found", "Workflow step not found.");

    public static ResultError UserNotFound =>
        new("user_not_found", "User not found.");

    public static ResultError TeamInvalid =>
        new("team_invalid", "Team is invalid.");

    public static ResultError WorkTimeAlreadyOpen =>
        new("work_time_already_open", "An open work time already exists for this user on this ticket.");

    public static ResultError TicketCannotRollback =>
        new("ticket_cannot_rollback", "Only a review task in Doing can be rolled back.");

    public static ResultError TicketUnauthorized =>
        new("ticket_unauthorized", "You cannot perform this action.");

    public static ResultError TicketFlagged =>
        new("ticket_flagged", "A flagged task cannot be worked on.");

    public static ResultError AssigneeTeamMismatch =>
        new("ticket_assignee_team", "Cannot assign a user from another team.");
}
