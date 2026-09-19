using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.SprintLearningObjectives.RemoveSprintLearningObjective;

public sealed record RemoveSprintLearningObjectiveCommand(int SprintId, int LearningObjectiveId)
    : ICommand<Result<NoValue>>;

