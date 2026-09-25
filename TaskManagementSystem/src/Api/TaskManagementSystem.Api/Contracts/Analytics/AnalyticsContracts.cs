namespace TaskManagementSystem.Api.Contracts.Analytics;

public sealed class OverviewResponse
{
    public int ScopeId { get; set; }
    public string ScopeType { get; set; } = string.Empty;
    public int TotalLearningObjectives { get; set; }
    public int IdleLearningObjectives { get; set; }
    public int RunningLearningObjectives { get; set; }
    public int DoneLearningObjectives { get; set; }
    public int ProgressPercent { get; set; }
    public int BacklogTickets { get; set; }
    public int ToDoTickets { get; set; }
    public int DoingTickets { get; set; }
    public int DoneTickets { get; set; }
    public DateTime CalculatedAtUtc { get; set; }
}
