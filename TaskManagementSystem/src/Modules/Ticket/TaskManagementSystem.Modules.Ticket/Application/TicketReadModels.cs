namespace TaskManagementSystem.Modules.Ticket.Application;

public sealed record LearningObjectiveSummary(int Id, int SchemaId);

public sealed record TicketBankSummary(
    int Id,
    string Name,
    int Duration,
    int TeamId,
    bool TeamLeaderOnly,
    int Type = 0);

public sealed record ActiveUserRecord(int Id, int? TeamId);

public sealed record WorkflowStepSummary(
    int Id,
    int Duration,
    int Priority,
    int NodeId,
    int TicketBankId);

public sealed record UserSummary(int Id, string Name);
