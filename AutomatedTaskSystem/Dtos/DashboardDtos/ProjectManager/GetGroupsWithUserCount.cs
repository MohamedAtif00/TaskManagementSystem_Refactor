namespace AutomatedTaskSystem.Dtos.Dashboard.GetProjectManagerDashboard;

public class GetGroupsWithUserCountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UsersCount { get; set; }
}
