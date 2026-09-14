namespace TaskManagementSystem.Modules.Ticket.Application;

public sealed record LearningObjectiveSummary(int Id, int SchemaId);

public sealed record TaskBankSummary(
    int Id,
    string Name,
    int Duration,
    int TeamId,
    bool TeamLeaderOnly);

public sealed record WorkflowStepSummary(
    int Id,
    int Duration,
    int Priority,
    int NodeId,
    int TaskBankId);

public sealed record UserSummary(int Id, string Name);
