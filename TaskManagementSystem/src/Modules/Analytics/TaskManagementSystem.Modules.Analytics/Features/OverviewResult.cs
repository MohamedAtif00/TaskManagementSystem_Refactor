namespace TaskManagementSystem.Modules.Analytics.Features;

public sealed record OverviewResult(
    int ScopeId,
    string ScopeType,
    int TotalLearningObjectives,
    int IdleLearningObjectives,
    int RunningLearningObjectives,
    int DoneLearningObjectives,
    int ProgressPercent,
    int BacklogTickets,
    int ToDoTickets,
    int DoingTickets,
    int DoneTickets,
    DateTime CalculatedAtUtc);
