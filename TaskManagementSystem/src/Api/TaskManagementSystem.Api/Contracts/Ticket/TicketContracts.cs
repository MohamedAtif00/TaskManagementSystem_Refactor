using TaskManagementSystem.Modules.Ticket.Domain;
using DomainTaskStatus = TaskManagementSystem.Modules.Ticket.Domain.TaskStatus;
using DomainTaskPriority = TaskManagementSystem.Modules.Ticket.Domain.TaskPriority;

namespace TaskManagementSystem.Api.Contracts.Ticket;

public sealed class CreateTicketRequest
{
    public int LearningObjectiveId { get; set; }
    public int TaskBankItemId { get; set; }
    public int? UserId { get; set; }
}

public sealed class AssignTicketRequest
{
    public int UserId { get; set; }
}

public sealed class AddCommentRequest
{
    public string Content { get; set; } = string.Empty;
}

public sealed class TicketListItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DomainTaskStatus Status { get; set; }
    public DomainTaskPriority Priority { get; set; }
    public int Duration { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LearningObjectiveId { get; set; }
    public int? StepId { get; set; }
    public int? UserId { get; set; }
    public int? TeamId { get; set; }
    public bool Pause { get; set; }
    public bool Attention { get; set; }
    public bool Flagged { get; set; }
    public bool IsRollback { get; set; }
    public int RollbackCount { get; set; }
}

public sealed class TicketListPageResponse
{
    public IReadOnlyList<TicketListItemResponse> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public sealed class TicketStatsTicketResponse
{
    public int Id { get; set; }
    public int Status { get; set; }
    public int? UserId { get; set; }
    public int LearningObjectiveId { get; set; }
    public int SubjectId { get; set; }
}

public sealed class TicketStatsLearningObjectiveResponse
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
}

public sealed class TicketStatsResponse
{
    public IReadOnlyList<TicketStatsTicketResponse> Tickets { get; set; } = [];
    public IReadOnlyList<TicketStatsLearningObjectiveResponse> LearningObjectives { get; set; } = [];
}

public sealed class TicketDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DomainTaskStatus Status { get; set; }
    public DomainTaskPriority Priority { get; set; }
    public int Duration { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Pause { get; set; }
    public bool Attention { get; set; }
    public bool Flagged { get; set; }
    public bool Tl { get; set; }
    public bool IsReview { get; set; }
    public bool IsRollback { get; set; }
    public int RollbackCount { get; set; }
    public int LearningObjectiveId { get; set; }
    public int? StepId { get; set; }
    public int? UserId { get; set; }
    public int? TeamId { get; set; }
    public int? FromId { get; set; }
}

public sealed class CommentListItemResponse
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime Timestamp { get; set; }
    public int UserId { get; set; }
    public int LearningObjectiveId { get; set; }
    public int? TaskId { get; set; }
}

public sealed class TaskWorkTimeResponse
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double Duration { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
}
