namespace AutomatedTaskSystem.Dtos.Dashboard.GetTeamLeaderDashboard;

public class GetTeamLeaderDashboardDto
{
    public int Members { get; set; }
    public int ActiveTasks { get; set; }
    public int Projects { get; set; }
    public List<GetTasksPerItemDto> ProjectsDetails { get; set; } =
        new List<GetTasksPerItemDto> { };
    public List<GetTasksPerItemDto> TasksPerUser { get; set; } = new List<GetTasksPerItemDto> { };
}
