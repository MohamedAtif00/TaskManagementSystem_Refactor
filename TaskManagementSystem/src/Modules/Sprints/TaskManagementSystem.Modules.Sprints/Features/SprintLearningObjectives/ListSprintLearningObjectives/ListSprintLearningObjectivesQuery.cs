using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.SprintLearningObjectives.ListSprintLearningObjectives;

public sealed record ListSprintLearningObjectivesQuery(int SprintId) : IQuery<Result<IReadOnlyList<int>>>;

