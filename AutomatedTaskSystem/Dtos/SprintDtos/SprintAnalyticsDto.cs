namespace AutomatedTaskSystem.Dtos.SprintDtos
{
    /// <summary>
    /// DTO for tag/learning objective distribution data
    /// </summary>
    public class TagDataDto
    {
        public int GroupId { get; set; }
        public string Label { get; set; } = "";
        public int Value { get; set; }
        public string Color { get; set; } = "";
        public bool IsFilled { get; set; } = false;
    }

    /// <summary>
    /// DTO for task summary statistics
    /// </summary>
    public class TaskSummaryDto
    {
        public int Active { get; set; }
        public int Completed { get; set; }
        public int Rollback { get; set; }
        public int Flagged { get; set; }
        public int NotStarted { get; set; }
        public int Total { get; set; }
    }

    /// <summary>
    /// DTO for learning objectives summary
    /// </summary>
    public class LearningObjectiveSummaryDto
    {
        public int Completed { get; set; }
        public int InProcess { get; set; }
        public int NotStarted { get; set; }
        public int Total { get; set; }
    }

    /// <summary>
    /// DTO for sprint overview analytics response
    /// </summary>
    public class SprintOverviewDto
    {
        public List<TagDataDto> Tags { get; set; } = new();
        public TaskSummaryDto TaskSummary { get; set; } = new();
        public LearningObjectiveSummaryDto LoSummary { get; set; } = new();
        public TaskSummaryDto SprintSummary { get; set; } = new();
    }

    /// <summary>
    /// DTO for learning objectives progress data (for ProjectStatusChart)
    /// </summary>
    public class LearningObjectiveProgressDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Value { get; set; }
        public string Status { get; set; } = "On Track"; // "On Track" | "At Risk" | "Delayed"
        public List<CurrentPhaseDto> CurrentPhases { get; set; } = new();
    }

    /// <summary>
    /// DTO for learning objectives progress response
    /// </summary>
    public class SprintLearningObjectivesProgressDto
    {
        public List<LearningObjectiveProgressDto> Data { get; set; } = new();
    }

    /// <summary>
    /// DTO for current phase with group info
    /// </summary>
    public class CurrentPhaseDto
    {
        public string GroupName { get; set; } = "";
        public string ColorCode { get; set; } = "";
    }

    /// <summary>
    /// DTO for learning objectives table row
    /// </summary>
    public class LearningObjectiveTableRowDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Subject { get; set; } = "";
        public string StartDate { get; set; } = "";
        public int ActiveTasks { get; set; }
        public List<CurrentPhaseDto> CurrentPhases { get; set; } = new();
        public string Status { get; set; } = "Delayed"; // "On Track" | "At Risk" | "Delayed"
        public int Progress { get; set; }
    }

    /// <summary>
    /// DTO for learning objectives table response
    /// </summary>
    public class SprintLearningObjectivesTableDto
    {
        public string SprintName { get; set; } = string.Empty; // Sprint name
        public List<LearningObjectiveTableRowDto> Data { get; set; } = new();
    }
}

