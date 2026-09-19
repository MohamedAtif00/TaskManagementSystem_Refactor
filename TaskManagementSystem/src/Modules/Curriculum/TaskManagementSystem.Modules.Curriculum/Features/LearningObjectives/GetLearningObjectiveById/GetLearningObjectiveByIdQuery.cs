using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.GetLearningObjectiveById;

public sealed record GetLearningObjectiveByIdQuery(int Id) : IQuery<Result<LearningObjectiveDetailResult>>;

