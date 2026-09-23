using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Application;

public static class TicketActivityText
{
    public static string Describe(
        TicketActivityType type,
        string taskName,
        string? actorOneName,
        string? actorTwoName,
        string? secondaryTaskName,
        string? additionalInfo)
    {
        var actor = NameOr(actorOneName, "User");
        var jumpActor = NameOr(actorOneName, "a User");
        var assignee = NameOr(actorTwoName, "no one");
        var previous = NameOr(secondaryTaskName, "a previous task");
        var rolledFrom = NameOr(secondaryTaskName, "User");
        var priority = string.IsNullOrWhiteSpace(additionalInfo) ? "None" : additionalInfo.Trim();
        var title = string.IsNullOrWhiteSpace(taskName) ? "Task" : taskName.Trim();

        return type switch
        {
            TicketActivityType.Created => actorOneName is null
                ? $"{title} was created."
                : $"{title} was created by {actor}.",
            TicketActivityType.StatusToDo => $"{actor} added task to their To Do list.",
            TicketActivityType.StatusDoing => $"Task started by {actor}.",
            TicketActivityType.StatusDone => $"Task completed by {actor}.",
            TicketActivityType.StatusRollback => $"{actor} rolled back Task to {previous}.",
            TicketActivityType.Pause => $"{actor} paused the task.",
            TicketActivityType.Resume => $"{actor} resumed the task.",
            TicketActivityType.Flag => FlagSentence(actor, actorTwoName, additionalInfo),
            TicketActivityType.Unflag => $"{actor} cleared flag.",
            TicketActivityType.Assign => $"{actor} assigned the task to {assignee}.",
            TicketActivityType.Comment => $"{actor} left a comment.",
            TicketActivityType.Rollback => $"Task was rolled back from {rolledFrom} by {actor}.",
            TicketActivityType.PriorityChange => $"{actor} updated task priority to {priority}.",
            TicketActivityType.Skip => $"Task was skipped by {actor}.",
            TicketActivityType.ProcessChange => "Task was completed due to process change.",
            TicketActivityType.Jump => $"Task was skipped due to a jump by {jumpActor}.",
            TicketActivityType.ReactivateJump => $"Task was reactivated due to a jump by {jumpActor}.",
            TicketActivityType.Reactivated => "Task was reactivated due to a previous task completion.",
            TicketActivityType.EditComment => $"{actor} edited a comment.",
            TicketActivityType.DeleteComment => $"{actor} removed a comment.",
            _ => string.Empty
        };
    }

    private static string FlagSentence(string actor, string? actorTwoName, string? additionalInfo)
    {
        var flaggedFor = string.IsNullOrWhiteSpace(actorTwoName) ? string.Empty : $" for {actorTwoName.Trim()}";
        var sentence = $"{actor} flagged the task{flaggedFor}.";
        if (string.IsNullOrWhiteSpace(additionalInfo))
        {
            return sentence;
        }

        return $"{sentence} \"{additionalInfo.Trim()}\"";
    }

    private static string NameOr(string? name, string fallback) =>
        string.IsNullOrWhiteSpace(name) ? fallback : name.Trim();
}
