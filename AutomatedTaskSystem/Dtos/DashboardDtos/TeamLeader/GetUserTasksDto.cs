namespace AutomatedTaskSystem.Dtos.Dashboard.GetTeamLeaderDashboard;

public class GetUserTasksDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<GetTasksPerItemDto> TasksInLo { get; set; } = new List<GetTasksPerItemDto> { };
}
