using TaskManagementSystem.Modules.Ticket.Domain;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;
using DomainTaskStatus = TaskManagementSystem.Modules.Ticket.Domain.TaskStatus;
using DomainTaskPriority = TaskManagementSystem.Modules.Ticket.Domain.TaskPriority;

namespace TaskManagementSystem.Modules.Ticket.Features;

public sealed record TicketListItemResult(
    int Id,
    string Name,
    DomainTaskStatus Status,
    DomainTaskPriority Priority,
    int Duration,
    DateTime CreatedAt,
    int LearningObjectiveId,
    int? StepId,
    int? UserId,
    int? TeamId)
{
    public static TicketListItemResult From(TicketTask task) =>
        new(
            task.Id,
            task.Name,
            task.Status,
            task.Priority,
            task.Duration,
            task.CreatedAt,
            task.LearningObjectiveId,
            task.StepId,
            task.UserId,
            task.TeamId);

    internal static TicketListItemResult FromRow(SubjectTicketsQueries.TicketRow row) =>
        new(
            row.Id,
            row.Name,
            row.Status,
            row.Priority,
            row.Duration,
            row.CreatedAt,
            row.LearningObjectiveId,
            row.StepId,
            row.UserId,
            row.TeamId);
}

public sealed record TicketDetailResult(
    int Id,
    string Name,
    DomainTaskStatus Status,
    DomainTaskPriority Priority,
    int Duration,
    DateTime CreatedAt,
    bool Pause,
    bool Attention,
    bool Flagged,
    bool Tl,
    bool IsReview,
    bool IsRollback,
    int RollbackCount,
    int LearningObjectiveId,
    int? StepId,
    int? UserId,
    int? TeamId,
    int? FromId)
{
    public static TicketDetailResult From(TicketTask task) =>
        new(
            task.Id,
            task.Name,
            task.Status,
            task.Priority,
            task.Duration,
            task.CreatedAt,
            task.Pause,
            task.Attention,
            task.Flagged,
            task.Tl,
            task.IsReview,
            task.IsRollback,
            task.RollbackCount,
            task.LearningObjectiveId,
            task.StepId,
            task.UserId,
            task.TeamId,
            task.FromId);
}

public sealed record CommentListItemResult(
    int Id,
    string Content,
    DateTime CreatedAt,
    DateTime Timestamp,
    int UserId,
    int LearningObjectiveId,
    int? TaskId)
{
    public static CommentListItemResult From(Comment comment) =>
        new(
            comment.Id,
            comment.Content,
            comment.CreatedAt,
            comment.Timestamp,
            comment.UserId,
            comment.LearningObjectiveId,
            comment.TaskId);
}

public sealed record TaskWorkTimeResult(
    int Id,
    DateTime StartDate,
    DateTime? EndDate,
    double Duration,
    int TaskId,
    int UserId)
{
    public static TaskWorkTimeResult From(TaskWorkTime workTime) =>
        new(
            workTime.Id,
            workTime.StartDate,
            workTime.EndDate,
            workTime.Duration,
            workTime.TaskId,
            workTime.UserId);
}
