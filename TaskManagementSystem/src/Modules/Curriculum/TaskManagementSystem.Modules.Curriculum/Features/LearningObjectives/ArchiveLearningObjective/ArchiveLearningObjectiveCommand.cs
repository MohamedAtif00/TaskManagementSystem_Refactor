using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.ArchiveLearningObjective;

public sealed record ArchiveLearningObjectiveCommand(int Id) : ICommand<Result<NoValue>>;

