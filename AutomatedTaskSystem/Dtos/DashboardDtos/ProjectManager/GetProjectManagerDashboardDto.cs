namespace AutomatedTaskSystem.Dtos.Dashboard.GetProjectManagerDashboard;

public class GetProjectManagerDashboardDto
{
    public int Projects { get; set; }
    public int Sprints { get; set; }
    public int LearningObjectives { get; set; }
    public int Users { get; set; }
    public List<ProjectManagerTeamWorkloadDto> TeamsWorkload { get; set; } = new();
    public ProjectManagerLearningObjectivesOverviewDto LearningObjectivesOverview { get; set; } =
        new();
    public ProjectManagerTasksOverviewDto TasksOverview { get; set; } = new();
    public List<ProjectManagerProjectRowDto> ProjectsTable { get; set; } = new();
    public List<ProjectManagerFlaggedRollbackDto> FlaggedRollbackTasks { get; set; } = new();
    public List<ProjectManagerSprintRowDto> SprintsTable { get; set; } = new();
    public List<ProjectManagerActivityDto> ActivityLog { get; set; } = new();
}

public class ProjectManagerTeamWorkloadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int TaskCount { get; set; }
    public double WorkloadPercent { get; set; }
}

public class ProjectManagerLearningObjectivesOverviewDto
{
    public int Completed { get; set; }
    public int Uncompleted { get; set; }
    public int Total { get; set; }
}

public class ProjectManagerTasksOverviewDto
{
    public int ToDo { get; set; }
    public int Doing { get; set; }
    public int Rollback { get; set; }
    public int Flagged { get; set; }
    public int Done { get; set; }
    public int Total { get; set; }
}

public class ProjectManagerProjectRowDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Year { get; set; } = "";
    public string Status { get; set; } = "on_track";
    public double ProgressPercent { get; set; }
    public string Deadline { get; set; } = "";
}

public class ProjectManagerSprintRowDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public string Year { get; set; } = "";
    public string Status { get; set; } = "on_track";
    public double ProgressPercent { get; set; }
    public string Deadline { get; set; } = "";
}

public class ProjectManagerFlaggedRollbackDto
{
    public int TaskId { get; set; }
    public int ProjectId { get; set; }
    public string UserName { get; set; } = "";
    public string TaskName { get; set; } = "";
    public string Type { get; set; } = "";
    public string Timestamp { get; set; } = "";
}

public class ProjectManagerActivityDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string Initials { get; set; } = "";
    public string Message { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}
