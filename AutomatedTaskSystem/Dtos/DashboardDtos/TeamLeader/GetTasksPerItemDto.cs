namespace AutomatedTaskSystem.Dtos.Dashboard.GetTeamLeaderDashboard;

public class GetTasksPerItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TasksCount { get; set; }
}
