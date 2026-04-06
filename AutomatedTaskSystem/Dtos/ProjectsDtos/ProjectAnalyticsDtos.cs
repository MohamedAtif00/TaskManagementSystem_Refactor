using AutomatedTaskSystem.Dtos.SprintDtos;

namespace AutomatedTaskSystem.Dtos.Projects;

/// <summary>
/// Aggregate project completion based on schema-defined tasks vs done work.
/// </summary>
public class ProjectProgressDto
{
    public int ProgressPercent { get; set; }
    public int TotalExpectedTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int ActiveTasks { get; set; }
}

/// <summary>
/// Learning-side analytics: objective status breakdown and in-flight work by group (tags).
/// </summary>
public class ProjectLearningActivitiesSummaryDto
{
    public LearningObjectiveSummaryDto LoSummary { get; set; } = new();
    public List<TagDataDto> Tags { get; set; } = new();
}

/// <summary>
/// Full project analytics overview (aligned with sprint overview shape + project scope counts).
/// </summary>
public class ProjectOverviewAnalyticsDto
{
    public int NumberOfUnits { get; set; }
    public int NumberOfLessons { get; set; }
    public int NumberOfLearningObjectives { get; set; }
    public ProjectProgressDto ProjectProgress { get; set; } = new();
    public ProjectLearningActivitiesSummaryDto LearningActivitiesSummary { get; set; } = new();
    public TaskSummaryDto TasksSummary { get; set; } = new();
}

public class ProjectLearningObjectivesProgressDto
{
    public List<LearningObjectiveProgressDto> Data { get; set; } = new();
}

public class ProjectLearningObjectivesTableDto
{
    public string ProjectName { get; set; } = string.Empty;
    public List<LearningObjectiveTableRowDto> Data { get; set; } = new();
}
