using TaskManagementSystem.Modules.Sprints.Domain;

namespace TaskManagementSystem.Modules.Sprints.Features;

public sealed record SprintListItemResult(
    int Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate)
{
    public static SprintListItemResult From(Sprint sprint) =>
        new(sprint.Id, sprint.Name, sprint.Description, sprint.StartDate, sprint.EndDate);
}

public sealed record SprintDetailResult(
    int Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    IReadOnlyList<int> LearningObjectiveIds)
{
    public static SprintDetailResult From(Sprint sprint, IReadOnlyList<int> learningObjectiveIds) =>
        new(sprint.Id, sprint.Name, sprint.Description, sprint.StartDate, sprint.EndDate, learningObjectiveIds);
}
