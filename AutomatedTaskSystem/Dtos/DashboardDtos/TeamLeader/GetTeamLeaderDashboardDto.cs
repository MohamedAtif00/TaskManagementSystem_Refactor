namespace AutomatedTaskSystem.Dtos.Dashboard.GetTeamLeaderDashboard;

public class GetTeamLeaderDashboardDto
{
    public int Members { get; set; }
    public int ActiveTasks { get; set; }
    public int Projects { get; set; }
    public List<GetTasksPerItem> ProjectsDetails { get; set; } = new List<GetTasksPerItem> { };
    public List<GetTasksPerItem> TasksPerUser { get; set; } = new List<GetTasksPerItem> { };
}
