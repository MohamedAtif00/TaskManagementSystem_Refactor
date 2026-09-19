using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.UpdateLearningObjective;

public sealed record UpdateLearningObjectiveCommand(
    int Id,
    string Name,
    string Tag,
    string Template,
    string Environment,
    DateTime? StartedAt,
    DateTime? DoneAt) : ICommand<Result<LearningObjectiveDetailResult>>;

