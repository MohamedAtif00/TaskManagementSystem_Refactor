using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.CreateLearningObjective;

public sealed record CreateLearningObjectiveCommand(
    int LessonId,
    int SchemaId,
    string Name,
    string Tag,
    string Template,
    string Environment) : ICommand<Result<LearningObjectiveDetailResult>>;

