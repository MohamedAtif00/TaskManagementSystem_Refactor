using TaskManagementSystem.Modules.Ticket.Domain;
using DomainTicketStatus = TaskManagementSystem.Modules.Ticket.Domain.TicketStatus;
using DomainTicketPriority = TaskManagementSystem.Modules.Ticket.Domain.TicketPriority;

namespace TaskManagementSystem.Api.Contracts.Ticket;

public sealed class CreateTicketRequest
{
    public int LearningObjectiveId { get; set; }
    public int TicketBankItemId { get; set; }
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
    public DomainTicketStatus Status { get; set; }
    public DomainTicketPriority Priority { get; set; }
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
    public DomainTicketStatus Status { get; set; }
    public DomainTicketPriority Priority { get; set; }
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

public sealed class CommentListPageResponse
{
    public IReadOnlyList<CommentListItemResponse> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public sealed class TicketActivityListPageResponse
{
    public IReadOnlyList<TicketActivityResponse> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public sealed class CommentListItemResponse
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime Timestamp { get; set; }
    public int UserId { get; set; }
    public int LearningObjectiveId { get; set; }
    public int? TicketId { get; set; }
}

public sealed class UpdateTicketPriorityRequest
{
    public int Priority { get; set; }
}

public sealed class JumpTicketRequest
{
    public int StepId { get; set; }
}

public sealed class JumpPointResponse
{
    public int StepId { get; set; }
    public int NodeId { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class TicketActivityResponse
{
    public int Id { get; set; }
    public int Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? UserId { get; set; }
}

public sealed class TicketWorkTimeResponse
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double Duration { get; set; }
    public int TicketId { get; set; }
    public int UserId { get; set; }
}
