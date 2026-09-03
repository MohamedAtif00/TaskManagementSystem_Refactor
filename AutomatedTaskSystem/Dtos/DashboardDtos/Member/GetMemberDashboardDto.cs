namespace AutomatedTaskSystem.Dtos.Dashboard.GetMemberDashboard;

public class GetMemberDashboardDto
{
    public int Projects { get; set; }
    public int ActiveProjects { get; set; }
    public int Sprints { get; set; }
    public int LearningObjectives { get; set; }
    public int LearningObjectivesThisMonth { get; set; }
    public double TeamPerformance { get; set; }
    public int ActiveTasks { get; set; }
    public MemberLearningObjectivesOverviewDto LearningObjectivesOverview { get; set; } =
        new MemberLearningObjectivesOverviewDto();
    public List<MemberSubjectOverviewDto> SubjectsOverview { get; set; } = new();
    public MemberTasksOverviewDto TasksOverview { get; set; } = new MemberTasksOverviewDto();
    public List<MemberTaskBoardItemDto> ToDoTasks { get; set; } = new();
    public List<MemberTaskBoardItemDto> InProgressTasks { get; set; } = new();
    public List<MemberTaskBoardItemDto> DoneTasks { get; set; } = new();
    public List<MemberSprintDeadlineDto> SprintDeadlines { get; set; } = new();
    public List<MemberEscalatedTaskDto> EscalatedTasks { get; set; } = new();
    public List<MemberWorkUpdateDto> WorkUpdates { get; set; } = new();
}

public class MemberSubjectOverviewDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int ActiveTasks { get; set; }
}

public class MemberLearningObjectivesOverviewDto
{
    public int Completed { get; set; }
    public int Uncompleted { get; set; }
    public int Total { get; set; }
}

public class MemberTasksOverviewDto
{
    public int ToDo { get; set; }
    public int Doing { get; set; }
    public int Rollback { get; set; }
    public int Flagged { get; set; }
    public int Done { get; set; }
    public int Total { get; set; }
}

public class MemberTaskBoardItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public int ProjectId { get; set; }
    public int Priority { get; set; }
    public string? DueDate { get; set; }
    public bool IsDueToday { get; set; }
    public bool IsCompleted { get; set; }
}

public class MemberSprintDeadlineDto
{
    public int SprintId { get; set; }
    public string SprintName { get; set; } = "";
    public string TaskName { get; set; } = "";
    public int DaysLeft { get; set; }
    public double ProgressPercent { get; set; }
    public string DeadlineDate { get; set; } = "";
    public string Urgency { get; set; } = "normal";
}

public class MemberEscalatedTaskDto
{
    public int TaskId { get; set; }
    public string TaskName { get; set; } = "";
    public string EscalatedBy { get; set; } = "";
    public string EscalatedAt { get; set; } = "";
    public int ProjectId { get; set; }
}

public class MemberWorkUpdateDto
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public string Message { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}
