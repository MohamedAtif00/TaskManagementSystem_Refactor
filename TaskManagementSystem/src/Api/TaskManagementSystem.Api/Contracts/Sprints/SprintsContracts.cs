namespace TaskManagementSystem.Api.Contracts.Sprints;

public sealed class CreateSprintRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class UpdateSprintRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class SprintListItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int LearningObjectiveCount { get; set; }
    public int ProgressPercent { get; set; }
}

public sealed class SprintListPageResponse
{
    public IReadOnlyList<SprintListItemResponse> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public sealed class SprintDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public IReadOnlyList<int> LearningObjectiveIds { get; set; } = [];
}

public sealed class AddSprintLearningObjectivesRequest
{
    public IReadOnlyList<int> LearningObjectiveIds { get; set; } = [];
}
