using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Modules.Ticket.Features;

namespace TaskManagementSystem.Api.Endpoints.Ticket;

internal static class TicketMapping
{
    internal static TicketListItemResponse MapTicketListItem(TicketListItemResult ticket) =>
        new()
        {
            Id = ticket.Id,
            Name = ticket.Name,
            Status = ticket.Status,
            Priority = ticket.Priority,
            Duration = ticket.Duration,
            CreatedAt = ticket.CreatedAt,
            LearningObjectiveId = ticket.LearningObjectiveId,
            StepId = ticket.StepId,
            UserId = ticket.UserId,
            TeamId = ticket.TeamId
        };

    internal static TicketDetailResponse MapTicketDetail(TicketDetailResult ticket) =>
        new()
        {
            Id = ticket.Id,
            Name = ticket.Name,
            Status = ticket.Status,
            Priority = ticket.Priority,
            Duration = ticket.Duration,
            CreatedAt = ticket.CreatedAt,
            Pause = ticket.Pause,
            Attention = ticket.Attention,
            Flagged = ticket.Flagged,
            Tl = ticket.Tl,
            IsReview = ticket.IsReview,
            IsRollback = ticket.IsRollback,
            RollbackCount = ticket.RollbackCount,
            LearningObjectiveId = ticket.LearningObjectiveId,
            StepId = ticket.StepId,
            UserId = ticket.UserId,
            TeamId = ticket.TeamId,
            FromId = ticket.FromId
        };

    internal static CommentListItemResponse MapComment(CommentListItemResult comment) =>
        new()
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            Timestamp = comment.Timestamp,
            UserId = comment.UserId,
            LearningObjectiveId = comment.LearningObjectiveId,
            TaskId = comment.TaskId
        };

    internal static TaskWorkTimeResponse MapWorkTime(TaskWorkTimeResult workTime) =>
        new()
        {
            Id = workTime.Id,
            StartDate = workTime.StartDate,
            EndDate = workTime.EndDate,
            Duration = workTime.Duration,
            TaskId = workTime.TaskId,
            UserId = workTime.UserId
        };
}
