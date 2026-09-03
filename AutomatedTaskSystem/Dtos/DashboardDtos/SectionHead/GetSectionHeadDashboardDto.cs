namespace AutomatedTaskSystem.Dtos.Dashboard.GetSectionHeadDashboard;

public class GetSectionHeadDashboardDto
{
    public int Projects { get; set; }
    public int Sprints { get; set; }
    public int LearningObjectives { get; set; }
    public int Users { get; set; }
    public int Teams { get; set; }
    public int ActiveTasks { get; set; }
    public List<SectionHeadTeamWorkloadDto> TeamsWorkload { get; set; } = new();
    public SectionHeadLearningObjectivesOverviewDto LearningObjectivesOverview { get; set; } =
        new();
    public List<SectionHeadSubjectOverviewDto> SubjectsOverview { get; set; } = new();
    public SectionHeadTasksOverviewDto TasksOverview { get; set; } = new();
    public List<SectionHeadProjectRowDto> ProjectsTable { get; set; } = new();
    public List<SectionHeadFlaggedRollbackDto> FlaggedRollbackTasks { get; set; } = new();
    public List<SectionHeadSprintRowDto> SprintsTable { get; set; } = new();
    public List<SectionHeadActivityDto> ActivityLog { get; set; } = new();
}

public class SectionHeadSubjectOverviewDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int ActiveTasks { get; set; }
}

public class SectionHeadTeamWorkloadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int TaskCount { get; set; }
    public double WorkloadPercent { get; set; }
}

public class SectionHeadLearningObjectivesOverviewDto
{
    public int Completed { get; set; }
    public int Uncompleted { get; set; }
    public int Total { get; set; }
}

public class SectionHeadTasksOverviewDto
{
    public int ToDo { get; set; }
    public int Doing { get; set; }
    public int Rollback { get; set; }
    public int Flagged { get; set; }
    public int Done { get; set; }
    public int Total { get; set; }
}

public class SectionHeadProjectRowDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Year { get; set; } = "";
    public string Status { get; set; } = "on_track";
    public double ProgressPercent { get; set; }
    public string Deadline { get; set; } = "";
}

public class SectionHeadSprintRowDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public string Year { get; set; } = "";
    public string Status { get; set; } = "on_track";
    public double ProgressPercent { get; set; }
    public string Deadline { get; set; } = "";
}

public class SectionHeadFlaggedRollbackDto
{
    public int TaskId { get; set; }
    public int ProjectId { get; set; }
    public string UserName { get; set; } = "";
    public string TaskName { get; set; } = "";
    public string Type { get; set; } = "";
    public string Timestamp { get; set; } = "";
}

public class SectionHeadActivityDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string Initials { get; set; } = "";
    public string Message { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}
