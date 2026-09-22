using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;
using DomainTicketStatus = TaskManagementSystem.Modules.Ticket.Domain.TicketStatus;
using DomainTicketPriority = TaskManagementSystem.Modules.Ticket.Domain.TicketPriority;

namespace TaskManagementSystem.Modules.Ticket.Features;

public sealed record TicketListItemResult(
    int Id,
    string Name,
    DomainTicketStatus Status,
    DomainTicketPriority Priority,
    int Duration,
    DateTime CreatedAt,
    int LearningObjectiveId,
    int? StepId,
    int? UserId,
    int? TeamId,
    bool Pause = false,
    bool Attention = false,
    bool Flagged = false,
    bool IsRollback = false,
    int RollbackCount = 0)
{
    public static TicketListItemResult From(Domain.Ticket task) =>
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
            task.TeamId,
            task.Pause,
            task.Attention,
            task.Flagged,
            task.IsRollback,
            task.RollbackCount);

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
            row.TeamId,
            row.Pause,
            row.Attention,
            row.Flagged,
            row.IsRollback,
            row.RollbackCount);

    internal static TicketListItemResult FromRow(SprintTicketsQueries.TicketRow row) =>
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
            row.TeamId,
            row.Pause,
            row.Attention,
            row.Flagged,
            row.IsRollback,
            row.RollbackCount);
}

public sealed record TicketListPageResult(
    IReadOnlyList<TicketListItemResult> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record TicketDetailResult(
    int Id,
    string Name,
    DomainTicketStatus Status,
    DomainTicketPriority Priority,
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
    public static TicketDetailResult From(Domain.Ticket task) =>
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
    int? TicketId)
{
    public static CommentListItemResult From(Comment comment) =>
        new(
            comment.Id,
            comment.Content,
            comment.CreatedAt,
            comment.Timestamp,
            comment.UserId,
            comment.LearningObjectiveId,
            comment.TicketId);
}

public sealed record TicketActivityListItemResult(
    int Id,
    int Type,
    string Message,
    DateTime CreatedAt,
    int? UserId)
{
    public static TicketActivityListItemResult From(TicketActivity activity) =>
        new(
            activity.Id,
            (int)activity.Type,
            activity.AdditionalInfo ?? string.Empty,
            activity.TimeStamp,
            activity.ActorOneId);
}

public sealed record JumpPointResult(int StepId, int NodeId, string Label)
{
    public static JumpPointResult From(WorkflowJumpPoint point) =>
        new(point.StepId, point.NodeId, point.Label);
}

public sealed record TicketWorkTimeResult(
    int Id,
    DateTime StartDate,
    DateTime? EndDate,
    double Duration,
    int TicketId,
    int UserId)
{
    public static TicketWorkTimeResult From(TicketWorkTime workTime) =>
        new(
            workTime.Id,
            workTime.StartDate,
            workTime.EndDate,
            workTime.Duration,
            workTime.TicketId,
            workTime.UserId);
}
