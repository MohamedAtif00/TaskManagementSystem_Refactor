namespace AutomatedTaskSystem.Dtos.Dashboard.GetTeamLeaderDashboard;

public class GetTeamLeaderDashboardDto
{
    public int Projects { get; set; }
    public int Sprints { get; set; }
    public int LearningObjectives { get; set; }
    public int Users { get; set; }
    public double TeamPerformance { get; set; }
    public List<TeamLeaderMemberWorkloadDto> MembersWorkload { get; set; } = new();
    public TeamLeaderLearningObjectivesOverviewDto LearningObjectivesOverview { get; set; } =
        new();
    public TeamLeaderTasksOverviewDto TasksOverview { get; set; } = new();
    public List<TeamLeaderProjectRowDto> ProjectsTable { get; set; } = new();
    public List<TeamLeaderFlaggedRollbackDto> FlaggedRollbackTasks { get; set; } = new();
    public List<TeamLeaderSprintRowDto> SprintsTable { get; set; } = new();
    public List<TeamLeaderActivityDto> ActivityLog { get; set; } = new();
}

public class TeamLeaderMemberWorkloadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int TaskCount { get; set; }
    public double WorkloadPercent { get; set; }
}

public class TeamLeaderLearningObjectivesOverviewDto
{
    public int Completed { get; set; }
    public int Uncompleted { get; set; }
    public int Total { get; set; }
}

public class TeamLeaderTasksOverviewDto
{
    public int ToDo { get; set; }
    public int Doing { get; set; }
    public int Rollback { get; set; }
    public int Flagged { get; set; }
    public int Done { get; set; }
    public int Total { get; set; }
}

public class TeamLeaderProjectRowDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Year { get; set; } = "";
    public string Status { get; set; } = "on_track";
    public double ProgressPercent { get; set; }
    public string Deadline { get; set; } = "";
}

public class TeamLeaderSprintRowDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public string Year { get; set; } = "";
    public string Status { get; set; } = "on_track";
    public double ProgressPercent { get; set; }
    public string Deadline { get; set; } = "";
}

public class TeamLeaderFlaggedRollbackDto
{
    public int TaskId { get; set; }
    public int ProjectId { get; set; }
    public string UserName { get; set; } = "";
    public string TaskName { get; set; } = "";
    public string Type { get; set; } = "";
    public string Timestamp { get; set; } = "";
}

public class TeamLeaderActivityDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string Initials { get; set; } = "";
    public string Message { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}
