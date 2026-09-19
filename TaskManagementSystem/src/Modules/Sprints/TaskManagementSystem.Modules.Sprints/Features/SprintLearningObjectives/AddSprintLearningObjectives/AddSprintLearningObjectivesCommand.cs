using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.SprintLearningObjectives.AddSprintLearningObjectives;

public sealed record AddSprintLearningObjectivesCommand(int SprintId, IReadOnlyList<int> LearningObjectiveIds)
    : ICommand<Result<NoValue>>;

