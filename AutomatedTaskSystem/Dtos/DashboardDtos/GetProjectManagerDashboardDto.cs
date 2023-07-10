using AutomatedTaskSystem.Dtos.Report;

namespace AutomatedTaskSystem.Dtos.Dashboard.GetProjectManagerDashboard;

public class GetProjectManagerDashboardDto
{
    public List<GetReportDto> ProjectsReport { get; set; } = new List<GetReportDto> { };
    public List<GetGroupsWithUserCountDto> GroupsCount { get; set; } =
        new List<GetGroupsWithUserCountDto> { };
    public int NumberOfProject { get; set; }
    public int NumberOfUsers { get; set; }
    public int NumberOfSchemas { get; set; }
    public int NumberOfActiveTasks { get; set; }
}
